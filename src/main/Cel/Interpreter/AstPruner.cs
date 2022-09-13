using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

/*
 * Copyright (C) 2022 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Cel.Interpreter;

// TODO Consider having a separate walk of the AST that finds common
//  subexpressions. This can be called before or after constant folding to find
//  common subexpressions.

/// <summary>
///     PruneAst prunes the given AST based on the given EvalState and generates a new AST. Given AST is
///     copied on write and a new AST is returned.
///     <para>
///         Couple of typical use cases this interface would be:
///         <ol>
///             <li>
///                 <ol>
///                     <li>
///                         Evaluate expr with some unknowns,
///                     </li>
///                     <li>
///                             If result is unknown:
///                          <ol>
///                              <li>
///                                  PruneAst
///                              </li>
///                              <li>
///                                  Goto 1
///                              </li>
///                          </ol>
///                          Functional call results which are known would be effectively cached across
///                          iterations.
///                     </li>
///                 </ol>
///             </li>
///             <li>
///                 <ol>
///                     <li>
///                          Compile the expression (maybe via a service and maybe after checking a compiled
///                          expression does not exists in local cache)
///                     </li>
///                     <li>
///                          Prepare the environment and the interpreter. Activation might be empty.
///                     </li>
///                     <li>
///                          Eval the expression. This might return unknown or error or a concrete value.
///                     </li>
///                     <li>
///                          PruneAst
///                     </li>
///                     <li>
///                          Maybe cache the expression
///                     </li>
///                 </ol>
///             </li>
///         </ol>
///     </para>
///     <para>
///         This is effectively constant folding the expression. How the environment is prepared in step 2
///         is flexible. For example, If the caller caches the compiled and constant folded expressions, but
///         is not willing to constant fold(and thus cache results of) some external calls, then they can
///         prepare the overloads accordingly.
///     </para>
/// </summary>
public sealed class AstPruner
{
    private readonly Expr expr;
    private readonly IEvalState state;
    private long nextExprId;

    private AstPruner(Expr expr, IEvalState state, long nextExprId)
    {
        this.expr = expr;
        this.state = state;
        this.nextExprId = nextExprId;
    }

    public static Expr? PruneAst(Expr expr, IEvalState state)
    {
        var pruner = new AstPruner(expr, state, 1);
        var newExpr = pruner.Prune(expr);
        return newExpr;
    }

    internal static Expr CreateLiteral(long id, Constant val)
    {
        var expr = new Expr();
        expr.Id = id;
        expr.ConstExpr = val;
        return expr;
    }

    internal Expr? MaybeCreateLiteral(long id, IVal v)
    {
        var constant = new Constant();
        var t = v.Type();
        switch (t.TypeEnum().InnerEnumValue)
        {
            case TypeEnum.InnerEnum.Bool:
                constant.BoolValue = (bool)v.Value();
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.Int:
                constant.Int64Value = (long)v.Value();
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.Uint:
                constant.Uint64Value = (ulong)v.Value();
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.String:
                constant.StringValue = v.Value().ToString();
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.Double:
                constant.DoubleValue = (double)v.Value();
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.Bytes:
                constant.BytesValue = ByteString.CopyFrom((byte[])v.Value());
                return CreateLiteral(id, constant);
            case TypeEnum.InnerEnum.Null:
                constant.NullValue = NullValue.NullValue;
                return CreateLiteral(id, constant);
        }

        // Attempt to build a list literal.
        if (v is ILister)
        {
            var list = (ILister)v;
            var sz = (int)list.Size().IntValue();
            IList<Expr> elemExprs = new List<Expr>(sz);
            for (var i = 0; i < sz; i++)
            {
                var elem = list.Get(IntT.IntOf(i));
                if (Util.IsUnknownOrError(elem)) return null;

                var elemExpr = MaybeCreateLiteral(NextId(), elem);
                if (elemExpr == null) return null;

                elemExprs.Add(elemExpr);
            }

            var createList = new Expr.Types.CreateList();
            createList.Elements.Add(elemExprs);
            var expr = new Expr();
            expr.Id = id;
            expr.ListExpr = createList;
            return expr;
        }

        // Create a map literal if possible.
        if (v is IMapper)
        {
            var mp = (IMapper)v;
            var it = mp.Iterator();
            IList<Expr.Types.CreateStruct.Types.Entry> entries =
                new List<Expr.Types.CreateStruct.Types.Entry>((int)mp.Size().IntValue());
            while (it.HasNext() == BoolT.True)
            {
                var key = it.Next();
                var val = mp.Get(key);
                if (Util.IsUnknownOrError(key) || Util.IsUnknownOrError(val)) return null;

                var keyExpr = MaybeCreateLiteral(NextId(), key);
                if (keyExpr == null) return null;

                var valExpr = MaybeCreateLiteral(NextId(), val);
                if (valExpr == null) return null;

                var entry = new Expr.Types.CreateStruct.Types.Entry();
                entry.Id = id;
                entry.MapKey = keyExpr;
                entry.Value = valExpr;
                entries.Add(entry);
            }

            var createStruct = new Expr.Types.CreateStruct();
            createStruct.Entries.Add(entries);
            var expr = new Expr();
            expr.StructExpr = createStruct;
            return expr;
        }

        // TODO(issues/377) To construct message literals, the type provider will need to support
        //  the enumeration the fields for a given message.
        return null;
    }

    internal Expr? MaybePruneAndOr(Expr node)
    {
        if (!ExistsWithUnknownValue(node.Id)) return null;

        var call = node.CallExpr;
        // We know result is unknown, so we have at least one unknown arg
        // and if one side is a known value, we know we can ignore it.
        if (ExistsWithKnownValue(call.Args[0].Id)) return call.Args[1];

        if (ExistsWithKnownValue(call.Args[1].Id)) return call.Args[0];

        return null;
    }

    internal Expr? MaybePruneConditional(Expr node)
    {
        if (!ExistsWithUnknownValue(node.Id)) return null;

        var call = node.CallExpr;
        var condVal = Value(call.Args[0].Id);
        if (condVal == null || Util.IsUnknownOrError(condVal)) return null;

        if (condVal == BoolT.True) return call.Args[1];

        return call.Args[2];
    }

    internal Expr? MaybePruneFunction(Expr node)
    {
        var call = node.CallExpr;
        if (call.Function.Equals(Operator.LogicalOr.Id) || call.Function.Equals(Operator.LogicalAnd.Id))
            return MaybePruneAndOr(node);

        if (call.Function.Equals(Operator.Conditional.Id)) return MaybePruneConditional(node);

        return null;
    }

    internal Expr? Prune(Expr? node)
    {
        if (node == null) return null;

        var val = Value(node.Id);
        if (val != null && !Util.IsUnknownOrError(val))
        {
            var newNode = MaybeCreateLiteral(node.Id, val);
            if (newNode != null) return newNode;
        }

        // We have either an unknown/error value, or something we dont want to
        // transform, or expression was not evaluated. If possible, drill down
        // more.

        switch (node.ExprKindCase)
        {
            case Expr.ExprKindOneofCase.SelectExpr:
                var select = node.SelectExpr;
                var operand = Prune(select.Operand);
                if (operand != null && operand != select.Operand)
                {
                    var sel = new Expr.Types.Select();
                    sel.Operand = operand;
                    sel.Field = select.Field;
                    sel.TestOnly = select.TestOnly;
                    var expr = new Expr();
                    expr.Id = nextExprId;
                    expr.SelectExpr = sel;
                    return expr;
                }

                break;
            case Expr.ExprKindOneofCase.CallExpr:
                var call = node.CallExpr;
                var newExpr = MaybePruneFunction(node);
                if (newExpr != null)
                {
                    newExpr = Prune(newExpr);
                    return newExpr;
                }

                var prunedCall = false;
                IList<Expr> args = call.Args;
                IList<Expr> newArgs = new List<Expr>(args.Count);
                for (var i = 0; i < args.Count; i++)
                {
                    var arg = args[i];
                    newArgs.Add(arg);
                    var newArg = Prune(arg);
                    if (newArg != null && newArg != arg)
                    {
                        prunedCall = true;
                        newArgs[i] = newArg;
                    }
                }

                var newCall = new Expr.Types.Call();
                newCall.Function = call.Function;
                newCall.Target = call.Target;
                newCall.Args.Add(newArgs);
                var newTarget = Prune(call.Target);
                if (newTarget != null && newTarget != call.Target)
                {
                    prunedCall = true;
                    newCall = new Expr.Types.Call();
                    newCall.Function = call.Function;
                    newCall.Target = newTarget;
                    newCall.Args.Add(newArgs);
                }

                if (prunedCall)
                {
                    var expr = new Expr();
                    expr.Id = node.Id;
                    expr.CallExpr = newCall;
                    return expr;
                }

                break;
            case Expr.ExprKindOneofCase.ListExpr:
                var list = node.ListExpr;
                IList<Expr> elems = list.Elements;
                IList<Expr> newElems = new List<Expr>(elems.Count);
                var prunedList = false;
                for (var i = 0; i < elems.Count; i++)
                {
                    var elem = elems[i];
                    newElems.Add(elem);
                    var newElem = Prune(elem);
                    if (newElem != null && newElem != elem)
                    {
                        newElems[i] = newElem;
                        prunedList = true;
                    }
                }

                if (prunedList)
                {
                    var createList = new Expr.Types.CreateList();
                    createList.Elements.Add(newElems);
                    var expr = new Expr();
                    expr.Id = node.Id;
                    expr.ListExpr = createList;
                    return expr;
                }

                break;
            case Expr.ExprKindOneofCase.StructExpr:
                var prunedStruct = false;
                var @struct = node.StructExpr;
                IList<Expr.Types.CreateStruct.Types.Entry> entries = @struct.Entries;
                var messageType = @struct.MessageName;
                IList<Expr.Types.CreateStruct.Types.Entry> newEntries =
                    new List<Expr.Types.CreateStruct.Types.Entry>(entries.Count);
                for (var i = 0; i < entries.Count; i++)
                {
                    var entry = entries[i];
                    newEntries.Add(entry);
                    var mapKey = entry.MapKey;
                    var newKey = mapKey != null && !mapKey.Equals(new Expr.Types.CreateStruct.Types.Entry().MapKey)
                        ? Prune(mapKey)
                        : null;
                    var newValue = Prune(entry.Value);
                    if ((newKey == null || newKey == mapKey) && (newValue == null || newValue == entry.Value)) continue;

                    prunedStruct = true;
                    Expr.Types.CreateStruct.Types.Entry newEntry;
                    if (messageType.Length > 0)
                    {
                        newEntry = new Expr.Types.CreateStruct.Types.Entry();
                        newEntry.FieldKey = entry.FieldKey;
                        newEntry.Value = newValue;
                    }
                    else
                    {
                        newEntry = new Expr.Types.CreateStruct.Types.Entry();
                        newEntry.MapKey = newKey;
                        newEntry.Value = newValue;
                    }

                    newEntries[i] = newEntry;
                }

                if (prunedStruct)
                {
                    var createStruct = new Expr.Types.CreateStruct();
                    createStruct.MessageName = messageType;
                    createStruct.Entries.Add(entries);
                    var expr = new Expr();
                    expr.Id = node.Id;
                    expr.StructExpr = createStruct;
                    return expr;
                }

                break;
            case Expr.ExprKindOneofCase.ComprehensionExpr:
                var compre = node.ComprehensionExpr;
                // Only the range of the comprehension is pruned since the state tracking only records
                // the last iteration of the comprehension and not each step in the evaluation which
                // means that the any residuals computed in between might be inaccurate.
                var newRange = Prune(compre.IterRange);
                if (newRange != null && newRange != compre.IterRange)
                {
                    var comp = new Expr.Types.Comprehension();
                    comp.IterVar = compre.IterVar;
                    comp.IterRange = newRange;
                    comp.AccuVar = compre.AccuVar;
                    comp.AccuInit = compre.AccuInit;
                    comp.LoopCondition = compre.LoopCondition;
                    comp.LoopStep = compre.LoopStep;
                    comp.Result = compre.Result;
                    var expr = new Expr();
                    expr.Id = node.Id;
                    expr.ComprehensionExpr = comp;
                    return expr;
                }

                break;
        }

        // Note: original Go implementation returns "node, false". We could wrap 'node' in some
        // 'PruneResult' wrapper, but that would just exchange allocation cost at one point w/
        // allocation cost at another point. So go with the simple approach - at least for now.
        return node;
    }

    internal IVal? Value(long id)
    {
        return state.Value(id);
    }

    internal bool ExistsWithUnknownValue(long id)
    {
        var val = Value(id);
        return UnknownT.IsUnknown(val);
    }

    internal bool ExistsWithKnownValue(long id)
    {
        var val = Value(id);
        return val != null && !UnknownT.IsUnknown(val);
    }

    internal long NextId()
    {
        while (true)
            if (state.Value(nextExprId) != null)
                nextExprId++;
            else
                return nextExprId++;
    }
}