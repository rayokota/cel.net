using System.Collections.Generic;
using Cel.Common.Types;

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
namespace Cel.Interpreter
{
    // TODO Consider having a separate walk of the AST that finds common
    //  subexpressions. This can be called before or after constant folding to find
    //  common subexpressions.

    using Constant = Google.Api.Expr.V1Alpha1.Constant;
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
    using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Types.Comprehension;
    using CreateList = Google.Api.Expr.V1Alpha1.Expr.Types.CreateList;
    using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct;
    using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
    using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
    using ByteString = Google.Protobuf.ByteString;
    using NullValue = Google.Protobuf.WellKnownTypes.NullValue;
    using Operator = global::Cel.Common.Operators.Operator;
    using IteratorT = global::Cel.Common.Types.IteratorT;
    using Type = global::Cel.Common.Types.Ref.Type;
    using Val = global::Cel.Common.Types.Ref.Val;
    using Lister = global::Cel.Common.Types.Traits.Lister;
    using Mapper = global::Cel.Common.Types.Traits.Mapper;

    /// <summary>
    /// PruneAst prunes the given AST based on the given EvalState and generates a new AST. Given AST is
    /// copied on write and a new AST is returned.
    /// 
    /// <para>Couple of typical use cases this interface would be:
    /// 
    /// <ol>
    ///   <li>
    ///       <ol>
    ///         <li>Evaluate expr with some unknowns,
    ///         <li>If result is unknown:
    ///             <ol>
    ///               <li>PruneAst
    ///               <li>Goto 1
    ///             </ol>
    ///             Functional call results which are known would be effectively cached across
    ///             iterations.
    ///       </ol>
    ///   <li>
    ///       <ol>
    ///         <li>Compile the expression (maybe via a service and maybe after checking a compiled
    ///             expression does not exists in local cache)
    ///         <li>Prepare the environment and the interpreter. Activation might be empty.
    ///         <li>Eval the expression. This might return unknown or error or a concrete value.
    ///         <li>PruneAst
    ///         <li>Maybe cache the expression
    ///       </ol>
    /// </ol>
    /// 
    /// </para>
    /// <para>This is effectively constant folding the expression. How the environment is prepared in step 2
    /// is flexible. For example, If the caller caches the compiled and constant folded expressions, but
    /// is not willing to constant fold(and thus cache results of) some external calls, then they can
    /// prepare the overloads accordingly.
    /// </para>
    /// </summary>
    public sealed class AstPruner
    {
        private readonly Expr expr;
        private readonly EvalState state;
        private long nextExprID;

        private AstPruner(Expr expr, EvalState state, long nextExprID)
        {
            this.expr = expr;
            this.state = state;
            this.nextExprID = nextExprID;
        }

        public static Expr PruneAst(Expr expr, EvalState state)
        {
            AstPruner pruner = new AstPruner(expr, state, 1);
            Expr newExpr = pruner.Prune(expr);
            return newExpr;
        }

        internal static Expr CreateLiteral(long id, Constant val)
        {
            Expr expr = new Expr();
            expr.Id = id;
            expr.ConstExpr = val;
            return expr;
        }

