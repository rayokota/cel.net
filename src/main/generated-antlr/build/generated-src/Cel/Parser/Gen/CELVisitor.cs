//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.10.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from /Users/ryokota/code/personal/cel-csharp/src/main/generated-antlr/CEL.g4 by ANTLR 4.10.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

namespace Cel.Parser.Gen {
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="CELParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.10.1")]
[System.CLSCompliant(false)]
public interface ICELVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.start"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStart([NotNull] CELParser.StartContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpr([NotNull] CELParser.ExprContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.conditionalOr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionalOr([NotNull] CELParser.ConditionalOrContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.conditionalAnd"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionalAnd([NotNull] CELParser.ConditionalAndContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.relation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelation([NotNull] CELParser.RelationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.calc"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCalc([NotNull] CELParser.CalcContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>MemberExpr</c>
	/// labeled alternative in <see cref="CELParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMemberExpr([NotNull] CELParser.MemberExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>LogicalNot</c>
	/// labeled alternative in <see cref="CELParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalNot([NotNull] CELParser.LogicalNotContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Negate</c>
	/// labeled alternative in <see cref="CELParser.unary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNegate([NotNull] CELParser.NegateContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>SelectOrCall</c>
	/// labeled alternative in <see cref="CELParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelectOrCall([NotNull] CELParser.SelectOrCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>PrimaryExpr</c>
	/// labeled alternative in <see cref="CELParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimaryExpr([NotNull] CELParser.PrimaryExprContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Index</c>
	/// labeled alternative in <see cref="CELParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIndex([NotNull] CELParser.IndexContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateMessage</c>
	/// labeled alternative in <see cref="CELParser.member"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateMessage([NotNull] CELParser.CreateMessageContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>IdentOrGlobalCall</c>
	/// labeled alternative in <see cref="CELParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentOrGlobalCall([NotNull] CELParser.IdentOrGlobalCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Nested</c>
	/// labeled alternative in <see cref="CELParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNested([NotNull] CELParser.NestedContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateList</c>
	/// labeled alternative in <see cref="CELParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateList([NotNull] CELParser.CreateListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>CreateStruct</c>
	/// labeled alternative in <see cref="CELParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCreateStruct([NotNull] CELParser.CreateStructContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ConstantLiteral</c>
	/// labeled alternative in <see cref="CELParser.primary"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstantLiteral([NotNull] CELParser.ConstantLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.exprList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExprList([NotNull] CELParser.ExprListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.fieldInitializerList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFieldInitializerList([NotNull] CELParser.FieldInitializerListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="CELParser.mapInitializerList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMapInitializerList([NotNull] CELParser.MapInitializerListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Int</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInt([NotNull] CELParser.IntContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Uint</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUint([NotNull] CELParser.UintContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Double</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDouble([NotNull] CELParser.DoubleContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>String</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitString([NotNull] CELParser.StringContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Bytes</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBytes([NotNull] CELParser.BytesContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>BoolTrue</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolTrue([NotNull] CELParser.BoolTrueContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>BoolFalse</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolFalse([NotNull] CELParser.BoolFalseContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>Null</c>
	/// labeled alternative in <see cref="CELParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNull([NotNull] CELParser.NullContext context);
}
} // namespace Cel.Parser.Gen
