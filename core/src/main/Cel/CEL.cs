using System.Collections.Generic;
using Cel.Common;
using Cel.Interpreter;
using Cel.Parser;

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
namespace Cel
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.Source.newInfoSource;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Activation.emptyActivation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Activation.newPartialActivation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.AttributeFactory.newAttributeFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.AttributePattern.newAttributePattern;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.AttributePattern.newPartialAttributeFactory;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Dispatcher.newDispatcher;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.EvalState.newEvalState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Interpreter.exhaustiveEval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Interpreter.newInterpreter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Interpreter.optimize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Interpreter.trackState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.parser.Unparser.unparse;

	using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using ParsedExpr = Google.Api.Expr.V1Alpha1.ParsedExpr;
	using Reference = Google.Api.Expr.V1Alpha1.Reference;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
	using Type = Google.Api.Expr.V1Alpha1.Type;
	using Activation = Cel.Interpreter.Activation;
	using Activation_PartialActivation = Cel.Interpreter.Activation_PartialActivation;
	using AttributeFactory = Cel.Interpreter.AttributeFactory;
	using AttributePattern = Cel.Interpreter.AttributePattern;
	using Coster = Cel.Interpreter.Coster;
	using Coster_Cost = Cel.Interpreter.Coster_Cost;
	using Dispatcher = Cel.Interpreter.Dispatcher;
	using InterpretableDecorator = Cel.Interpreter.InterpretableDecorator;

	public sealed class CEL
	{

	  /// <summary>
	  /// newProgram creates a program instance with an environment, an ast, and an optional list of
	  /// ProgramOption values.
	  /// 
	  /// <para>If the program cannot be configured the prog will be nil, with a non-nil error response.
	  /// </para>
	  /// </summary>
	  public static Program NewProgram(Env e, Ast ast, params ProgramOption[] opts)
	  {
		// Build the dispatcher, interpreter, and default program value.
		Dispatcher disp = Dispatcher.NewDispatcher();

		// Ensure the default attribute factory is set after the adapter and provider are
		// configured.
		Prog p = new Prog(e, disp);

		// Configure the program via the ProgramOption values.
		foreach (ProgramOption opt in opts)
		{
		  if (opt == null)
		  {
			throw new System.NullReferenceException("program options should be non-nil");
		  }
		  p = opt.Apply(p);
		  if (p == null)
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			throw new System.NullReferenceException(string.Format("program option of type '{0}' returned null", opt.GetType().FullName));
		  }
		}

		// Set the attribute factory after the options have been set.
		if (p.evalOpts.Contains(EvalOption.OptPartialEval))
		{
		  p.attrFactory = AttributePattern.NewPartialAttributeFactory(e.Container, e.TypeAdapter, e.TypeProvider);
		}
		else
		{
		  p.attrFactory = AttributeFactory.NewAttributeFactory(e.Container, e.TypeAdapter, e.TypeProvider);
		}

		Interpreter.Interpreter interp = Interpreter.Interpreter.NewInterpreter(disp, e.Container, e.TypeProvider, e.TypeAdapter, p.attrFactory);
		p.interpreter = interp;

		// Translate the EvalOption flags into InterpretableDecorator instances.
		IList<InterpretableDecorator> decorators = new List<InterpretableDecorator>(p.decorators);

		// Enable constant folding first.
		if (p.evalOpts.Contains(EvalOption.OptOptimize))
		{
		  decorators.Add(Interpreter.Interpreter.Optimize());
		}

		Prog pp = p;

		// Enable exhaustive eval over state tracking since it offers a superset of features.
		if (p.evalOpts.Contains(EvalOption.OptExhaustiveEval))
		{
		  // State tracking requires that each Eval() call operate on an isolated EvalState
		  // object; hence, the presence of the factory.
		  ProgFactory factory = state =>
		  {
			IList<InterpretableDecorator> decs = new List<InterpretableDecorator>(decorators);
			decs.Add(Interpreter.Interpreter.ExhaustiveEval(state));
			Prog clone = new Prog(e, pp.evalOpts, pp.defaultVars, disp, interp, state);
			return InitInterpretable(clone, ast, decs);
		  };
		  return InitProgGen(factory);
		}
		else if (p.evalOpts.Contains(EvalOption.OptTrackState))
		{
		  // Enable state tracking last since it too requires the factory approach but is less
		  // featured than the ExhaustiveEval decorator.
		  ProgFactory factory = state =>
		  {
			IList<InterpretableDecorator> decs = new List<InterpretableDecorator>(decorators);
			decs.Add(Interpreter.Interpreter.TrackState(state));
			Prog clone = new Prog(e, pp.evalOpts, pp.defaultVars, disp, interp, state);
			return InitInterpretable(clone, ast, decs);
		  };
		  return InitProgGen(factory);
		}
		return InitInterpretable(p, ast, decorators);
	  }

	  /// <summary>
	  /// initProgGen tests the factory object by calling it once and returns a factory-based Program if
	  /// the test is successful.
	  /// </summary>
	  private static Program InitProgGen(ProgFactory factory)
	  {
		// Test the factory to make sure that configuration errors are spotted at config
		factory(EvalState.NewEvalState());
		return new ProgGen(factory);
	  }

	  /// <summary>
	  /// initIterpretable creates a checked or unchecked interpretable depending on whether the Ast has
	  /// been run through the type-checker.
	  /// </summary>
	  private static Program InitInterpretable(Prog p, Ast ast, IList<InterpretableDecorator> decorators)
	  {

		InterpretableDecorator[] decs = ((List<InterpretableDecorator>)decorators).ToArray();

		// Unchecked programs do not contain type and reference information and may be
		// slower to execute than their checked counterparts.
		if (!ast.Checked)
		{
		  p.interpretable = p.interpreter.NewUncheckedInterpretable(ast.Expr, decs);
		  return p;
		}
		// When the AST has been checked it contains metadata that can be used to speed up program
		// execution.
		CheckedExpr @checked = AstToCheckedExpr(ast);
		p.interpretable = p.interpreter.NewInterpretable(@checked, decs);

		return p;
	  }

	  /// <summary>
	  /// CheckedExprToAst converts a checked expression proto message to an Ast. </summary>
	  public static Ast CheckedExprToAst(CheckedExpr checkedExpr)
	  {
		IDictionary<long, Reference> refMap = checkedExpr.ReferenceMap;
		IDictionary<long, Type> typeMap = checkedExpr.TypeMap;
		return new Ast(checkedExpr.Expr, checkedExpr.SourceInfo, Source.NewInfoSource(checkedExpr.SourceInfo), refMap, typeMap);
	  }

	  /// <summary>
	  /// AstToCheckedExpr converts an Ast to an protobuf CheckedExpr value.
	  /// 
	  /// <para>If the Ast.IsChecked() returns false, this conversion method will return an error.
	  /// </para>
	  /// </summary>
	  public static CheckedExpr AstToCheckedExpr(Ast a)
	  {
		if (!a.Checked)
		{
		  throw new System.ArgumentException("cannot convert unchecked ast");
		}

		CheckedExpr checkedExpr = new CheckedExpr();
		checkedExpr.Expr = a.Expr;
		checkedExpr.SourceInfo = a.SourceInfo;
		checkedExpr.ReferenceMap.Add(a.refMap);
		checkedExpr.TypeMap.Add(a.typeMap);
		return checkedExpr;
	  }

	  /// <summary>
	  /// ParsedExprToAst converts a parsed expression proto message to an Ast. </summary>
	  public static Ast ParsedExprToAst(ParsedExpr parsedExpr)
	  {
		SourceInfo si = parsedExpr.SourceInfo;
		return new Ast(parsedExpr.Expr, si, Source.NewInfoSource(si));
	  }

	  /// <summary>
	  /// AstToParsedExpr converts an Ast to an protobuf ParsedExpr value. </summary>
	  public static ParsedExpr AstToParsedExpr(Ast a)
	  {
		  ParsedExpr parsedExpr = new ParsedExpr();
		  parsedExpr.Expr = a.Expr;
		  parsedExpr.SourceInfo = a.SourceInfo;
		  return parsedExpr;
	  }

	  /// <summary>
	  /// AstToString converts an Ast back to a string if possible.
	  /// 
	  /// <para>Note, the conversion may not be an exact replica of the original expression, but will
	  /// produce a string that is semantically equivalent and whose textual representation is stable.
	  /// </para>
	  /// </summary>
	  public static string AstToString(Ast a)
	  {
		Expr expr = a.Expr;
		SourceInfo info = a.SourceInfo;
		return Unparser.Unparse(expr, info);
	  }

	  /// <summary>
	  /// NoVars returns an empty Activation. </summary>
	  public static Activation NoVars()
	  {
		return Activation.EmptyActivation();
	  }

	  /// <summary>
	  /// PartialVars returns a PartialActivation which contains variables and a set of AttributePattern
	  /// values that indicate variables or parts of variables whose value are not yet known.
	  /// 
	  /// <para>The `vars` value may either be an interpreter.Activation or any valid input to the
	  /// interpreter.NewActivation call.
	  /// </para>
	  /// </summary>
	  public static Activation_PartialActivation PartialVars(object vars, params AttributePattern[] unknowns)
	  {
		return Activation.NewPartialActivation(vars, unknowns);
	  }

	  /// <summary>
	  /// AttributePattern returns an AttributePattern that matches a top-level variable. The pattern is
	  /// mutable, and its methods support the specification of one or more qualifier patterns.
	  /// 
	  /// <para>For example, the AttributePattern(`a`).QualString(`b`) represents a variable access `a` with
	  /// a string field or index qualification `b`. This pattern will match Attributes `a`, and `a.b`,
	  /// but not `a.c`.
	  /// 
	  /// </para>
	  /// <para>When using a CEL expression within a container, e.g. a package or namespace, the variable
	  /// name in the pattern must match the qualified name produced during the variable namespace
	  /// resolution. For example, when variable `a` is declared within an expression whose container is
	  /// `ns.app`, the fully qualified variable name may be `ns.app.a`, `ns.a`, or `a` per the CEL
	  /// namespace resolution rules. Pick the fully qualified variable name that makes sense within the
	  /// container as the AttributePattern `varName` argument.
	  /// 
	  /// </para>
	  /// <para>See the interpreter.AttributePattern and interpreter.AttributeQualifierPattern for more info
	  /// about how to create and manipulate AttributePattern values.
	  /// </para>
	  /// </summary>
	  public static AttributePattern NewAttributePattern(string varName)
	  {
		return AttributePattern.NewAttributePattern(varName);
	  }

	  /// <summary>
	  /// EstimateCost returns the heuristic cost interval for the program. </summary>
	  public static Coster_Cost EstimateCost(object p)
	  {
		if (p is Coster)
		{
		  return ((Coster) p).Cost();
		}
		return Coster_Cost.Unknown;
	  }
	}

}