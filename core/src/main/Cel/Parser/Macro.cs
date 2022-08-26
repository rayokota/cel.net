using System.Collections.Generic;

/*
 * Copyright (C) 2021 The Authors of CEL-Java
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
namespace Cel.Parser
{
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
    using ErrorWithLocation = Cel.Common.ErrorWithLocation;
    using Location = Cel.Common.Location;
    using Operator = Cel.Common.Operators.Operator;

    public sealed class Macro
    {
        /// <summary>
        /// AccumulatorName is the traditional variable name assigned to the fold accumulator variable. </summary>
        public const string AccumulatorName = "__result__";

        /// <summary>
        /// AllMacros includes the list of all spec-supported macros. </summary>
        public static readonly IList<Macro> AllMacros = new List<Macro>
        {
            NewGlobalMacro(Operator.Has.id, 1, Macro.makeHas), NewReceiverMacro(Operator.All.id, 2, Macro.MakeAll),
            NewReceiverMacro(Operator.Exists.id, 2, Macro.MakeExists),
            NewReceiverMacro(Operator.ExistsOne.id, 2, Macro.MakeExistsOne),
            NewReceiverMacro(Operator.Map.id, 2, Macro.makeMap), NewReceiverMacro(Operator.Map.id, 3, Macro.makeMap),
            NewReceiverMacro(Operator.Filter.id, 2, Macro.makeFilter)
        };

        /// <summary>
        /// NoMacros list. </summary>
        public static IList<Macro> MoMacros = new List<Macro>();

//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly string function_Conflict;
        private readonly bool receiverStyle;

        private readonly bool varArgStyle;

//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly int argCount_Conflict;

//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
        private readonly MacroExpander expander_Conflict;

        public Macro(string function, bool receiverStyle, bool varArgStyle, int argCount, MacroExpander expander)
        {
            this.function_Conflict = function;
            this.receiverStyle = receiverStyle;
            this.varArgStyle = varArgStyle;
            this.argCount_Conflict = argCount;
            this.expander_Conflict = expander;
        }

        public override string ToString()
        {
            return "Macro{" + "function='" + function_Conflict + '\'' + ", receiverStyle=" + receiverStyle +
                   ", varArgStyle=" + varArgStyle + ", argCount=" + argCount_Conflict + '}';
        }

        internal static string MakeMacroKey(string name, int args, bool receiverStyle)
        {
            return string.Format("{0}:{1:D}:{2}", name, args, receiverStyle);
        }

        internal static string MakeVarArgMacroKey(string name, bool receiverStyle)
        {
            return string.Format("{0}:*:{1}", name, receiverStyle);
        }

        /// <summary>
        /// NewGlobalMacro creates a Macro for a global function with the specified arg count. </summary>
        internal static Macro NewGlobalMacro(string function, int argCount, MacroExpander expander)
        {
            return new Macro(function, false, false, argCount, expander);
        }

        /// <summary>
        /// NewReceiverMacro creates a Macro for a receiver function matching the specified arg count. </summary>
        public static Macro NewReceiverMacro(string function, int argCount, MacroExpander expander)
        {
            return new Macro(function, true, false, argCount, expander);
        }

        /// <summary>
        /// NewGlobalVarArgMacro creates a Macro for a global function with a variable arg count. </summary>
        internal static Macro NewGlobalVarArgMacro(string function, MacroExpander expander)
        {
            return new Macro(function, false, true, 0, expander);
        }

        /// <summary>
        /// NewReceiverVarArgMacro creates a Macro for a receiver function matching a variable arg count.
        /// </summary>
        internal static Macro NewReceiverVarArgMacro(string function, MacroExpander expander)
        {
            return new Macro(function, true, true, 0, expander);
        }

        internal static Expr MakeAll(ExprHelper eh, Expr target, IList<Expr> args)
        {
            return MakeQuantifier(QuantifierKind.quantifierAll, eh, target, args);
        }

        internal static Expr MakeExists(ExprHelper eh, Expr target, IList<Expr> args)
        {
            return MakeQuantifier(QuantifierKind.quantifierExists, eh, target, args);
        }

        internal static Expr MakeExistsOne(ExprHelper eh, Expr target, IList<Expr> args)
        {
            return MakeQuantifier(QuantifierKind.quantifierExistsOne, eh, target, args);
        }

        internal static Expr MakeQuantifier(QuantifierKind kind, ExprHelper eh, Expr target, IList<Expr> args)
        {
            string v = extractIdent(args[0]);
            if (string.ReferenceEquals(v, null))
            {
                Location location = eh.OffsetLocation(args[0].Id);
                throw new ErrorWithLocation(location, "argument must be a simple name");
            }

            System.Func<Expr> accuIdent = () => eh.Ident(AccumulatorName);

            Expr init;
            Expr condition;
            Expr step;
            Expr result;
            switch (kind)
            {
                case QuantifierKind.quantifierAll:
                    init = eh.LiteralBool(true);
                    condition = eh.GlobalCall(Operator.NotStrictlyFalse.id, accuIdent());
                    step = eh.GlobalCall(Operator.LogicalAnd.id, accuIdent(), args[1]);
                    result = accuIdent();
                    break;
                case QuantifierKind.quantifierExists:
                    init = eh.LiteralBool(false);
                    condition = eh.GlobalCall(Operator.NotStrictlyFalse.id,
                        eh.GlobalCall(Operator.LogicalNot.id, accuIdent()));
                    step = eh.GlobalCall(Operator.LogicalOr.id, accuIdent(), args[1]);
                    result = accuIdent();
                    break;
                case QuantifierKind.quantifierExistsOne:
                    Expr zeroExpr = eh.LiteralInt(0);
                    Expr oneExpr = eh.LiteralInt(1);
                    init = zeroExpr;
                    condition = eh.LiteralBool(true);
                    step = eh.GlobalCall(Operator.Conditional.id, args[1],
                        eh.GlobalCall(Operator.Add.id, accuIdent(), oneExpr), accuIdent());
                    result = eh.GlobalCall(Operator.Equals.id, accuIdent(), oneExpr);
                    break;
                default:
                    throw new ErrorWithLocation(null, string.Format("unrecognized quantifier '{0}'", kind));
            }

            return eh.Fold(v, target, AccumulatorName, init, condition, step, result);
        }

        internal static Expr makeMap(ExprHelper eh, Expr target, IList<Expr> args)
        {
            string v = extractIdent(args[0]);
            if (string.ReferenceEquals(v, null))
            {
                throw new ErrorWithLocation(null, "argument is not an identifier");
            }

            Expr fn;
            Expr filter;

            if (args.Count == 3)
            {
                filter = args[1];
                fn = args[2];
            }
            else
            {
                filter = null;
                fn = args[1];
            }

            Expr accuExpr = eh.Ident(AccumulatorName);
            Expr init = eh.NewList();
            Expr condition = eh.LiteralBool(true);
            Expr step = eh.GlobalCall(Operator.Add.id, accuExpr, eh.NewList(fn));

            if (filter != null)
            {
                step = eh.GlobalCall(Operator.Conditional.id, filter, step, accuExpr);
            }

            return eh.Fold(v, target, AccumulatorName, init, condition, step, accuExpr);
        }

        internal static Expr makeFilter(ExprHelper eh, Expr target, IList<Expr> args)
        {
            string v = extractIdent(args[0]);
            if (string.ReferenceEquals(v, null))
            {
                throw new ErrorWithLocation(null, "argument is not an identifier");
            }

            Expr filter = args[1];
            Expr accuExpr = eh.Ident(AccumulatorName);
            Expr init = eh.NewList();
            Expr condition = eh.LiteralBool(true);
            Expr step = eh.GlobalCall(Operator.Add.id, accuExpr, eh.NewList(args[0]));
            step = eh.GlobalCall(Operator.Conditional.id, filter, step, accuExpr);
            return eh.Fold(v, target, AccumulatorName, init, condition, step, accuExpr);
        }

        internal static string extractIdent(Expr e)
        {
            if (e.ExprKindCase == Expr.ExprKindOneofCase.IdentExpr)
            {
                return e.IdentExpr.Name;
            }

            return null;
        }

        internal static Expr makeHas(ExprHelper eh, Expr target, IList<Expr> args)
        {
            if (args[0].ExprKindCase == Expr.ExprKindOneofCase.SelectExpr)
            {
                Expr.Types.Select s = args[0].SelectExpr;
                return eh.PresenceTest(s.Operand, s.Field);
            }

            throw new ErrorWithLocation(null, "invalid argument to has() macro");
        }

        public string function()
        {
            return function_Conflict;
        }

        public bool ReceiverStyle
        {
            get { return receiverStyle; }
        }

        public bool VarArgStyle
        {
            get { return varArgStyle; }
        }

        public int argCount()
        {
            return argCount_Conflict;
        }

        public MacroExpander Expander()
        {
            return expander_Conflict;
        }

        public string MacroKey()
        {
            if (varArgStyle)
            {
                return MakeVarArgMacroKey(function_Conflict, receiverStyle);
            }

            return MakeMacroKey(function_Conflict, argCount_Conflict, receiverStyle);
        }

        internal enum QuantifierKind
        {
            quantifierAll,
            quantifierExists,
            quantifierExistsOne
        }
    }
}