        internal Expr MaybeCreateLiteral(long id, Val v)
        {
            Constant constant = new Constant();
            Type t = v.Type();
            switch (t.TypeEnum().InnerEnumValue)
            {
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bool:
                    constant.BoolValue = (bool)v.Value();
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Int:
                    constant.Int64Value = (long)v.Value();
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Uint:
                    constant.Uint64Value = (ulong)v.Value();
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.String:
                    constant.StringValue = v.Value().ToString();
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Double:
                    constant.DoubleValue = (double)v.Value();
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Bytes:
                    constant.BytesValue = ByteString.CopyFrom((byte[])v.Value());
                    return CreateLiteral(id, constant);
                case global::Cel.Common.Types.Ref.TypeEnum.InnerEnum.Null:
                    constant.NullValue = NullValue.NullValue;
                    return CreateLiteral(id, constant);
            }

            // Attempt to build a list literal.
            if (v is Lister)
            {
                Lister list = (Lister)v;
                int sz = (int)list.Size().IntValue();
                IList<Expr> elemExprs = new List<Expr>(sz);
                for (int i = 0; i < sz; i++)
                {
                    Val elem = list.Get(IntT.IntOf(i));
                    if (Util.IsUnknownOrError(elem))
                    {
                        return null;
                    }

                    Expr elemExpr = MaybeCreateLiteral(NextID(), elem);
                    if (elemExpr == null)
                    {
                        return null;
                    }

                    elemExprs.Add(elemExpr);
                }

                CreateList createList = new CreateList();
                createList.Elements.Add(elemExprs);
                Expr expr = new Expr();
                expr.Id = id;
                expr.ListExpr = createList;
                return expr;
            }

            // Create a map literal if possible.
            if (v is Mapper)
            {
                Mapper mp = (Mapper)v;
                IteratorT it = mp.Iterator();
                IList<Entry> entries = new List<Entry>((int)mp.Size().IntValue());
                while (it.HasNext() == BoolT.True)
                {
                    Val key = it.Next();
                    Val val = mp.Get(key);
                    if (Util.IsUnknownOrError(key) || Util.IsUnknownOrError(val))
                    {
                        return null;
                    }

                    Expr keyExpr = MaybeCreateLiteral(NextID(), key);
                    if (keyExpr == null)
                    {
                        return null;
                    }

                    Expr valExpr = MaybeCreateLiteral(NextID(), val);
                    if (valExpr == null)
                    {
                        return null;
                    }

                    Entry entry = new Entry();
                    entry.Id = id;
                    entry.MapKey = keyExpr;
                    entry.Value = valExpr;
                    entries.Add(entry);
                }

                CreateStruct createStruct = new CreateStruct();
                createStruct.Entries.Add(entries);
                Expr expr = new Expr();
                expr.StructExpr = createStruct;
                return expr;
            }

            // TODO(issues/377) To construct message literals, the type provider will need to support
            //  the enumeration the fields for a given message.
            return null;
        }

        internal Expr MaybePruneAndOr(Expr node)
        {
            if (!ExistsWithUnknownValue(node.Id))
            {
                return null;
            }

            Call call = node.CallExpr;
            // We know result is unknown, so we have at least one unknown arg
            // and if one side is a known value, we know we can ignore it.
            if (ExistsWithKnownValue(call.Args[0].Id))
            {
                return call.Args[1];
            }

            if (ExistsWithKnownValue(call.Args[1].Id))
            {
                return call.Args[0];
            }

            return null;
        }

        internal Expr MaybePruneConditional(Expr node)
        {
            if (!ExistsWithUnknownValue(node.Id))
            {
                return null;
            }

            Call call = node.CallExpr;
            Val condVal = Value(call.Args[0].Id);
            if (condVal == null || Util.IsUnknownOrError(condVal))
            {
                return null;
            }

            if (condVal == BoolT.True)
            {
                return call.Args[1];
            }

            return call.Args[2];
        }

        internal Expr MaybePruneFunction(Expr node)
        {
            Call call = node.CallExpr;
            if (call.Function.Equals(Operator.LogicalOr.id) || call.Function.Equals(Operator.LogicalAnd.id))
            {
                return MaybePruneAndOr(node);
            }

            if (call.Function.Equals(Operator.Conditional.id))
            {
                return MaybePruneConditional(node);
            }

            return null;
        }

