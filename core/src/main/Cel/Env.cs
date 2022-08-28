using System;
using System.Collections.Generic;
using Cel.Common;
using Cel.Common.Types.Pb;
using Cel.Interpreter;

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
//	import static Cel.CEL.astToParsedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.CEL.astToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.CEL.newProgram;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.CEL.parsedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.CEL.partialVars;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.EnvOption_EnvFeature.FeatureDisableDynamicAggregateLiterals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Issues.newIssues;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Library.StdLib;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.Location.NoLocation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.Source.newTextSource;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.containers.Container.defaultContainer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.pb.ProtoTypeRegistry.newRegistry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.Activation.emptyActivation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.AstPruner.pruneAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.interpreter.AttributePattern.newAttributePattern;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.parser.Parser.parseWithMacros;

	using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
	using Decl = Google.Api.Expr.V1Alpha1.Decl;
	using DeclKindCase = Google.Api.Expr.V1Alpha1.Decl.DeclKindOneofCase;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using ParsedExpr = Google.Api.Expr.V1Alpha1.ParsedExpr;
	using CheckResult = global::Cel.Checker.Checker.CheckResult;
	using CheckerEnv = global::Cel.Checker.CheckerEnv;
	using Errors = global::Cel.Common.Errors;
	using Source = global::Cel.Common.Source;
	using Container = global::Cel.Common.Containers.Container;
	using TypeAdapter = global::Cel.Common.Types.Ref.TypeAdapter;
	using TypeProvider = global::Cel.Common.Types.Ref.TypeProvider;
	using TypeRegistry = global::Cel.Common.Types.Ref.TypeRegistry;
	using Activation_PartialActivation = global::Cel.Interpreter.Activation_PartialActivation;
	using AttributePattern = global::Cel.Interpreter.AttributePattern;
	using Macro = global::Cel.Parser.Macro;
	using ParseResult = global::Cel.Parser.Parser.ParseResult;
	using Parser = global::Cel.Parser.Parser;

	/// <summary>
	/// Env encapsulates the context necessary to perform parsing, type checking, or generation of
	/// evaluable programs for different expressions.
	/// </summary>
	public sealed class Env
	{

	  internal Container container;
	  internal readonly IList<Decl> declarations;
	  internal readonly IList<Macro> macros;
	  internal TypeAdapter adapter;
	  internal TypeProvider provider;
	  private readonly ISet<EnvOption_EnvFeature> features;
	  /// <summary>
	  /// program options tied to the environment. </summary>
	  private readonly IList<ProgramOption> progOpts;

	  /// <summary>
	  /// Internal checker representation </summary>
	  private CheckerEnv chk;

	  private Exception chkErr;
	  private readonly object once = new object();

	  private Env(Container container, IList<Decl> declarations, IList<Macro> macros, TypeAdapter adapter, TypeProvider provider, ISet<EnvOption_EnvFeature> features, IList<ProgramOption> progOpts)
	  {
		this.container = container;
		this.declarations = declarations;
		this.macros = macros;
		this.adapter = adapter;
		this.provider = provider;
		this.features = features;
		this.progOpts = progOpts;
	  }

	  /// <summary>
	  /// NewEnv creates a program environment configured with the standard library of CEL functions and
	  /// macros. The Env value returned can parse and check any CEL program which builds upon the core
	  /// features documented in the CEL specification.
	  /// 
	  /// <para>See the EnvOption helper functions for the options that can be used to configure the
	  /// environment.
	  /// </para>
	  /// </summary>
	  public static Env NewEnv(params EnvOption[] opts)
	  {
		List<EnvOption> stdOpts = new List<EnvOption>(opts.Length + 1);
		stdOpts.Add(Library.StdLib());
		stdOpts.AddRange(opts);
		return NewCustomEnv(((List<EnvOption>)stdOpts).ToArray());
	  }

	  /// <summary>
	  /// NewCustomEnv creates a custom program environment which is not automatically configured with
	  /// the standard library of functions and macros documented in the CEL spec.
	  /// 
	  /// <para>The purpose for using a custom environment might be for subsetting the standard library
	  /// produced by the cel.StdLib() function. Subsetting CEL is a core aspect of its design that
	  /// allows users to limit the compute and memory impact of a CEL program by controlling the
	  /// functions and macros that may appear in a given expression.
	  /// 
	  /// </para>
	  /// <para>See the EnvOption helper functions for the options that can be used to configure the
	  /// environment.
	  /// </para>
	  /// </summary>
	  public static Env NewCustomEnv(TypeRegistry registry, IList<EnvOption> opts)
	  {
		return (new Env(Container.DefaultContainer, new List<Decl>(), new List<Macro>(), registry.ToTypeAdapter(), registry, new HashSet<EnvOption_EnvFeature>(), new List<ProgramOption>())).Configure(opts);
	  }

	  public static Env NewCustomEnv(params EnvOption[] opts)
	  {
		return NewCustomEnv(ProtoTypeRegistry.NewRegistry(), new List<EnvOption> (opts));
	  }

	  internal void AddProgOpts(IList<ProgramOption> progOpts)
	  {
		((List<ProgramOption>)this.progOpts).AddRange(progOpts);
	  }

	  public sealed class AstIssuesTuple
	  {
		internal readonly Ast ast;
		internal readonly Issues issues;

		internal AstIssuesTuple(Ast ast, Issues issues)
		{
		  this.ast = ast;
		  this.issues = issues;
		}

		public bool HasIssues()
		{
		  return issues.HasIssues();
		}

		public Ast Ast
		{
			get
			{
			  return ast;
			}
		}

		public Issues Issues
		{
			get
			{
			  return issues;
			}
		}
	  }

	  /// <summary>
	  /// Check performs type-checking on the input Ast and yields a checked Ast and/or set of Issues.
	  /// 
	  /// <para>Checking has failed if the returned Issues value and its Issues.Err() value are non-nil.
	  /// Issues should be inspected if they are non-nil, but may not represent a fatal error.
	  /// 
	  /// </para>
	  /// <para>It is possible to have both non-nil Ast and Issues values returned from this call: however,
	  /// the mere presence of an Ast does not imply that it is valid for use.
	  /// </para>
	  /// </summary>
	  public AstIssuesTuple Check(Ast ast)
	  {
		// Note, errors aren't currently possible on the Ast to ParsedExpr conversion.
		ParsedExpr pe = global::Cel.Cel.AstToParsedExpr(ast);

		// Construct the internal checker env, erroring if there is an issue adding the declarations.
		lock (once)
		{
		  if (chk == null && chkErr == null)
		  {
			CheckerEnv ce = CheckerEnv.NewCheckerEnv(container, provider);
			ce.EnableDynamicAggregateLiterals(true);
			if (HasFeature(EnvOption_EnvFeature.FeatureDisableDynamicAggregateLiterals))
			{
			  ce.EnableDynamicAggregateLiterals(false);
			}
			try
			{
			  ce.Add(declarations);
			  chk = ce;
			}
			catch (Exception e)
			{
			  chkErr = e;
			}
		  }
		}

		// The once call will ensure that this value is set or nil for all invocations.
		if (chkErr != null)
		{
		  Errors errs = new Errors(ast.Source);
		  errs.ReportError(Location.NoLocation, "%s", chkErr.ToString());
		  return new AstIssuesTuple(null, Issues.NewIssues(errs));
		}

		ParseResult pr = new ParseResult(pe.Expr, new Errors(ast.Source), pe.SourceInfo);
		CheckResult checkRes = global::Cel.Checker.Checker.Check(pr, ast.Source, chk);
		if (checkRes.HasErrors())
		{
		  return new AstIssuesTuple(null, Issues.NewIssues(checkRes.Errors));
		}
		// Manually create the Ast to ensure that the Ast source information (which may be more
		// detailed than the information provided by Check), is returned to the caller.
		CheckedExpr expr = checkRes.CheckedExpr;
		ast = new Ast(expr.Expr, expr.SourceInfo, ast.Source, expr.ReferenceMap, expr.TypeMap);
		return new AstIssuesTuple(ast, Issues.NoIssues(ast.Source));
	  }

	  /// <summary>
	  /// Compile combines the Parse and Check phases CEL program compilation to produce an Ast and
	  /// associated issues.
	  /// 
	  /// <para>If an error is encountered during parsing the Compile step will not continue with the Check
	  /// phase. If non-error issues are encountered during Parse, they may be combined with any issues
	  /// discovered during Check.
	  /// 
	  /// </para>
	  /// <para>Note, for parse-only uses of CEL use Parse.
	  /// </para>
	  /// </summary>
	  public AstIssuesTuple Compile(string txt)
	  {
		return CompileSource(Source.NewTextSource(txt));
	  }

	  /// <summary>
	  /// CompileSource combines the Parse and Check phases CEL program compilation to produce an Ast and
	  /// associated issues.
	  /// 
	  /// <para>If an error is encountered during parsing the CompileSource step will not continue with the
	  /// Check phase. If non-error issues are encountered during Parse, they may be combined with any
	  /// issues discovered during Check.
	  /// 
	  /// </para>
	  /// <para>Note, for parse-only uses of CEL use Parse.
	  /// </para>
	  /// </summary>
	  public AstIssuesTuple CompileSource(Source src)
	  {
		AstIssuesTuple aiParse = ParseSource(src);
		AstIssuesTuple aiCheck = Check(aiParse.ast);
		Issues iss = aiParse.issues.Append(aiCheck.issues);
		return new AstIssuesTuple(aiCheck.ast, iss);
	  }

	  /// <summary>
	  /// Extend the current environment with additional options to produce a new Env.
	  /// 
	  /// <para>Note, the extended Env value should not share memory with the original. It is possible,
	  /// however, that a CustomTypeAdapter or CustomTypeProvider options could provide values which are
	  /// mutable. To ensure separation of state between extended environments either make sure the
	  /// TypeAdapter and TypeProvider are immutable, or that their underlying implementations are based
	  /// on the ref.TypeRegistry which provides a Copy method which will be invoked by this method.
	  /// </para>
	  /// </summary>
	  public Env Extend(IList<EnvOption> opts)
	  {
		if (chkErr != null)
		{
		  throw chkErr;
		}
		// Copy slices.
		IList<Decl> decsCopy = new List<Decl>(declarations);
		IList<Macro> macsCopy = new List<Macro>(macros);
		IList<ProgramOption> progOptsCopy = new List<ProgramOption>(progOpts);

		// Copy the adapter / provider if they appear to be mutable.
		TypeAdapter adapter = this.adapter;
		TypeProvider provider = this.provider;
		// In most cases the provider and adapter will be a ref.TypeRegistry;
		// however, in the rare cases where they are not, they are assumed to
		// be immutable. Since it is possible to set the TypeProvider separately
		// from the TypeAdapter, the possible configurations which could use a
		// TypeRegistry as the base implementation are captured below.
     	// TODO check
		/* 
		if (this.adapter is TypeRegistry && this.provider is TypeRegistry)
		{
		  TypeRegistry adapterReg = (TypeRegistry) this.adapter;
		  TypeRegistry providerReg = (TypeRegistry) this.provider;
		  TypeRegistry reg = providerReg.Copy();
		  provider = reg;
		  // If the adapter and provider are the same object, set the adapter
		  // to the same ref.TypeRegistry as the provider.
		  if (adapterReg.Equals(providerReg))
		  {
			adapter = reg;
		  }
		  else
		  {
			// Otherwise, make a copy of the adapter.
			adapter = adapterReg.Copy();
		  }
		}
		*/
		if (this.provider is TypeRegistry)
		{
		  provider = ((TypeRegistry) this.provider).Copy();
		}

		ISet<EnvOption_EnvFeature> featuresCopy = new HashSet<EnvOption_EnvFeature>(this.features);

		Env ext = new Env(this.container, decsCopy, macsCopy, adapter, provider, featuresCopy, progOptsCopy);
		return ext.Configure(opts);
	  }

	  public Env Extend(params EnvOption[] opts)
	  {
		return Extend(new List<EnvOption>(opts));
	  }

	  /// <summary>
	  /// HasFeature checks whether the environment enables the given feature flag, as enumerated in
	  /// options.go.
	  /// </summary>
	  public bool HasFeature(EnvOption_EnvFeature flag)
	  {
		return features.Contains(flag);
	  }

	  /// <summary>
	  /// Parse parses the input expression value `txt` to a Ast and/or a set of Issues.
	  /// 
	  /// <para>This form of Parse creates a common.Source value for the input `txt` and forwards to the
	  /// ParseSource method.
	  /// </para>
	  /// </summary>
	  public AstIssuesTuple Parse(string txt)
	  {
		Source src = Source.NewTextSource(txt);
		return ParseSource(src);
	  }

	  /// <summary>
	  /// ParseSource parses the input source to an Ast and/or set of Issues.
	  /// 
	  /// <para>Parsing has failed if the returned Issues value and its Issues.Err() value is non-nil.
	  /// Issues should be inspected if they are non-nil, but may not represent a fatal error.
	  /// 
	  /// </para>
	  /// <para>It is possible to have both non-nil Ast and Issues values returned from this call; however,
	  /// the mere presence of an Ast does not imply that it is valid for use.
	  /// </para>
	  /// </summary>
	  public AstIssuesTuple ParseSource(Source src)
	  {
		ParseResult res = global::Cel.Parser.Parser.ParseWithMacros(src, macros);
		if (res.HasErrors())
		{
		  return new AstIssuesTuple(null, Issues.NewIssues(res.Errors));
		}
		// Manually create the Ast to ensure that the text source information is propagated on
		// subsequent calls to Check.
		return new AstIssuesTuple(new Ast(res.Expr, res.SourceInfo, src), Issues.NoIssues(src));
	  }

	  /// <summary>
	  /// Program generates an evaluable instance of the Ast within the environment (Env). </summary>
	  public Program Program(Ast ast, params ProgramOption[] opts)
	  {
		IList<ProgramOption> optSet = progOpts;
		if (opts.Length > 0)
		{
		  List<ProgramOption> mergedOpts = new List<ProgramOption>(progOpts);
		  mergedOpts.AddRange(opts);
		  optSet = mergedOpts;
		}
		return Cel.NewProgram(this, ast, ((List<ProgramOption>)optSet).ToArray());
	  }

	  /// <summary>
	  /// SetFeature sets the given feature flag, as enumerated in options.go. </summary>
	  public EnvOption_EnvFeature Feature
	  {
		  set
		  {
			features.Add(value);
		  }
	  }

	  internal Container Container
	  {
		  get
		  {
			return container;
		  }
	  }

	  /// <summary>
	  /// TypeAdapter returns the `ref.TypeAdapter` configured for the environment. </summary>
	  public TypeAdapter TypeAdapter
	  {
		  get
		  {
			return adapter;
		  }
	  }

	  /// <summary>
	  /// TypeProvider returns the `ref.TypeProvider` configured for the environment. </summary>
	  public TypeProvider TypeProvider
	  {
		  get
		  {
			return provider;
		  }
	  }

	  /// <summary>
	  /// UnknownVars returns an interpreter.PartialActivation which marks all variables declared in the
	  /// Env as unknown AttributePattern values.
	  /// 
	  /// <para>Note, the UnknownVars will behave the same as an interpreter.EmptyActivation unless the
	  /// PartialAttributes option is provided as a ProgramOption.
	  /// </para>
	  /// </summary>
	  public Activation_PartialActivation UnknownVars
	  {
		  get
		  {
			IList<AttributePattern> unknownPatterns = new List<AttributePattern>();
			foreach (Decl d in declarations)
			{
			  if (d.DeclKindCase == DeclKindCase.Ident)
			  {
				unknownPatterns.Add(AttributePattern.NewAttributePattern((string)d.Name));
			  }
			}
			return Cel.PartialVars(Activation.EmptyActivation(), ((List<AttributePattern>)unknownPatterns).ToArray());
		  }
	  }

	  /// <summary>
	  /// ResidualAst takes an Ast and its EvalDetails to produce a new Ast which only contains the
	  /// attribute references which are unknown.
	  /// 
	  /// <para>Residual expressions are beneficial in a few scenarios:
	  /// 
	  /// <ul>
	  ///   <li>Optimizing constant expression evaluations away.
	  ///   <li>Indexing and pruning expressions based on known input arguments.
	  ///   <li>Surfacing additional requirements that are needed in order to complete an evaluation.
	  ///   <li>Sharing the evaluation of an expression across multiple machines/nodes.
	  /// </ul>
	  /// 
	  /// </para>
	  /// <para>For example, if an expression targets a 'resource' and 'request' attribute and the possible
	  /// values for the resource are known, a PartialActivation could mark the 'request' as an unknown
	  /// interpreter.AttributePattern and the resulting ResidualAst would be reduced to only the parts
	  /// of the expression that reference the 'request'.
	  /// 
	  /// </para>
	  /// <para>Note, the expression ids within the residual AST generated through this method have no
	  /// correlation to the expression ids of the original AST.
	  /// 
	  /// </para>
	  /// <para>See the PartialVars helper for how to construct a PartialActivation.
	  /// 
	  /// </para>
	  /// <para>TODO: Consider adding an option to generate a Program.Residual to avoid round-tripping to an
	  /// Ast format and then Program again.
	  /// </para>
	  /// </summary>
	  public Ast ResidualAst(Ast a, EvalDetails details)
	  {
		Expr pruned = AstPruner.PruneAst(a.Expr, details.State);
		ParsedExpr parsedExpr = new ParsedExpr();
		parsedExpr.Expr = pruned;
		string expr = Cel.AstToString(Cel.ParsedExprToAst(parsedExpr));
		AstIssuesTuple parsedIss = Parse(expr);
		if (parsedIss.HasIssues())
		{
		  throw parsedIss.Issues.Err();
		}
		if (!a.Checked)
		{
		  return parsedIss.ast;
		}
		AstIssuesTuple checkedIss = Check(parsedIss.ast);
		if (checkedIss.HasIssues())
		{
		  throw checkedIss.Issues.Err();
		}
		return checkedIss.ast;
	  }

	  /// <summary>
	  /// configure applies a series of EnvOptions to the current environment. </summary>
	  internal Env Configure(IList<EnvOption> opts)
	  {
		// Customized the environment using the provided EnvOption values. If an error is
		// generated at any step this, will be returned as a nil Env with a non-nil error.
		Env e = this;
		foreach (EnvOption opt in opts)
		{
		  e = opt(e);
		}
		return e;
	  }

	  public override string ToString()
	  {
		return "Env{" + "container=" + container + "\n    , declarations=" + declarations + "\n    , macros=" + macros + "\n    , adapter=" + adapter + "\n    , provider=" + provider + "\n    , features=" + features + "\n    , progOpts=" + progOpts + "\n    , chk=" + chk + "\n    , chkErr=" + chkErr + "\n    , once=" + once + '}';
	  }
	}

}