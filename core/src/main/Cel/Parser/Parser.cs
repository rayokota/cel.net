using System.Collections;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Tree;
using Cel.Common;
using Cel.Common.Operators;
using Cel.Parser.Gen;
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
namespace Cel.Parser;

/*
	using CommonTokenStream = Antlr4.Runtime.CommonTokenStream;

	//using ANTLRErrorListener = Antlr4.Runtime.IAntlrErrorListener;
	using DefaultErrorStrategy = Cel.shaded.org.antlr.v4.runtime.DefaultErrorStrategy;
	using IntStream = Cel.shaded.org.antlr.v4.runtime.IntStream;
	using ParserRuleContext = Cel.shaded.org.antlr.v4.runtime.ParserRuleContext;
	using RecognitionException = Cel.shaded.org.antlr.v4.runtime.RecognitionException;
	using Recognizer = Cel.shaded.org.antlr.v4.runtime.Recognizer;
	using RuleContext = Cel.shaded.org.antlr.v4.runtime.RuleContext;
	using Token = Antlr4.Runtime.IToken;
	using ATNConfigSet = Cel.shaded.org.antlr.v4.runtime.atn.ATNConfigSet;
	using DFA = Cel.shaded.org.antlr.v4.runtime.dfa.DFA;
	using AbstractParseTreeVisitor = Cel.shaded.org.antlr.v4.runtime.tree.AbstractParseTreeVisitor;
	using ErrorNode = Cel.shaded.org.antlr.v4.runtime.tree.ErrorNode;
	using ParseTree = Cel.shaded.org.antlr.v4.runtime.tree.ParseTree;
	using ParseTreeListener = Cel.shaded.org.antlr.v4.runtime.tree.ParseTreeListener;
	using TerminalNode = Cel.shaded.org.antlr.v4.runtime.tree.TerminalNode;
	*/

public sealed class Parser
{
    private static readonly ISet<string> reservedIds = new HashSet<string>
    {
        "as", "break", "const", "continue", "else", "false", "for", "function", "if", "import", "in", "let", "loop",
        "package", "namespace", "null", "return", "true", "var", "void", "while"
    };

    private readonly Options options;

    internal Parser(Options options)
    {
        this.options = options;
    }

    public static ParseResult ParseAllMacros(Source source)
    {
        return Parse(Options.NewBuilder().Macros(Macro.AllMacros).Build(), source);
    }

    public static ParseResult ParseWithMacros(Source source, IList<Macro> macros)
    {
        return Parse(Options.NewBuilder().Macros(macros).Build(), source);
    }

    public static ParseResult Parse(Options options, Source source)
    {
        return new Parser(options).Parse(source);
    }

    internal ParseResult Parse(Source source)
    {
        ICharStream charStream = new StringCharStream(source.Content(), source.Description());
        var lexer = new CELLexer(charStream);
        var parser = new CELParser(new CommonTokenStream(lexer, 0));

        var parserListener = new RecursionListener(options.MaxRecursionDepth);

        parser.AddParseListener(parserListener);

        parser.ErrorHandler = new RecoveryLimitErrorStrategy(options.ErrorRecoveryLimit);

        var helper = new Helper(source);
        var errors = new Errors(source);

        var inner = new InnerParser<int>(this, helper, errors);
        var inner2 = new InnerParser<IToken>(this, helper, errors);

        lexer.AddErrorListener(inner);
        parser.AddErrorListener(inner2);

        Expr expr = null;
        try
        {
            if (charStream.Size > options.ExpressionSizeCodePointLimit)
                errors.ReportError(Location.NoLocation,
                    "expression code point size exceeds limit: size: %d, limit %d", charStream.Size,
                    options.ExpressionSizeCodePointLimit);
            else
                expr = inner.ExprVisit(parser.start());
        }
        catch (Exception e) when (e is RecoveryLimitError || e is RecursionError)
        {
            errors.ReportError(Location.NoLocation, "{0}", e.Message);
        }

        if (errors.HasErrors()) expr = null;

        return new ParseResult(expr, errors, helper.SourceInfo);
    }

    public sealed class ParseResult
    {
        internal readonly Errors errors;
        internal readonly Expr expr;
        internal readonly SourceInfo sourceInfo;