        internal Expr Prune(Expr node)
        {
            if (node == null)
            {
                return null;
            }

            Val val = Value(node.Id);
            if (val != null && !Util.IsUnknownOrError(val))
            {
                Expr newNode = MaybeCreateLiteral(node.Id, val);
                if (newNode != null)
                {
                    return newNode;
                }
            }

            // We have either an unknown/error value, or something we dont want to
            // transform, or expression was not evaluated. If possible, drill down
            // more.

            switch (node.ExprKindCase)
            {
                case Expr.ExprKindOneofCase.SelectExpr:
                    Select select = node.SelectExpr;
                    Expr operand = Prune(select.Operand);
                    if (operand != null && operand != select.Operand)
                    {
                        Select sel = new Select();
                        sel.Operand = operand;
                        sel.Field = select.Field;
                        sel.TestOnly = select.TestOnly;
                        Expr expr = new Expr();
                        expr.Id = nextExprID;
                        expr.SelectExpr = sel;
                        return expr;
                    }

                    break;
                case Expr.ExprKindOneofCase.CallExpr:
                    Call call = node.CallExpr;
                    Expr newExpr = MaybePruneFunction(node);
                    if (newExpr != null)
                    {
                        newExpr = Prune(newExpr);
                        return newExpr;
                    }

                    bool prunedCall = false;
                    IList<Expr> args = call.Args;
                    IList<Expr> newArgs = new List<Expr>(args.Count);
                    for (int i = 0; i < args.Count; i++)
                    {
                        Expr arg = args[i];
                        newArgs.Add(arg);
                        Expr newArg = Prune(arg);
                        if (newArg != null && newArg != arg)
                        {
                            prunedCall = true;
                            newArgs[i] = newArg;
                        }
                    }

                    Call newCall = new Call();
                    newCall.Function = call.Function;
                    newCall.Target = call.Target;
                    newCall.Args.Add(newArgs);
                    Expr newTarget = Prune(call.Target);
                    if (newTarget != null && newTarget != call.Target)
                    {
                        prunedCall = true;
                        newCall = new Call();
                        newCall.Function = call.Function;
                        newCall.Target = newTarget;
                        newCall.Args.Add(newArgs);
                    }

                    if (prunedCall)
                    {
                        Expr expr = new Expr();
                        expr.Id = node.Id;
                        expr.CallExpr = newCall;
                        return expr;
                    }

                    break;
                case Expr.ExprKindOneofCase.ListExpr:
                    CreateList list = node.ListExpr;
                    IList<Expr> elems = list.Elements;
                    IList<Expr> newElems = new List<Expr>(elems.Count);
                    bool prunedList = false;
                    for (int i = 0; i < elems.Count; i++)
                    {
                        Expr elem = elems[i];
                        newElems.Add(elem);
                        Expr newElem = Prune(elem);
                        if (newElem != null && newElem != elem)
                        {
                            newElems[i] = newElem;
                            prunedList = true;
                        }
                    }

                    if (prunedList)
                    {
                        CreateList createList = new CreateList();
                        createList.Elements.Add(newElems);
                        Expr expr = new Expr();
                        expr.Id = node.Id;
                        expr.ListExpr = createList;
                        return expr;
                    }

                    break;
                case Expr.ExprKindOneofCase.StructExpr:
                    bool prunedStruct = false;
                    CreateStruct @struct = node.StructExpr;
                    IList<Entry> entries = @struct.Entries;
                    string messageType = @struct.MessageName;
                    IList<Entry> newEntries = new List<Entry>(entries.Count);
                    for (int i = 0; i < entries.Count; i++)
                    {
                        Entry entry = entries[i];
                        newEntries.Add(entry);
                        Expr mapKey = entry.MapKey;
                        Expr newKey = mapKey != new Entry().MapKey ? Prune(mapKey) : null;
                        Expr newValue = Prune(entry.Value);
                        if ((newKey == null || newKey == mapKey) && (newValue == null || newValue == entry.Value))
                        {
                            continue;
                        }

                        prunedStruct = true;
                        Entry newEntry;
                        if (messageType.Length > 0)
                        {
                            newEntry = new Entry();
                            newEntry.FieldKey = entry.FieldKey;
                            newEntry.Value = newValue;
                        }
                        else
                        {
                            newEntry = new Entry();
                            newEntry.MapKey = newKey;
                            newEntry.Value = newValue;
                        }

                        newEntries[i] = newEntry;
                    }

                    if (prunedStruct)
                    {
                        CreateStruct createStruct = new CreateStruct();
                        createStruct.MessageName = messageType;
                        createStruct.Entries.Add(entries);
                        Expr expr = new Expr();
                        expr.Id = node.Id;
                        expr.StructExpr = createStruct;
                        return expr;
                    }

                    break;
                case Expr.ExprKindOneofCase.ComprehensionExpr:
                    Comprehension compre = node.ComprehensionExpr;
                    // Only the range of the comprehension is pruned since the state tracking only records
                    // the last iteration of the comprehension and not each step in the evaluation which
                    // means that the any residuals computed in between might be inaccurate.
                    Expr newRange = Prune(compre.IterRange);
                    if (newRange != null && newRange != compre.IterRange)
                    {
                        Comprehension comp = new Comprehension();
                        comp.IterVar = compre.IterVar;
                        comp.IterRange = newRange;
                        comp.AccuVar = compre.AccuVar;
                        comp.AccuInit = compre.AccuInit;
                        comp.LoopCondition = compre.LoopCondition;
                        comp.LoopStep = compre.LoopStep;
                        comp.Result = compre.Result;
                        Expr expr = new Expr();
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

        internal Val Value(long id)
        {
            return state.Value(id);
        }

        internal bool ExistsWithUnknownValue(long id)
        {
            Val val = Value(id);
            return UnknownT.IsUnknown(val);
        }

        internal bool ExistsWithKnownValue(long id)
        {
            Val val = Value(id);
            return val != null && !UnknownT.IsUnknown(val);
        }

        internal long NextID()
        {
            while (true)
            {
                if (state.Value(nextExprID) != null)
                {
                    nextExprID++;
                }
                else
                {
                    return nextExprID++;
                }
            }
        }
    }
}