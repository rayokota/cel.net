using System;
using System.Collections;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.parser.Macro.AllMacros;

	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
	using ByteString = com.google.protobuf.ByteString;
	using NullValue = com.google.protobuf.NullValue;
	using ErrorWithLocation = Cel.Common.ErrorWithLocation;
	using Errors = Cel.Common.Errors;
	using Location = Cel.Common.Location;
	using Source = Cel.Common.Source;
	using Operator = Cel.Common.Operators.Operator;
	using Balancer = Cel.Parser.Helper.Balancer;
	using CELLexer = Cel.Parser.Gen.CELLexer;
	using CELParser = Cel.Parser.Gen.CELParser;
	using BoolFalseContext = Cel.Parser.Gen.CELParser.BoolFalseContext;
	using BoolTrueContext = Cel.Parser.Gen.CELParser.BoolTrueContext;
	using BytesContext = Cel.Parser.Gen.CELParser.BytesContext;
	using CalcContext = Cel.Parser.Gen.CELParser.CalcContext;
	using ConditionalAndContext = Cel.Parser.Gen.CELParser.ConditionalAndContext;
	using ConditionalOrContext = Cel.Parser.Gen.CELParser.ConditionalOrContext;
	using ConstantLiteralContext = Cel.Parser.Gen.CELParser.ConstantLiteralContext;
	using CreateListContext = Cel.Parser.Gen.CELParser.CreateListContext;
	using CreateMessageContext = Cel.Parser.Gen.CELParser.CreateMessageContext;
	using CreateStructContext = Cel.Parser.Gen.CELParser.CreateStructContext;
	using DoubleContext = Cel.Parser.Gen.CELParser.DoubleContext;
	using ExprContext = Cel.Parser.Gen.CELParser.ExprContext;
	using ExprListContext = Cel.Parser.Gen.CELParser.ExprListContext;
	using FieldInitializerListContext = Cel.Parser.Gen.CELParser.FieldInitializerListContext;
	using IdentOrGlobalCallContext = Cel.Parser.Gen.CELParser.IdentOrGlobalCallContext;
	using IndexContext = Cel.Parser.Gen.CELParser.IndexContext;
	using IntContext = Cel.Parser.Gen.CELParser.IntContext;
	using LogicalNotContext = Cel.Parser.Gen.CELParser.LogicalNotContext;
	using MapInitializerListContext = Cel.Parser.Gen.CELParser.MapInitializerListContext;
	using MemberExprContext = Cel.Parser.Gen.CELParser.MemberExprContext;
	using NegateContext = Cel.Parser.Gen.CELParser.NegateContext;
	using NestedContext = Cel.Parser.Gen.CELParser.NestedContext;
	using NullContext = Cel.Parser.Gen.CELParser.NullContext;
	using PrimaryExprContext = Cel.Parser.Gen.CELParser.PrimaryExprContext;
	using RelationContext = Cel.Parser.Gen.CELParser.RelationContext;
	using SelectOrCallContext = Cel.Parser.Gen.CELParser.SelectOrCallContext;
	using StartContext = Cel.Parser.Gen.CELParser.StartContext;
	using StringContext = Cel.Parser.Gen.CELParser.StringContext;
	using UintContext = Cel.Parser.Gen.CELParser.UintContext;
	using UnaryContext = Cel.Parser.Gen.CELParser.UnaryContext;
	using CommonTokenStream = Antlr4.Runtime.CommonTokenStream;
	using ANTLRErrorListener = Antlr4.Runtime.IAntlrErrorListener;
	using DefaultErrorStrategy = org.projectnessie.cel.shaded.org.antlr.v4.runtime.DefaultErrorStrategy;
	using IntStream = org.projectnessie.cel.shaded.org.antlr.v4.runtime.IntStream;
	using ParserRuleContext = org.projectnessie.cel.shaded.org.antlr.v4.runtime.ParserRuleContext;
	using RecognitionException = org.projectnessie.cel.shaded.org.antlr.v4.runtime.RecognitionException;
	using Recognizer = org.projectnessie.cel.shaded.org.antlr.v4.runtime.Recognizer;
	using RuleContext = org.projectnessie.cel.shaded.org.antlr.v4.runtime.RuleContext;
	using Token = Antlr4.Runtime.IToken;
	using ATNConfigSet = org.projectnessie.cel.shaded.org.antlr.v4.runtime.atn.ATNConfigSet;
	using DFA = org.projectnessie.cel.shaded.org.antlr.v4.runtime.dfa.DFA;
	using AbstractParseTreeVisitor = org.projectnessie.cel.shaded.org.antlr.v4.runtime.tree.AbstractParseTreeVisitor;
	using ErrorNode = org.projectnessie.cel.shaded.org.antlr.v4.runtime.tree.ErrorNode;
	using ParseTree = org.projectnessie.cel.shaded.org.antlr.v4.runtime.tree.ParseTree;
	using ParseTreeListener = org.projectnessie.cel.shaded.org.antlr.v4.runtime.tree.ParseTreeListener;
	using TerminalNode = org.projectnessie.cel.shaded.org.antlr.v4.runtime.tree.TerminalNode;

	public sealed class Parser
	{

	  private static readonly ISet<string> reservedIds = Collections.unmodifiableSet(new HashSet<string>(Arrays.asList("as", "break", "const", "continue", "else", "false", "for", "function", "if", "import", "in", "let", "loop", "package", "namespace", "null", "return", "true", "var", "void", "while")));

	  private readonly Options options;

	  public static ParseResult parseAllMacros(Source source)
	  {
		return parse(Options.builder().macros(AllMacros).build(), source);
	  }

	  public static ParseResult parseWithMacros(Source source, IList<Macro> macros)
	  {
		return parse(Options.builder().macros(macros).build(), source);
	  }

	  public static ParseResult parse(Options options, Source source)
	  {
		return (new Parser(options)).parse(source);
	  }

	  internal Parser(Options options)
	  {
		this.options = options;
	  }

	  internal ParseResult parse(Source source)
	  {
		StringCharStream charStream = new StringCharStream(source.content(), source.description());
		CELLexer lexer = new CELLexer(charStream);
		CELParser parser = new CELParser(new CommonTokenStream(lexer, 0));

		RecursionListener parserListener = new RecursionListener(options.MaxRecursionDepth);

		parser.addParseListener(parserListener);

		parser.setErrorHandler(new RecoveryLimitErrorStrategy(options.ErrorRecoveryLimit));

		Helper helper = new Helper(source);
		Errors errors = new Errors(source);

		InnerParser inner = new InnerParser(this, helper, errors);

		lexer.addErrorListener(inner);
		parser.addErrorListener(inner);

		Expr expr = null;
		try
		{
		  if (charStream.size() > options.ExpressionSizeCodePointLimit)
		  {
			errors.reportError(Location.NoLocation, "expression code point size exceeds limit: size: %d, limit %d", charStream.size(), options.ExpressionSizeCodePointLimit);
		  }
		  else
		  {
			expr = inner.exprVisit(parser.start());
		  }
		}
		catch (Exception e) when (e is RecoveryLimitError || e is RecursionError)
		{
		  errors.reportError(Location.NoLocation, "%s", e.getMessage());
		}

		if (errors.hasErrors())
		{
		  expr = null;
		}

		return new ParseResult(expr, errors, helper.SourceInfo);
	  }

	  public sealed class ParseResult
	  {
		internal readonly Expr expr;
		internal readonly Errors errors;
		internal readonly SourceInfo sourceInfo;

		public ParseResult(Expr expr, Errors errors, SourceInfo sourceInfo)
		{
		  this.expr = expr;
		  this.errors = errors;
		  this.sourceInfo = sourceInfo;
		}

		public Expr Expr
		{
			get
			{
			  return expr;
			}
		}

		public Errors Errors
		{
			get
			{
			  return errors;
			}
		}

		public SourceInfo SourceInfo
		{
			get
			{
			  return sourceInfo;
			}
		}

		public bool hasErrors()
		{
		  return errors.hasErrors();
		}
	  }

	  internal sealed class RecursionListener : ParseTreeListener
	  {
		internal readonly int maxDepth;
		internal int depth;

		internal RecursionListener(int maxDepth)
		{
		  this.maxDepth = maxDepth;
		}

		public override void visitTerminal(TerminalNode node)
		{
		}
		public override void visitErrorNode(ErrorNode node)
		{
		}
		public override void enterEveryRule(ParserRuleContext ctx)
		{
		  if (ctx != null && ctx.getRuleIndex() == CELParser.RULE_expr)
		  {
			if (this.depth >= this.maxDepth)
			{
			  this.depth++;
			  throw new RecursionError(string.Format("expression recursion limit exceeded: {0:D}", maxDepth));
			}
			this.depth++;
		  }
		}

		public override void exitEveryRule(ParserRuleContext ctx)
		{
		  if (ctx != null && ctx.getRuleIndex() == CELParser.RULE_expr)
		  {
			depth--;
		  }
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
//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
//ORIGINAL LINE: public RecoveryLimitError(String message, org.projectnessie.cel.shaded.org.antlr.v4.runtime.Recognizer<?, ?> recognizer, org.projectnessie.cel.shaded.org.antlr.v4.runtime.IntStream input, org.projectnessie.cel.shaded.org.antlr.v4.runtime.ParserRuleContext ctx)
		public RecoveryLimitError(string message, Recognizer<T1, T2> recognizer, IntStream input, ParserRuleContext ctx) : base(message, recognizer, input, ctx)
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

		public override void recover(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer, RecognitionException e)
		{
		  checkAttempts(recognizer);
		  base.recover(recognizer, e);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public org.projectnessie.cel.shaded.org.antlr.v4.runtime.Token recoverInline(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer) throws org.projectnessie.cel.shaded.org.antlr.v4.runtime.RecognitionException
		public override Token recoverInline(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer)
		{
		  checkAttempts(recognizer);
		  return base.recoverInline(recognizer);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkAttempts(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer) throws org.projectnessie.cel.shaded.org.antlr.v4.runtime.RecognitionException
		internal void checkAttempts(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer)
		{
		  if (attempts >= maxAttempts)
		  {
			attempts++;
			string msg = string.Format("error recovery attempt limit exceeded: {0:D}", maxAttempts);
			recognizer.notifyErrorListeners(null, msg, null);
			throw new RecoveryLimitError(msg, recognizer, null, null);
		  }
		  attempts++;
		}
	  }

	  internal sealed class InnerParser : AbstractParseTreeVisitor<object>, ANTLRErrorListener
	  {
		  private readonly Parser outerInstance;


		internal readonly Helper helper;
		internal readonly Errors errors;

		internal InnerParser(Parser outerInstance, Helper helper, Errors errors)
		{
			this.outerInstance = outerInstance;
		  this.helper = helper;
		  this.errors = errors;
		}

		public override void syntaxError<T1, T2>(Recognizer<T1, T2> recognizer, object offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
		{
		  errors.syntaxError(Location.newLocation(line, charPositionInLine), msg);
		}

		public override void reportAmbiguity(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitArray ambigAlts, ATNConfigSet configs)
		{
		  // empty
		}

		public override void reportAttemptingFullContext(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitArray conflictingAlts, ATNConfigSet configs)
		{
		  // empty
		}

		public override void reportContextSensitivity(org.projectnessie.cel.shaded.org.antlr.v4.runtime.Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, ATNConfigSet configs)
		{
		  // empty
		}

		internal Expr reportError(object ctx, string message)
		{
		  return reportError(ctx, "%s", message);
		}

		internal Expr reportError(object ctx, string format, params object[] args)
		{
		  Location location;
		  if (ctx is Location)
		  {
			location = (Location) ctx;
		  }
		  else if (ctx is Token || ctx is ParserRuleContext)
		  {
			Expr err = helper.newExpr(ctx);
			location = helper.getLocation(err.getId());
		  }
		  else
		  {
			location = Location.NoLocation;
		  }
		  Expr err = helper.newExpr(ctx);
		  // Provide arguments to the report error.
		  errors.reportError(location, format, args);
		  return err;
		}

		public Expr exprVisit(ParseTree tree)
		{
		  object r = visit(tree);
		  return (Expr) r;
		}

		public override object visit(ParseTree tree)
		{
		  if (tree is RuleContext)
		  {
			RuleContext ruleContext = (RuleContext) tree;
			int ruleIndex = ruleContext.getRuleIndex();
			switch (ruleIndex)
			{
			  case CELParser.RULE_start:
				return visitStart((CELParser.StartContext) tree);
			  case CELParser.RULE_expr:
				return visitExpr((CELParser.ExprContext) tree);
			  case CELParser.RULE_conditionalOr:
				return visitConditionalOr((CELParser.ConditionalOrContext) tree);
			  case CELParser.RULE_conditionalAnd:
				return visitConditionalAnd((CELParser.ConditionalAndContext) tree);
			  case CELParser.RULE_relation:
				return visitRelation((CELParser.RelationContext) tree);
			  case CELParser.RULE_calc:
				return visitCalc((CELParser.CalcContext) tree);
			  case CELParser.RULE_unary:
				if (tree is LogicalNotContext)
				{
				  return visitLogicalNot((CELParser.LogicalNotContext) tree);
				}
				else if (tree is NegateContext)
				{
				  return visitNegate((CELParser.NegateContext) tree);
				}
				else if (tree is MemberExprContext)
				{
				  return visitMemberExpr((CELParser.MemberExprContext) tree);
				}
				return visitUnary((CELParser.UnaryContext) tree);
			  case CELParser.RULE_member:
				if (tree is CreateMessageContext)
				{
				  return visitCreateMessage((CELParser.CreateMessageContext) tree);
				}
				else if (tree is PrimaryExprContext)
				{
				  return visitPrimaryExpr((CELParser.PrimaryExprContext) tree);
				}
				else if (tree is SelectOrCallContext)
				{
				  return visitSelectOrCall((CELParser.SelectOrCallContext) tree);
				}
				else if (tree is IndexContext)
				{
				  return visitIndex((CELParser.IndexContext) tree);
				}
				break;
			  case CELParser.RULE_primary:
				if (tree is CreateListContext)
				{
				  return visitCreateList((CELParser.CreateListContext) tree);
				}
				else if (tree is CreateStructContext)
				{
				  return visitCreateStruct((CELParser.CreateStructContext) tree);
				}
				break;
			  case CELParser.RULE_fieldInitializerList:
			  case CELParser.RULE_mapInitializerList:
				return visitMapInitializerList((CELParser.MapInitializerListContext) tree);
				// case CELParser.RULE_exprList:
				// case CELParser.RULE_literal:
			  default:
				return reportError(tree, "parser rule '%d'", ruleIndex);
			}
		  }

		  // Report at least one error if the parser reaches an unknown parse element.
		  // Typically, this happens if the parser has already encountered a syntax error elsewhere.
		  if (!errors.hasErrors())
		  {
			string txt = "<<nil>>";
			if (tree != null)
			{
			  txt = string.Format("<<{0}>>", tree.GetType().Name);
			}
			return reportError(Location.NoLocation, "unknown parse element encountered: %s", txt);
		  }
		  return helper.newExpr(Location.NoLocation);
		}

		internal object visitStart(CELParser.StartContext ctx)
		{
		  return visit(ctx.expr());
		}

		internal Expr visitExpr(CELParser.ExprContext ctx)
		{
		  Expr result = exprVisit(ctx.e);
		  if (ctx.op == null)
		  {
			return result;
		  }
		  long opID = helper.id(ctx.op);
		  Expr ifTrue = exprVisit(ctx.e1);
		  Expr ifFalse = exprVisit(ctx.e2);
		  return globalCallOrMacro(opID, Operator.Conditional.id, result, ifTrue, ifFalse);
		}

		internal Expr visitConditionalAnd(CELParser.ConditionalAndContext ctx)
		{
		  Expr result = exprVisit(ctx.e);
		  if (ctx.ops == null || ctx.ops.isEmpty())
		  {
			return result;
		  }
		  Balancer b = helper.newBalancer(Operator.LogicalAnd.id, result);
		  IList<CELParser.RelationContext> rest = ctx.e1;
		  for (int i = 0; i < ctx.ops.size(); i++)
		  {
			Token op = ctx.ops.get(i);
			if (i >= rest.Count)
			{
			  return reportError(ctx, "unexpected character, wanted '&&'");
			}
			Expr next = exprVisit(rest[i]);
			long opID = helper.id(op);
			b.addTerm(opID, next);
		  }
		  return b.balance();
		}

		internal Expr visitConditionalOr(CELParser.ConditionalOrContext ctx)
		{
		  Expr result = exprVisit(ctx.e);
		  if (ctx.ops == null || ctx.ops.isEmpty())
		  {
			return result;
		  }
		  Balancer b = helper.newBalancer(Operator.LogicalOr.id, result);
		  IList<CELParser.ConditionalAndContext> rest = ctx.e1;
		  for (int i = 0; i < ctx.ops.size(); i++)
		  {
			Token op = ctx.ops.get(i);
			if (i >= rest.Count)
			{
			  return reportError(ctx, "unexpected character, wanted '||'");
			}
			Expr next = exprVisit(rest[i]);
			long opID = helper.id(op);
			b.addTerm(opID, next);
		  }
		  return b.balance();
		}

		internal Expr visitRelation(CELParser.RelationContext ctx)
		{
		  if (ctx.calc() != null)
		  {
			return exprVisit(ctx.calc());
		  }
		  string opText = "";
		  if (ctx.op != null)
		  {
			opText = ctx.op.getText();
		  }
		  Operator op = Operator.find(opText);
		  if (op != null)
		  {
			Expr lhs = exprVisit(ctx.relation(0));
			long opID = helper.id(ctx.op);
			Expr rhs = exprVisit(ctx.relation(1));
			return globalCallOrMacro(opID, op.id, lhs, rhs);
		  }
		  return reportError(ctx, "operator not found");
		}

		internal Expr visitCalc(CELParser.CalcContext ctx)
		{
		  if (ctx.unary() != null)
		  {
			return exprVisit(ctx.unary());
		  }
		  string opText = "";
		  if (ctx.op != null)
		  {
			opText = ctx.op.getText();
		  }
		  Operator op = Operator.find(opText);
		  if (op != null)
		  {
			Expr lhs = exprVisit(ctx.calc(0));
			long opID = helper.id(ctx.op);
			Expr rhs = exprVisit(ctx.calc(1));
			return globalCallOrMacro(opID, op.id, lhs, rhs);
		  }
		  return reportError(ctx, "operator not found");
		}

		internal Expr visitLogicalNot(CELParser.LogicalNotContext ctx)
		{
		  if (ctx.ops.size() % 2 == 0)
		  {
			return exprVisit(ctx.member());
		  }
		  long opID = helper.id(ctx.ops.get(0));
		  Expr target = exprVisit(ctx.member());
		  return globalCallOrMacro(opID, Operator.LogicalNot.id, target);
		}

		internal Expr visitMemberExpr(CELParser.MemberExprContext ctx)
		{
		  if (ctx.member() is PrimaryExprContext)
		  {
			return visitPrimaryExpr((CELParser.PrimaryExprContext) ctx.member());
		  }
		  else if (ctx.member() is SelectOrCallContext)
		  {
			return visitSelectOrCall((CELParser.SelectOrCallContext) ctx.member());
		  }
		  else if (ctx.member() is IndexContext)
		  {
			return visitIndex((CELParser.IndexContext) ctx.member());
		  }
		  else if (ctx.member() is CreateMessageContext)
		  {
			return visitCreateMessage((CELParser.CreateMessageContext) ctx.member());
		  }
		  return reportError(ctx, "unsupported simple expression");
		}

		internal Expr visitPrimaryExpr(CELParser.PrimaryExprContext ctx)
		{
		  if (ctx.primary() is NestedContext)
		  {
			return visitNested((CELParser.NestedContext) ctx.primary());
		  }
		  else if (ctx.primary() is IdentOrGlobalCallContext)
		  {
			return visitIdentOrGlobalCall((CELParser.IdentOrGlobalCallContext) ctx.primary());
		  }
		  else if (ctx.primary() is CreateListContext)
		  {
			return visitCreateList((CELParser.CreateListContext) ctx.primary());
		  }
		  else if (ctx.primary() is CreateStructContext)
		  {
			return visitCreateStruct((CELParser.CreateStructContext) ctx.primary());
		  }
		  else if (ctx.primary() is ConstantLiteralContext)
		  {
			return visitConstantLiteral((CELParser.ConstantLiteralContext) ctx.primary());
		  }

		  return reportError(ctx, "invalid primary expression");
		}

		internal Expr visitConstantLiteral(CELParser.ConstantLiteralContext ctx)
		{
		  if (ctx.literal() is IntContext)
		  {
			return visitInt((CELParser.IntContext) ctx.literal());
		  }
		  else if (ctx.literal() is UintContext)
		  {
			return visitUint((CELParser.UintContext) ctx.literal());
		  }
		  else if (ctx.literal() is DoubleContext)
		  {
			return visitDouble((CELParser.DoubleContext) ctx.literal());
		  }
		  else if (ctx.literal() is StringContext)
		  {
			return visitString((CELParser.StringContext) ctx.literal());
		  }
		  else if (ctx.literal() is BytesContext)
		  {
			return visitBytes((CELParser.BytesContext) ctx.literal());
		  }
		  else if (ctx.literal() is BoolFalseContext)
		  {
			return visitBoolFalse((CELParser.BoolFalseContext) ctx.literal());
		  }
		  else if (ctx.literal() is BoolTrueContext)
		  {
			return visitBoolTrue((CELParser.BoolTrueContext) ctx.literal());
		  }
		  else if (ctx.literal() is NullContext)
		  {
			return visitNull((CELParser.NullContext) ctx.literal());
		  }
		  return reportError(ctx, "invalid literal");
		}

		internal Expr visitInt(CELParser.IntContext ctx)
		{
		  string text = ctx.tok.getText();
		  int @base = 10;
		  if (text.StartsWith("0x", StringComparison.Ordinal))
		  {
			@base = 16;
			text = text.Substring(2);
		  }
		  if (ctx.sign != null)
		  {
			text = ctx.sign.getText() + text;
		  }
		  try
		  {
			long i = Long.parseLong(text, @base);
			return helper.newLiteralInt(ctx, i);
		  }
		  catch (Exception)
		  {
			return reportError(ctx, "invalid int literal");
		  }
		}

		internal Expr visitUint(CELParser.UintContext ctx)
		{
		  string text = ctx.tok.getText();
		  // trim the 'u' designator included in the uint literal.
		  text = text.Substring(0, text.Length - 1);
		  int @base = 10;
		  if (text.StartsWith("0x", StringComparison.Ordinal))
		  {
			@base = 16;
			text = text.Substring(2);
		  }
		  try
		  {
			long i = Long.parseUnsignedLong(text, @base);
			return helper.newLiteralUint(ctx, i);
		  }
		  catch (Exception)
		  {
			return reportError(ctx, "invalid int literal");
		  }
		}

		internal Expr visitDouble(CELParser.DoubleContext ctx)
		{
		  string txt = ctx.tok.getText();
		  if (ctx.sign != null)
		  {
			txt = ctx.sign.getText() + txt;
		  }
		  try
		  {
			double f = double.Parse(txt);
			return helper.newLiteralDouble(ctx, f);
		  }
		  catch (Exception)
		  {
			return reportError(ctx, "invalid double literal");
		  }
		}

		internal Expr visitString(CELParser.StringContext ctx)
		{
		  string s = unquoteString(ctx, ctx.getText());
		  return helper.newLiteralString(ctx, s);
		}

		internal Expr visitBytes(CELParser.BytesContext ctx)
		{
		  ByteString b = unquoteBytes(ctx, ctx.tok.getText().Substring(1));
		  return helper.newLiteralBytes(ctx, b);
		}

		internal Expr visitBoolFalse(CELParser.BoolFalseContext ctx)
		{
		  return helper.newLiteralBool(ctx, false);
		}

		internal Expr visitBoolTrue(CELParser.BoolTrueContext ctx)
		{
		  return helper.newLiteralBool(ctx, true);
		}

		internal Expr visitNull(CELParser.NullContext ctx)
		{
		  return helper.newLiteral(ctx, Constant.newBuilder().setNullValue(NullValue.NULL_VALUE));
		}

		internal IList<Expr> visitList(CELParser.ExprListContext ctx)
		{
		  if (ctx == null)
		  {
			return Collections.emptyList();
		  }
		  return visitSlice(ctx.e);
		}

		internal IList<Expr> visitSlice(IList<CELParser.ExprContext> expressions)
		{
		  if (expressions == null)
		  {
			return Collections.emptyList();
		  }
		  IList<Expr> result = new List<Expr>(expressions.Count);
		  foreach (CELParser.ExprContext e in expressions)
		  {
			Expr ex = exprVisit(e);
			result.Add(ex);
		  }
		  return result;
		}

		internal string extractQualifiedName(Expr e)
		{
		  if (e == null)
		  {
			return null;
		  }
		  switch (e.getExprKindCase())
		  {
			case IDENT_EXPR:
			  return e.getIdentExpr().getName();
			case SELECT_EXPR:
			  Expr.Select s = e.getSelectExpr();
			  string prefix = extractQualifiedName(s.getOperand());
			  return prefix + "." + s.getField();
		  }
		  // TODO: Add a method to Source to get location from character offset.
		  Location location = helper.getLocation(e.getId());
		  reportError(location, "expected a qualified name");
		  return null;
		}

		// Visit a parse tree of field initializers.
		internal IList<Expr.CreateStruct.Entry> visitIFieldInitializerList(CELParser.FieldInitializerListContext ctx)
		{
		  if (ctx == null || ctx.fields == null)
		  {
			// This is the result of a syntax error handled elswhere, return empty.
			return Collections.emptyList();
		  }

		  IList<Expr.CreateStruct.Entry> result = new List<Expr.CreateStruct.Entry>(ctx.fields.size());
		  IList<Token> cols = ctx.cols;
		  IList<CELParser.ExprContext> vals = ctx.values;
		  for (int i = 0; i < ctx.fields.size(); i++)
		  {
			Token f = ctx.fields.get(i);
			if (i >= cols.Count || i >= vals.Count)
			{
			  // This is the result of a syntax error detected elsewhere.
			  return Collections.emptyList();
			}
			long initID = helper.id(cols[i]);
			Expr value = exprVisit(vals[i]);
			Expr.CreateStruct.Entry field = helper.newObjectField(initID, f.getText(), value);
			result.Add(field);
		  }
		  return result;
		}

		internal Expr visitIdentOrGlobalCall(CELParser.IdentOrGlobalCallContext ctx)
		{
		  string identName = "";
		  if (ctx.leadingDot != null)
		  {
			identName = ".";
		  }
		  // Handle the error case where no valid identifier is specified.
		  if (ctx.id == null)
		  {
			return helper.newExpr(ctx);
		  }
		  // Handle reserved identifiers.
		  string id = ctx.id.getText();
		  if (reservedIds.Contains(id))
		  {
			return reportError(ctx, "reserved identifier: %s", id);
		  }
		  identName += id;
		  if (ctx.op != null)
		  {
			long opID = helper.id(ctx.op);
			return globalCallOrMacro(opID, identName, visitList(ctx.args));
		  }
		  return helper.newIdent(ctx.id, identName);
		}

		internal Expr visitNested(CELParser.NestedContext ctx)
		{
		  return exprVisit(ctx.e);
		}

		internal Expr visitSelectOrCall(CELParser.SelectOrCallContext ctx)
		{
		  Expr operand = exprVisit(ctx.member());
		  // Handle the error case where no valid identifier is specified.
		  if (ctx.id == null)
		  {
			return helper.newExpr(ctx);
		  }
		  string id = ctx.id.getText();
		  if (ctx.open != null)
		  {
			long opID = helper.id(ctx.open);
			return receiverCallOrMacro(opID, id, operand, visitList(ctx.args));
		  }
		  return helper.newSelect(ctx.op, operand, id);
		}

		internal IList<Expr.CreateStruct.Entry> visitMapInitializerList(CELParser.MapInitializerListContext ctx)
		{
		  if (ctx == null || ctx.keys.isEmpty())
		  {
			// This is the result of a syntax error handled elswhere, return empty.
			return Collections.emptyList();
		  }

		  IList<Expr.CreateStruct.Entry> result = new List<Expr.CreateStruct.Entry>(ctx.cols.size());
		  IList<CELParser.ExprContext> keys = ctx.keys;
		  IList<CELParser.ExprContext> vals = ctx.values;
		  for (int i = 0; i < ctx.cols.size(); i++)
		  {
			Token col = ctx.cols.get(i);
			long colID = helper.id(col);
			if (i >= keys.Count || i >= vals.Count)
			{
			  // This is the result of a syntax error detected elsewhere.
			  return Collections.emptyList();
			}
			Expr key = exprVisit(keys[i]);
			Expr value = exprVisit(vals[i]);
			Expr.CreateStruct.Entry entry = helper.newMapEntry(colID, key, value);
			result.Add(entry);
		  }
		  return result;
		}

		internal Expr visitNegate(CELParser.NegateContext ctx)
		{
		  if (ctx.ops.size() % 2 == 0)
		  {
			return exprVisit(ctx.member());
		  }
		  long opID = helper.id(ctx.ops.get(0));
		  Expr target = exprVisit(ctx.member());
		  return globalCallOrMacro(opID, Operator.Negate.id, target);
		}

		internal Expr visitIndex(CELParser.IndexContext ctx)
		{
		  Expr target = exprVisit(ctx.member());
		  long opID = helper.id(ctx.op);
		  Expr index = exprVisit(ctx.index);
		  return globalCallOrMacro(opID, Operator.Index.id, target, index);
		}

		internal Expr visitUnary(CELParser.UnaryContext ctx)
		{
		  return helper.newLiteralString(ctx, "<<error>>");
		}

		internal Expr visitCreateList(CELParser.CreateListContext ctx)
		{
		  long listID = helper.id(ctx.op);
		  return helper.newList(listID, visitList(ctx.elems));
		}

		internal Expr visitCreateMessage(CELParser.CreateMessageContext ctx)
		{
		  Expr target = exprVisit(ctx.member());
		  long objID = helper.id(ctx.op);
		  string messageName = extractQualifiedName(target);
		  if (!string.ReferenceEquals(messageName, null))
		  {
			IList<Expr.CreateStruct.Entry> entries = visitIFieldInitializerList(ctx.entries);
			return helper.newObject(objID, messageName, entries);
		  }
		  return helper.newExpr(objID);
		}

		internal Expr visitCreateStruct(CELParser.CreateStructContext ctx)
		{
		  long structID = helper.id(ctx.op);
		  if (ctx.entries != null)
		  {
			return helper.newMap(structID, visitMapInitializerList(ctx.entries));
		  }
		  else
		  {
			return helper.newMap(structID, Collections.emptyList());
		  }
		}

		internal Expr globalCallOrMacro(long exprID, string function, params Expr[] args)
		{
		  return globalCallOrMacro(exprID, function, Arrays.asList(args));
		}

		internal Expr globalCallOrMacro(long exprID, string function, IList<Expr> args)
		{
		  Expr expr = expandMacro(exprID, function, null, args);
		  if (expr != null)
		  {
			return expr;
		  }
		  return helper.newGlobalCall(exprID, function, args);
		}

		internal Expr receiverCallOrMacro(long exprID, string function, Expr target, IList<Expr> args)
		{
		  Expr expr = expandMacro(exprID, function, target, args);
		  if (expr != null)
		  {
			return expr;
		  }
		  return helper.newReceiverCall(exprID, function, target, args);
		}

		internal Expr expandMacro(long exprID, string function, Expr target, IList<Expr> args)
		{
		  Macro macro = outerInstance.options.getMacro(Macro.makeMacroKey(function, args.Count, target != null));
		  if (macro == null)
		  {
			macro = outerInstance.options.getMacro(Macro.makeVarArgMacroKey(function, target != null));
			if (macro == null)
			{
			  return null;
			}
		  }

		  ExprHelperImpl eh = new ExprHelperImpl(helper, exprID);
		  try
		  {
			return macro.expander().expand(eh, target, args);
		  }
		  catch (ErrorWithLocation err)
		  {
			Location loc = err.Location;
			if (loc == null)
			{
			  loc = helper.getLocation(exprID);
			}
			return reportError(loc, err.Message);
		  }
		  catch (Exception e)
		  {
			return reportError(helper.getLocation(exprID), e.Message);
		  }
		}

		internal ByteString unquoteBytes(object ctx, string value)
		{
		  try
		  {
			ByteBuffer buf = Unescape.unescape(value, true);
			return ByteString.copyFrom(buf);
		  }
		  catch (Exception e)
		  {
			reportError(ctx, e.ToString());
			return ByteString.copyFromUtf8(value);
		  }
		}

		internal string unquoteString(object ctx, string value)
		{
		  try
		  {
			ByteBuffer buf = Unescape.unescape(value, false);

			return Unescape.toUtf8(buf);
		  }
		  catch (Exception e)
		  {
			reportError(ctx, e.ToString());
			return value;
		  }
		}
	  }
	}

}