        public ParseResult(Expr expr, Errors errors, SourceInfo sourceInfo)
        {
            this.expr = expr;
            this.errors = errors;
            this.sourceInfo = sourceInfo;
        }

        public Expr Expr => expr;

        public Errors Errors => errors;

        public SourceInfo SourceInfo => sourceInfo;

        public bool HasErrors()
        {
            return errors.HasErrors();
        }
    }

    internal sealed class RecursionListener : IParseTreeListener
    {
        internal readonly int maxDepth;
        internal int depth;

        internal RecursionListener(int maxDepth)
        {
            this.maxDepth = maxDepth;
        }

        public void VisitTerminal(ITerminalNode node)
        {
        }

        public void VisitErrorNode(IErrorNode node)
        {
        }

        public void EnterEveryRule(ParserRuleContext ctx)
        {
            if (ctx != null && ctx.RuleIndex == CELParser.RULE_expr)
            {
                if (depth >= maxDepth)
                {
                    depth++;
                    throw new RecursionError(string.Format("expression recursion limit exceeded: {0:D}", maxDepth));
                }

                depth++;
            }
        }

        public void ExitEveryRule(ParserRuleContext ctx)
        {
            if (ctx != null && ctx.RuleIndex == CELParser.RULE_expr) depth--;
        }
    }

    internal sealed class RecursionError : Exception
    {
        public RecursionError(string message) : base(message)
        {
        }
    }

    internal sealed class RecoveryLimitError : RecognitionException
    {
        public RecoveryLimitError(string message, IRecognizer recognizer, IIntStream input, ParserRuleContext ctx) :
            base(message, recognizer, input, ctx)
        {
        }
    }

    internal sealed class RecoveryLimitErrorStrategy : DefaultErrorStrategy
    {
        internal readonly int maxAttempts;
        internal int attempts;

        internal RecoveryLimitErrorStrategy(int maxAttempts)
        {
            this.maxAttempts = maxAttempts;
        }

        public override void Recover(Antlr4.Runtime.Parser recognizer, RecognitionException e)
        {
            CheckAttempts(recognizer);
            base.Recover(recognizer, e);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public Cel.shaded.org.antlr.v4.runtime.Token recoverInline(Cel.shaded.org.antlr.v4.runtime.Parser recognizer) throws Cel.shaded.org.antlr.v4.runtime.RecognitionException
        public override IToken RecoverInline(Antlr4.Runtime.Parser recognizer)
        {
            CheckAttempts(recognizer);
            return base.RecoverInline(recognizer);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkAttempts(Cel.shaded.org.antlr.v4.runtime.Parser recognizer) throws Cel.shaded.org.antlr.v4.runtime.RecognitionException
        internal void CheckAttempts(Antlr4.Runtime.Parser recognizer)
        {
            if (attempts >= maxAttempts)
            {
                attempts++;
                var msg = string.Format("error recovery attempt limit exceeded: {0:D}", maxAttempts);
                recognizer.NotifyErrorListeners(null, msg, null);
                throw new RecoveryLimitError(msg, recognizer, null, null);
            }

            attempts++;
        }
    }

    internal sealed class InnerParser<Symbol> : AbstractParseTreeVisitor<object>, IAntlrErrorListener<Symbol>
    {
        internal readonly Errors errors;


        internal readonly Helper helper;
        private readonly Parser outerInstance;

        internal InnerParser(Parser outerInstance, Helper helper, Errors errors)
        {
            this.outerInstance = outerInstance;
            this.helper = helper;
            this.errors = errors;
        }

        public void SyntaxError(TextWriter output, IRecognizer recognizer, Symbol offendingSymbol, int line,
            int charPositionInLine, string msg, RecognitionException e)
        {
            errors.SyntaxError(Location.NewLocation(line, charPositionInLine), msg);
        }

        public void ReportAmbiguity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex,
            bool exact, BitArray ambigAlts, ATNConfigSet configs)
        {
            // empty
        }

        public void ReportAttemptingFullContext(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex,
            int stopIndex, BitArray conflictingAlts, ATNConfigSet configs)
        {
            // empty
        }

        public void ReportContextSensitivity(Antlr4.Runtime.Parser recognizer, DFA dfa, int startIndex,
            int stopIndex, int prediction, ATNConfigSet configs)
        {
            // empty
        }

        internal Expr ReportError(object ctx, string message)
        {
            return ReportError(ctx, "{0}", message);
        }

        internal Expr ReportError(object ctx, string format, params object[] args)
        {
            Expr err;
            Location location;
            if (ctx is Location)
            {
                location = (Location)ctx;
            }
            else if (ctx is IToken || ctx is ParserRuleContext)
            {
                err = helper.NewExpr(ctx);
                location = helper.GetLocation(err.Id);
            }
            else
            {
                location = Location.NoLocation;
            }

            err = helper.NewExpr(ctx);
            // Provide arguments to the report error.
            errors.ReportError(location, format, args);
            return err;
        }

        public Expr ExprVisit(IParseTree tree)
        {
            var r = Visit(tree);
            return (Expr)r;
        }

        public override object Visit(IParseTree tree)
        {
            if (tree is RuleContext)
            {
                var ruleContext = (RuleContext)tree;
                var ruleIndex = ruleContext.RuleIndex;
                switch (ruleIndex)
                {
                    case CELParser.RULE_start:
                        return VisitStart((CELParser.StartContext)tree);
                    case CELParser.RULE_expr:
                        return VisitExpr((CELParser.ExprContext)tree);
                    case CELParser.RULE_conditionalOr:
                        return VisitConditionalOr((CELParser.ConditionalOrContext)tree);
                    case CELParser.RULE_conditionalAnd:
                        return VisitConditionalAnd((CELParser.ConditionalAndContext)tree);
                    case CELParser.RULE_relation:
                        return VisitRelation((CELParser.RelationContext)tree);
                    case CELParser.RULE_calc:
                        return VisitCalc((CELParser.CalcContext)tree);
                    case CELParser.RULE_unary:
                        if (tree is CELParser.LogicalNotContext)
                            return VisitLogicalNot((CELParser.LogicalNotContext)tree);
                        else if (tree is CELParser.NegateContext)
                            return VisitNegate((CELParser.NegateContext)tree);
                        else if (tree is CELParser.MemberExprContext)
                            return VisitMemberExpr((CELParser.MemberExprContext)tree);

                        return VisitUnary((CELParser.UnaryContext)tree);
                    case CELParser.RULE_member:
                        if (tree is CELParser.CreateMessageContext)
                            return VisitCreateMessage((CELParser.CreateMessageContext)tree);
                        else if (tree is CELParser.PrimaryExprContext)
                            return VisitPrimaryExpr((CELParser.PrimaryExprContext)tree);
                        else if (tree is CELParser.SelectOrCallContext)
                            return VisitSelectOrCall((CELParser.SelectOrCallContext)tree);
                        else if (tree is CELParser.IndexContext) return VisitIndex((CELParser.IndexContext)tree);

                        break;
                    case CELParser.RULE_primary:
                        if (tree is CELParser.CreateListContext)
                            return VisitCreateList((CELParser.CreateListContext)tree);
                        else if (tree is CELParser.CreateStructContext)
                            return VisitCreateStruct((CELParser.CreateStructContext)tree);

                        break;
                    case CELParser.RULE_fieldInitializerList:
                    case CELParser.RULE_mapInitializerList:
                        return VisitMapInitializerList((CELParser.MapInitializerListContext)tree);
                    // case CELParser.RULE_exprList:
                    // case CELParser.RULE_literal:
                    default:
                        return ReportError(tree, "parser rule '%d'", ruleIndex);
                }
            }

            // Report at least one error if the parser reaches an unknown parse element.
            // Typically, this happens if the parser has already encountered a syntax error elsewhere.
            if (!errors.HasErrors())
            {
                var txt = "<<nil>>";
                if (tree != null) txt = string.Format("<<{0}>>", tree.GetType().Name);

                return ReportError(Location.NoLocation, "unknown parse element encountered: {0}", txt);
            }

            return helper.NewExpr(Location.NoLocation);
        }

        internal object VisitStart(CELParser.StartContext ctx)
        {
            return Visit(ctx.expr());
        }

        internal Expr VisitExpr(CELParser.ExprContext ctx)
        {
            var result = ExprVisit(ctx.e);
            if (ctx.op == null) return result;

            var opID = helper.Id(ctx.op);
            var ifTrue = ExprVisit(ctx.e1);
            var ifFalse = ExprVisit(ctx.e2);
            return globalCallOrMacro(opID, Operator.Conditional.id, result, ifTrue, ifFalse);
        }

        internal Expr VisitConditionalAnd(CELParser.ConditionalAndContext ctx)
        {
            var result = ExprVisit(ctx.e);
            if (ctx._ops == null || ctx._ops.Count == 0) return result;

            var b = helper.NewBalancer(Operator.LogicalAnd.id, result);
            IList<CELParser.RelationContext> rest = ctx._e1;
            for (var i = 0; i < ctx._ops.Count; i++)
            {
                var op = ctx._ops[i];
                if (i >= rest.Count) return ReportError(ctx, "unexpected character, wanted '&&'");

                var next = ExprVisit(rest[i]);
                var opID = helper.Id(op);
                b.AddTerm(opID, next);
            }

            return b.balance();
        }

        internal Expr VisitConditionalOr(CELParser.ConditionalOrContext ctx)
        {
            var result = ExprVisit(ctx.e);
            if (ctx._ops == null || ctx._ops.Count == 0) return result;

            var b = helper.NewBalancer(Operator.LogicalOr.id, result);
            IList<CELParser.ConditionalAndContext> rest = ctx._e1;
            for (var i = 0; i < ctx._ops.Count; i++)
            {
                var op = ctx._ops[i];
                if (i >= rest.Count) return ReportError(ctx, "unexpected character, wanted '||'");

                var next = ExprVisit(rest[i]);
                var opID = helper.Id(op);
                b.AddTerm(opID, next);
            }

            return b.balance();
        }

        internal Expr VisitRelation(CELParser.RelationContext ctx)
        {
            if (ctx.calc() != null) return ExprVisit(ctx.calc());

            var opText = "";
            if (ctx.op != null) opText = ctx.op.Text;

            var op = Operator.Find(opText);
            if (op != null)
            {
                var lhs = ExprVisit(ctx.relation(0));
                var opID = helper.Id(ctx.op);
                var rhs = ExprVisit(ctx.relation(1));
                return globalCallOrMacro(opID, op.id, lhs, rhs);
            }

            return ReportError(ctx, "operator not found");
        }

        internal Expr VisitCalc(CELParser.CalcContext ctx)
        {
            if (ctx.unary() != null) return ExprVisit(ctx.unary());

            var opText = "";
            if (ctx.op != null) opText = ctx.op.Text;

            var op = Operator.Find(opText);
            if (op != null)
            {
                var lhs = ExprVisit(ctx.calc(0));
                var opID = helper.Id(ctx.op);
                var rhs = ExprVisit(ctx.calc(1));
                return globalCallOrMacro(opID, op.id, lhs, rhs);
            }

            return ReportError(ctx, "operator not found");
        }

        internal Expr VisitLogicalNot(CELParser.LogicalNotContext ctx)
        {
            if (ctx._ops.Count % 2 == 0) return ExprVisit(ctx.member());

            var opID = helper.Id(ctx._ops[0]);
            var target = ExprVisit(ctx.member());
            return globalCallOrMacro(opID, Operator.LogicalNot.id, target);
        }

        internal Expr VisitMemberExpr(CELParser.MemberExprContext ctx)
        {
            if (ctx.member() is CELParser.PrimaryExprContext)
                return VisitPrimaryExpr((CELParser.PrimaryExprContext)ctx.member());
            if (ctx.member() is CELParser.SelectOrCallContext)
                return VisitSelectOrCall((CELParser.SelectOrCallContext)ctx.member());
            if (ctx.member() is CELParser.IndexContext)
                return VisitIndex((CELParser.IndexContext)ctx.member());
            if (ctx.member() is CELParser.CreateMessageContext)
                return VisitCreateMessage((CELParser.CreateMessageContext)ctx.member());

            return ReportError(ctx, "unsupported simple expression");
        }

        internal Expr VisitPrimaryExpr(CELParser.PrimaryExprContext ctx)
        {
            if (ctx.primary() is CELParser.NestedContext)
                return VisitNested((CELParser.NestedContext)ctx.primary());
            if (ctx.primary() is CELParser.IdentOrGlobalCallContext)
                return VisitIdentOrGlobalCall((CELParser.IdentOrGlobalCallContext)ctx.primary());
            if (ctx.primary() is CELParser.CreateListContext)
                return VisitCreateList((CELParser.CreateListContext)ctx.primary());
            if (ctx.primary() is CELParser.CreateStructContext)
                return VisitCreateStruct((CELParser.CreateStructContext)ctx.primary());
            if (ctx.primary() is CELParser.ConstantLiteralContext)
                return VisitConstantLiteral((CELParser.ConstantLiteralContext)ctx.primary());

            return ReportError(ctx, "invalid primary expression");
        }

        internal Expr VisitConstantLiteral(CELParser.ConstantLiteralContext ctx)
        {
            if (ctx.literal() is CELParser.IntContext)
                return VisitInt((CELParser.IntContext)ctx.literal());
            if (ctx.literal() is CELParser.UintContext)
                return VisitUint((CELParser.UintContext)ctx.literal());
            if (ctx.literal() is CELParser.DoubleContext)
                return VisitDouble((CELParser.DoubleContext)ctx.literal());
            if (ctx.literal() is CELParser.StringContext)
                return VisitString((CELParser.StringContext)ctx.literal());
            if (ctx.literal() is CELParser.BytesContext)
                return VisitBytes((CELParser.BytesContext)ctx.literal());
            if (ctx.literal() is CELParser.BoolFalseContext)
                return VisitBoolFalse((CELParser.BoolFalseContext)ctx.literal());
            if (ctx.literal() is CELParser.BoolTrueContext)
                return VisitBoolTrue((CELParser.BoolTrueContext)ctx.literal());
            if (ctx.literal() is CELParser.NullContext) return VisitNull((CELParser.NullContext)ctx.literal());

            return ReportError(ctx, "invalid literal");
        }

        internal Expr VisitInt(CELParser.IntContext ctx)
        {
            var text = ctx.tok.Text;
            var @base = 10;
            if (text.StartsWith("0x", StringComparison.Ordinal))
            {
                @base = 16;
                text = text.Substring(2);
            }

            if (ctx.sign != null) text = ctx.sign.Text + text;

            try
            {
                var i = Convert.ToInt64(text, @base);
                return helper.NewLiteralInt(ctx, i);
            }
            catch (Exception)
            {
                return ReportError(ctx, "invalid int literal");
            }
        }

        internal Expr VisitUint(CELParser.UintContext ctx)
        {
            var text = ctx.tok.Text;
            // trim the 'u' designator included in the uint literal.
            text = text.Substring(0, text.Length - 1);
            var @base = 10;
            if (text.StartsWith("0x", StringComparison.Ordinal))
            {
                @base = 16;
                text = text.Substring(2);
            }

            try
            {
                var i = Convert.ToUInt64(text, @base);
                return helper.NewLiteralUint(ctx, i);
            }
            catch (Exception)
            {
                return ReportError(ctx, "invalid int literal");
            }
        }

        internal Expr VisitDouble(CELParser.DoubleContext ctx)
        {
            var txt = ctx.tok.Text;
            if (ctx.sign != null) txt = ctx.sign.Text + txt;

            try
            {
                var f = double.Parse(txt);
                return helper.NewLiteralDouble(ctx, f);
            }
            catch (Exception)
            {
                return ReportError(ctx, "invalid double literal");
            }
        }

        internal Expr VisitString(CELParser.StringContext ctx)
        {
            var s = unquoteString(ctx, ctx.GetText());
            return helper.NewLiteralString(ctx, s);
        }

        internal Expr VisitBytes(CELParser.BytesContext ctx)
        {
            var b = unquoteBytes(ctx, ctx.tok.Text.Substring(1));
            return helper.NewLiteralBytes(ctx, b);
        }

        internal Expr VisitBoolFalse(CELParser.BoolFalseContext ctx)
        {
            return helper.NewLiteralBool(ctx, false);
        }

        internal Expr VisitBoolTrue(CELParser.BoolTrueContext ctx)
        {
            return helper.NewLiteralBool(ctx, true);
        }

        internal Expr VisitNull(CELParser.NullContext ctx)
        {
            var constant = new Constant();
            constant.NullValue = NullValue.NullValue;
            return helper.NewLiteral(ctx, constant);
        }

        internal IList<Expr> VisitList(CELParser.ExprListContext ctx)
        {
            if (ctx == null) return new List<Expr>();

            return VisitSlice(ctx._e);
        }

        internal IList<Expr> VisitSlice(IList<CELParser.ExprContext> expressions)
        {
            if (expressions == null) return new List<Expr>();

            IList<Expr> result = new List<Expr>(expressions.Count);
            foreach (var e in expressions)
            {
                var ex = ExprVisit(e);
                result.Add(ex);
            }

            return result;
        }

        internal string ExtractQualifiedName(Expr e)
        {
            if (e == null) return null;

            switch (e.ExprKindCase)
            {
                case Expr.ExprKindOneofCase.IdentExpr:
                    return e.IdentExpr.Name;
                case Expr.ExprKindOneofCase.SelectExpr:
                    var s = e.SelectExpr;
                    var prefix = ExtractQualifiedName(s.Operand);
                    return prefix + "." + s.Field;
            }

            // TODO: Add a method to Source to get location from character offset.
            var location = helper.GetLocation(e.Id);
            ReportError(location, "expected a qualified name");
            return null;
        }

        // Visit a parse tree of field initializers.
        internal IList<Expr.Types.CreateStruct.Types.Entry> VisitIFieldInitializerList(
            CELParser.FieldInitializerListContext ctx)
        {
            if (ctx == null || ctx._fields == null)
                // This is the result of a syntax error handled elswhere, return empty.
                return new List<Expr.Types.CreateStruct.Types.Entry>();

            IList<Expr.Types.CreateStruct.Types.Entry> result =
                new List<Expr.Types.CreateStruct.Types.Entry>(ctx._fields.Count);
            IList<IToken> cols = ctx._cols;
            IList<CELParser.ExprContext> vals = ctx._values;
            for (var i = 0; i < ctx._fields.Count; i++)
            {
                var f = ctx._fields[i];
                if (i >= cols.Count || i >= vals.Count)
                    // This is the result of a syntax error detected elsewhere.
                    return new List<Expr.Types.CreateStruct.Types.Entry>();

                var initID = helper.Id(cols[i]);
                var value = ExprVisit(vals[i]);
                var field = helper.NewObjectField(initID, f.Text, value);
                result.Add(field);
            }

            return result;
        }

        internal Expr VisitIdentOrGlobalCall(CELParser.IdentOrGlobalCallContext ctx)
        {
            var identName = "";
            if (ctx.leadingDot != null) identName = ".";

            // Handle the error case where no valid identifier is specified.
            if (ctx.id == null) return helper.NewExpr(ctx);

            // Handle reserved identifiers.
            var id = ctx.id.Text;
            if (reservedIds.Contains(id)) return ReportError(ctx, "reserved identifier: {0}", id);

            identName += id;
            if (ctx.op != null)
            {
                var opID = helper.Id(ctx.op);
                return globalCallOrMacro(opID, identName, VisitList(ctx.args));
            }

            return helper.NewIdent(ctx.id, identName);
        }

        internal Expr VisitNested(CELParser.NestedContext ctx)
        {
            return ExprVisit(ctx.e);
        }

        internal Expr VisitSelectOrCall(CELParser.SelectOrCallContext ctx)
        {
            var operand = ExprVisit(ctx.member());
            // Handle the error case where no valid identifier is specified.
            if (ctx.id == null) return helper.NewExpr(ctx);

            var id = ctx.id.Text;
            if (ctx.open != null)
            {
                var opID = helper.Id(ctx.open);
                return receiverCallOrMacro(opID, id, operand, VisitList(ctx.args));
            }

            return helper.NewSelect(ctx.op, operand, id);
        }

        internal IList<Expr.Types.CreateStruct.Types.Entry> VisitMapInitializerList(
            CELParser.MapInitializerListContext ctx)
        {
            if (ctx == null || ctx._keys.Count == 0)
                // This is the result of a syntax error handled elswhere, return empty.
                return new List<Expr.Types.CreateStruct.Types.Entry>();

            IList<Expr.Types.CreateStruct.Types.Entry> result =
                new List<Expr.Types.CreateStruct.Types.Entry>(ctx._cols.Count);
            IList<CELParser.ExprContext> keys = ctx._keys;
            IList<CELParser.ExprContext> vals = ctx._values;
            for (var i = 0; i < ctx._cols.Count; i++)
            {
                var col = ctx._cols[i];
                var colID = helper.Id(col);
                if (i >= keys.Count || i >= vals.Count)
                    // This is the result of a syntax error detected elsewhere.
                    return new List<Expr.Types.CreateStruct.Types.Entry>();

                var key = ExprVisit(keys[i]);
                var value = ExprVisit(vals[i]);
                var entry = helper.NewMapEntry(colID, key, value);
                result.Add(entry);
            }

            return result;
        }

        internal Expr VisitNegate(CELParser.NegateContext ctx)
        {
            if (ctx._ops.Count % 2 == 0) return ExprVisit(ctx.member());

            var opID = helper.Id(ctx._ops[0]);
            var target = ExprVisit(ctx.member());
            return globalCallOrMacro(opID, Operator.Negate.id, target);
        }

        internal Expr VisitIndex(CELParser.IndexContext ctx)
        {
            var target = ExprVisit(ctx.member());
            var opID = helper.Id(ctx.op);
            var index = ExprVisit(ctx.index);
            return globalCallOrMacro(opID, Operator.Index.id, target, index);
        }

        internal Expr VisitUnary(CELParser.UnaryContext ctx)
        {
            return helper.NewLiteralString(ctx, "<<error>>");
        }

        internal Expr VisitCreateList(CELParser.CreateListContext ctx)
        {
            var listID = helper.Id(ctx.op);
            return helper.NewList(listID, VisitList(ctx.elems));
        }

        internal Expr VisitCreateMessage(CELParser.CreateMessageContext ctx)
        {
            var target = ExprVisit(ctx.member());
            var objID = helper.Id(ctx.op);
            var messageName = ExtractQualifiedName(target);
            if (!ReferenceEquals(messageName, null))
            {
                var entries = VisitIFieldInitializerList(ctx.entries);
                return helper.NewObject(objID, messageName, entries);
            }

            return helper.NewExpr(objID);
        }

        internal Expr VisitCreateStruct(CELParser.CreateStructContext ctx)
        {
            var structID = helper.Id(ctx.op);
            if (ctx.entries != null)
                return helper.NewMap(structID, VisitMapInitializerList(ctx.entries));
            return helper.NewMap(structID, new List<Expr.Types.CreateStruct.Types.Entry>());
        }

        internal Expr globalCallOrMacro(long exprID, string function, params Expr[] args)
        {
            return globalCallOrMacro(exprID, function, new List<Expr>(args));
        }

        internal Expr globalCallOrMacro(long exprID, string function, IList<Expr> args)
        {
            var expr = expandMacro(exprID, function, null, args);
            if (expr != null) return expr;

            return helper.NewGlobalCall(exprID, function, args);
        }

        internal Expr receiverCallOrMacro(long exprID, string function, Expr target, IList<Expr> args)
        {
            var expr = expandMacro(exprID, function, target, args);
            if (expr != null) return expr;

            return helper.NewReceiverCall(exprID, function, target, args);
        }

        internal Expr expandMacro(long exprID, string function, Expr target, IList<Expr> args)
        {
            var macro = outerInstance.options.GetMacro(Macro.MakeMacroKey(function, args.Count, target != null));
            if (macro == null)
            {
                macro = outerInstance.options.GetMacro(Macro.MakeVarArgMacroKey(function, target != null));
                if (macro == null) return null;
            }

            var eh = new ExprHelperImpl(helper, exprID);
            try
            {
                var expander = macro.Expander();
                return expander(eh, target, args);
            }
            catch (ErrorWithLocation err)
            {
                var loc = err.Location;
                if (loc == null) loc = helper.GetLocation(exprID);

                return ReportError(loc, err.Message);
            }
            catch (Exception e)
            {
                return ReportError(helper.GetLocation(exprID), e.Message);
            }
        }

        internal ByteString unquoteBytes(object ctx, string value)
        {
            try
            {
                var buf = Unescape.DoUnescape(value, true);
                return ByteString.CopyFrom(buf.ToArray());
            }
            catch (Exception e)
            {
                ReportError(ctx, e.ToString());
                return ByteString.CopyFromUtf8(value);
            }
        }

        internal string unquoteString(object ctx, string value)
        {
            try
            {
                var buf = Unescape.DoUnescape(value, false);

                return Unescape.ToUtf8(buf);
            }
            catch (Exception e)
            {
                ReportError(ctx, e.ToString());
                return value;
            }
        }
    }
}