using System;
using System.Collections.Generic;
using Cel;
using NUnit.Framework;

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
namespace org.projectnessie.cel
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThatThrownBy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.astToCheckedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.astToParsedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.astToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.attributePattern;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.checkedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.estimateCost;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.noVars;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.parsedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.partialVars;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.newCustomEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.newEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.abbrevs;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.container;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.customTypeAdapter;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.customTypeProvider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.declarations;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.homogeneousAggregateLiterals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.macros;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.types;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EvalOption.OptExhaustiveEval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EvalOption.OptPartialEval;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EvalOption.OptTrackState;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Library.StdLib;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.ProgramOption.customDecorator;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.ProgramOption.evalOptions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.ProgramOption.functions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.ProgramOption.globals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Util.mapOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.isError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.valOrErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.IntOne;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.pb.ProtoTypeRegistry.newEmptyRegistry;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.interpreter.Activation.emptyActivation;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.interpreter.Interpretable.newConstValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.parser.Macro.newReceiverMacro;

	using CheckedExpr = Google.Api.Expr.V1Alpha1.CheckedExpr;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
	using Ident = Google.Api.Expr.V1Alpha1.Expr.Types.Ident;
	using ParsedExpr = Google.Api.Expr.V1Alpha1.ParsedExpr;
	using Type = Google.Api.Expr.V1Alpha1.Type;
	using AstIssuesTuple = Cel.Env.AstIssuesTuple;
	using Decls = Cel.Checker.Decls;
	using Operator = Cel.Common.Operators.Operator;
	using BoolT = Cel.Common.Types.BoolT;
	using IntT = Cel.Common.Types.IntT;
	using Overloads = Cel.Common.Types.Overloads;
	using StringT = Cel.Common.Types.StringT;
	using UnknownT = Cel.Common.Types.UnknownT;
	using DefaultTypeAdapter = Cel.Common.Types.Pb.DefaultTypeAdapter;
	using TypeRegistry = Cel.Common.Types.Ref.TypeRegistry;
	using Val = Cel.Common.Types.Ref.Val;
	using Container = Cel.Common.Types.Traits.Container;
	using Mapper = Cel.Common.Types.Traits.Mapper;
	using Trait = Cel.Common.Types.Traits.Trait;
	using Activation_PartialActivation = Cel.Interpreter.Activation_PartialActivation;
	using AttributeFactory_NamespacedAttribute = Cel.Interpreter.AttributeFactory_NamespacedAttribute;
	using Coster_Cost = Cel.Interpreter.Coster_Cost;
	using EvalState = Cel.Interpreter.EvalState;
	using Interpretable = Cel.Interpreter.Interpretable;
	using Interpretable_InterpretableAttribute = Cel.Interpreter.Interpretable_InterpretableAttribute;
	using Interpretable_InterpretableCall = Cel.Interpreter.Interpretable_InterpretableCall;
	using Interpretable_InterpretableConst = Cel.Interpreter.Interpretable_InterpretableConst;
	using InterpretableDecorator = Cel.Interpreter.InterpretableDecorator;
	using Overload = Cel.Interpreter.Functions.Overload;
	using Macro = Cel.Parser.Macro;

	[TestFixture]
	public class CELTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void AstToProto()
		[Test]
		public virtual void AstToProto()
		{
			Env stdEnv =
				Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("a", Decls.Dyn), Decls.NewVar("b", Decls.Dyn)));
			AstIssuesTuple astIss = stdEnv.Parse("a + b");
			Assert.That(astIss.HasIssues(), Is.False);
			ParsedExpr parsed = Cel.Cel.AstToParsedExpr(astIss.Ast);
			Ast ast2 = Cel.Cel.ParsedExprToAst(parsed);
			Assert.That(ast2.Expr, Is.EqualTo(astIss.Ast.Expr));

			Assert.That(() => Cel.Cel.AstToCheckedExpr(astIss.Ast),
				Throws.Exception.TypeOf(typeof(System.ArgumentException)));
			AstIssuesTuple astIss2 = stdEnv.Check(astIss.Ast);
			Assert.That(astIss2.HasIssues(), Is.False);
			// Assert.That(astIss.hasIssues()).isFalse();
			CheckedExpr @checked = Cel.Cel.AstToCheckedExpr(astIss2.Ast);
			Ast ast3 = Cel.Cel.CheckedExprToAst(@checked);
			Assert.That(ast3.Expr, Is.EqualTo(astIss2.Ast.Expr));
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void AstToString()
		[Test]
		public virtual void AstToString()
		{
			Env stdEnv = Env.NewEnv();
			string @in = "a + b - (c ? (-d + 4) : e)";
			AstIssuesTuple astIss = stdEnv.Parse(@in);
			Assert.That(astIss.HasIssues(), Is.False);
			string expr = Cel.Cel.AstToString(astIss.Ast);
			Assert.That(expr, Is.EqualTo(@in));
		}

		/*
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void CheckedExprToAst_ConstantExpr()
		internal virtual void CheckedExprToAstConstantExpr()
		{
		  Env stdEnv = newEnv();
		  string @in = "10";
		  AstIssuesTuple astIss = stdEnv.Compile(@in);
		  assertThat(astIss.HasIssues()).isFalse();
		  CheckedExpr expr = astToCheckedExpr(astIss.Ast);
		  Ast ast2 = checkedExprToAst(expr);
		  assertThat(ast2.Expr).isEqualTo(astIss.Ast.Expr);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void exampleWithBuiltins()
		internal virtual void ExampleWithBuiltins()
		{
		  // Variables used within this expression environment.
		  EnvOption decls = declarations(Decls.NewVar("i", Decls.String), Decls.NewVar("you", Decls.String));
		  Env env = newEnv(decls);
  
		  // Compile the expression.
		  AstIssuesTuple astIss = env.Compile("\"Hello \" + you + \"! I'm \" + i + \".\"");
		  assertThat(astIss.HasIssues()).isFalse();
  
		  // Create the program, and evaluate it against some input.
		  Program prg = env.Program(astIss.Ast);
  
		  // If the Eval() call were provided with cel.evalOptions(OptTrackState) the details response
		  // (2nd return) would be non-nil.
		  Program_EvalResult @out = prg.Eval(mapOf("i", "CEL", "you", "world"));
  
		  assertThat(@out.Val.Equal(stringOf("Hello world! I'm CEL."))).isSameAs(True);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void Abbrevs_Compiled()
		internal virtual void AbbrevsCompiled()
		{
		  // Test whether abbreviations successfully resolve at type-check time (compile time).
		  Env env = newEnv(abbrevs("qualified.identifier.name"), declarations(Decls.NewVar("qualified.identifier.name.first", Decls.String)));
		  AstIssuesTuple astIss = env.Compile("\"hello \"+ name.first"); // abbreviation resolved here.
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = env.Program(astIss.Ast);
		  Program_EvalResult @out = prg.Eval(mapOf("qualified.identifier.name.first", "Jim"));
		  assertThat(@out.Val.Value()).isEqualTo("hello Jim");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void Abbrevs_Parsed()
		internal virtual void AbbrevsParsed()
		{
		  // Test whether abbreviations are resolved properly at evaluation time.
		  Env env = newEnv(abbrevs("qualified.identifier.name"));
		  AstIssuesTuple astIss = env.Parse("\"hello \" + name.first");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = env.Program(astIss.Ast); // abbreviation resolved here.
		  Program_EvalResult @out = prg.Eval(mapOf("qualified.identifier.name", mapOf("first", "Jim")));
		  assertThat(@out.Val.Value()).isEqualTo("hello Jim");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void Abbrevs_Disambiguation()
		internal virtual void AbbrevsDisambiguation()
		{
		  Env env = newEnv(abbrevs("external.Expr"), container("google.api.expr.v1alpha1"), types(Expr.getDefaultInstance()), declarations(Decls.NewVar("test", Decls.Bool), Decls.NewVar("external.Expr", Decls.String)));
		  // This expression will return either a string or a protobuf Expr value depending on the value
		  // of the 'test' argument. The fully qualified type name is used indicate that the protobuf
		  // typed 'Expr' should be used rather than the abbreviatation for 'external.Expr'.
		  AstIssuesTuple astIss = env.Compile("test ? dyn(Expr) : google.api.expr.v1alpha1.Expr{id: 1}");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = env.Program(astIss.Ast);
		  Program_EvalResult @out = prg.Eval(mapOf("test", true, "external.Expr", "string expr"));
		  assertThat(@out.Val.Value()).isEqualTo("string expr");
		  @out = prg.Eval(mapOf("test", false, "external.Expr", "wrong expr"));
		  Expr want = Expr.newBuilder().setId(1).build();
		  Expr got = @out.Val.ConvertToNative(typeof(Expr));
		  assertThat(got).isEqualTo(want);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void CustomEnvError()
		internal virtual void CustomEnvError()
		{
		  Env e = newCustomEnv(StdLib(), StdLib());
		  AstIssuesTuple xIss = e.Compile("a.b.c == true");
		  assertThat(xIss.HasIssues()).isTrue();
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void CustomEnv()
		internal virtual void CustomEnv()
		{
		  Env e = newCustomEnv(declarations(Decls.NewVar("a.b.c", Decls.Bool)));
  
		  // t.Run("err", func(t *testing.T) {
		  AstIssuesTuple xIss = e.Compile("a.b.c == true");
		  assertThat(xIss.HasIssues()).isTrue();
  
		  // t.Run("ok", func(t *testing.T) {
		  AstIssuesTuple astIss = e.Compile("a.b.c");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = e.Program(astIss.Ast);
		  Program_EvalResult @out = prg.Eval(mapOf("a.b.c", true));
		  assertThat(@out.Val).isSameAs(True);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void HomogeneousAggregateLiterals()
		internal virtual void HomogeneousAggregateLiterals()
		{
		  Env e = newCustomEnv(declarations(Decls.NewVar("name", Decls.String), Decls.NewFunction(Operator.In.id, Decls.NewOverload(Overloads.InList, new List<Type> {Decls.String, Decls.NewListType(Decls.String)}, Decls.Bool), Decls.NewOverload(Overloads.InMap, new List<Type> {Decls.String, Decls.NewMapType(Decls.String, Decls.Bool)}, Decls.Bool))), homogeneousAggregateLiterals());
  
		  // t.Run("err_list", func(t *testing.T) {
		  AstIssuesTuple xIss = e.Compile("name in ['hello', 0]");
		  assertThat(xIss.Issues).isNotNull();
		  assertThat(xIss.HasIssues()).isTrue();
		  // })
		  // t.Run("err_map_key", func(t *testing.T) {
		  xIss = e.Compile("name in {'hello':'world', 1:'!'}");
		  assertThat(xIss.Issues).isNotNull();
		  assertThat(xIss.HasIssues()).isTrue();
		  // })
		  // t.Run("err_map_val", func(t *testing.T) {
		  xIss = e.Compile("name in {'hello':'world', 'goodbye':true}");
		  assertThat(xIss.Issues).isNotNull();
		  assertThat(xIss.HasIssues()).isTrue();
		  // })
  
		  ProgramOption funcs = functions(Overload.binary(Operator.In.id, (lhs, rhs) =>
		  {
		  if (rhs.type().hasTrait(Trait.ContainerType))
		  {
			  return ((Container) rhs).Contains(lhs);
		  }
		  return valOrErr(rhs, "no such overload");
		  }));
		  // t.Run("ok_list", func(t *testing.T) {
		  AstIssuesTuple astIss = e.Compile("name in ['hello', 'world']");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = e.Program(astIss.Ast, funcs);
		  Program_EvalResult @out = prg.Eval(mapOf("name", "world"));
		  assertThat(@out.Val).isSameAs(True);
		  // })
		  // t.Run("ok_map", func(t *testing.T) {
		  astIss = e.Compile("name in {'hello': false, 'world': true}");
		  assertThat(astIss.HasIssues()).isFalse();
		  prg = e.Program(astIss.Ast, funcs);
		  @out = prg.Eval(mapOf("name", "world"));
		  assertThat(@out.Val).isSameAs(True);
		  // })
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void Customtypes()
		internal virtual void Customtypes()
		{
		  Type exprType = Decls.NewObjectType("google.api.expr.v1alpha1.Expr");
		  TypeRegistry reg = newEmptyRegistry();
		  Env e = newEnv(customTypeAdapter(reg), customTypeProvider(reg), container("google.api.expr.v1alpha1"), types(Expr.getDefaultInstance(), BoolT.BoolType, IntT.IntType, StringT.StringType), declarations(Decls.NewVar("expr", exprType)));
  
		  AstIssuesTuple astIss = e.Compile("expr == Expr{id: 2,\n" + "\t\t\tcall_expr: Expr.Call{\n" + "\t\t\t\tfunction: \"_==_\",\n" + "\t\t\t\targs: [\n" + "\t\t\t\t\tExpr{id: 1, ident_expr: Expr.Ident{ name: \"a\" }},\n" + "\t\t\t\t\tExpr{id: 3, ident_expr: Expr.Ident{ name: \"b\" }}]\n" + "\t\t\t}}");
		  assertThat(astIss.Ast.ResultType).isEqualTo(Decls.Bool);
		  Program prg = e.Program(astIss.Ast);
		  object vars = mapOf("expr", Expr.newBuilder().setId(2).setCallExpr(Expr.Call.newBuilder().setFunction("_==_").addAllArgs(asList(Expr.newBuilder().setId(1).setIdentExpr(Expr.Ident.newBuilder().setName("a")).build(), Expr.newBuilder().setId(3).setIdentExpr(Expr.Ident.newBuilder().setName("b")).build()))).build());
		  Program_EvalResult @out = prg.Eval(vars);
		  assertThat(@out.Val).isSameAs(True);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void TypeIsolation()
		internal virtual void TypeIsolation()
		{
		  //	b = ioutil.ReadFile("testdata/team.fds")
		  //	var fds descpb.FileDescriptorSet
		  //	if err = proto.Unmarshal(b, &fds); err != nil {
		  //		t.Fatal("can't unmarshal descriptor data: ", err)
		  //	}
		  //
		  //	Env e = newEnv(
		  //		typeDescs(&fds),
		  //		declarations(
		  //			Decls.newVar("myteam",
		  //				Decls.newObjectType("cel.testdata.Team"))));
		  //
		  //	String src = "myteam.members[0].name == 'Cyclops'";
		  //	AstIssuesTuple xIss = e.compile(src)
		  //	assertThat(xIss.getIssues().err()).isNull();
		  //
		  //	// Ensure that isolated types don't leak through.
		  //	Env e2 = newEnv(
		  //		declarations(
		  //			Decls.newVar("myteam",
		  //				Decls.newObjectType("cel.testdata.Team"))))
		  //	xIss = e2.compile(src)
		  //	if iss == nil || iss.Err() == nil {
		  //		t.Errorf("wanted compile failure for unknown message.")
		  //	}
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void DynamicProto()
		internal virtual void DynamicProto()
		{
		  //	b = ioutil.ReadFile("testdata/team.fds");
		  //	var fds descpb.FileDescriptorSet
		  //	if err = proto.Unmarshal(b, &fds); err != nil {
		  //		t.Fatalf("proto.Unmarshal() failed: %v", err)
		  //	}
		  //	files = (&fds).GetFile()
		  //	fileCopy = make([]interface{}, len(files))
		  //	for i = 0; i < len(files); i++ {
		  //		fileCopy[i] = files[i]
		  //	}
		  //	pbFiles = protodesc.NewFiles(&fds);
		  //	Env e = newEnv(
		  //		container("cel"),
		  //		// The following is identical to registering the FileDescriptorSet;
		  //		// however, it tests a different code path which aggregates individual
		  //		// FileDescriptorProto values together.
		  //		typeDescs(fileCopy...),
		  //		// Additionally, demonstrate that double registration of files doesn't
		  //		// cause any problems.
		  //		typeDescs(pbFiles),
		  //	);
		  //	src = `testdata.Team{name: 'X-Men', members: [
		  //		testdata.Mutant{name: 'Jean Grey', level: 20},
		  //		testdata.Mutant{name: 'Cyclops', level: 7},
		  //		testdata.Mutant{name: 'Storm', level: 7},
		  //		testdata.Mutant{name: 'Wolverine', level: 11}
		  //	]}`
		  //	AstIssuesTuple astIss = e.compile(src)
		  //	assertThat(astIss.hasIssues()).isFalse();
		  //  Program  prg = e.program(astIss.getAst(), evalOptions(OptOptimize));
		  //  EvalResult out = prg.eval(noVars);
		  //	obj, ok = out.(Trait.Indexer)
		  //	if !ok {
		  //		t.Fatalf("unable to convert output to object: %v", out)
		  //	}
		  //	if obj.Get(types.String("name")).equal(types.String("X-Men")) == types.False {
		  //		t.Fatalf("got field 'name' %v, wanted X-Men", obj.Get(types.String("name")))
		  //	}
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void DynamicProto_Input()
		internal virtual void DynamicProtoInput()
		{
		  //	b = ioutil.ReadFile("testdata/team.fds");
		  //	var fds descpb.FileDescriptorSet
		  //	if err = proto.Unmarshal(b, &fds); err != nil {
		  //		t.Fatalf("proto.Unmarshal() failed: %v", err)
		  //	}
		  //	files = (&fds).GetFile()
		  //	fileCopy = make([]interface{}, len(files))
		  //	for i = 0; i < len(files); i++ {
		  //		fileCopy[i] = files[i]
		  //	}
		  //	pbFiles = protodesc.NewFiles(&fds);
		  //	desc = pbFiles.FindDescriptorByName("cel.testdata.Mutant");
		  //	msgDesc, ok = desc.(protoreflect.MessageDescriptor)
		  //	if !ok {
		  //		t.Fatalf("desc not convertible to MessageDescriptor: %T", desc)
		  //	}
		  //	wolverine = dynamicpb.NewMessage(msgDesc)
		  //	wolverine.ProtoReflect().Set(msgDesc.Fields().ByName("name"),
		  // protoreflect.ValueOfString("Wolverine"))
		  //	Env e = newEnv(
		  //		// The following is identical to registering the FileDescriptorSet;
		  //		// however, it tests a different code path which aggregates individual
		  //		// FileDescriptorProto values together.
		  //		typeDescs(fileCopy...),
		  //		declarations(Decls.newVar("mutant", Decls.newObjectType("cel.testdata.Mutant"))),
		  //	);
		  //	src = `has(mutant.name) && mutant.name == 'Wolverine'`
		  //	AstIssuesTuple astIss = e.compile(src);
		  //	assertThat(astIss.hasIssues()).isFalse();
		  //  Program  prg = e.program(astIss.getAst(), evalOptions(OptOptimize));
		  //  EvalResult out = prg.eval(map[string]interface{}{
		  //		"mutant": wolverine,
		  //	});
		  //	obj, ok = out.(types.Bool)
		  //	if !ok {
		  //		t.Fatalf("unable to convert output to object: %v", out)
		  //	}
		  //	if obj != types.True {
		  //		t.Errorf("got %v, wanted true", out)
		  //	}
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void GlobalVars()
		internal virtual void GlobalVars()
		{
		  Type mapStrDyn = Decls.NewMapType(Decls.String, Decls.Dyn);
		  Env e = newEnv(declarations(Decls.NewVar("attrs", mapStrDyn), Decls.NewVar("default", Decls.Dyn), Decls.NewFunction("get", Decls.NewInstanceOverload("get_map", new List<Type> {mapStrDyn, Decls.String, Decls.Dyn}, Decls.Dyn))));
		  AstIssuesTuple astIss = e.Compile("attrs.get(\"first\", attrs.get(\"second\", default))");
  
		  // Create the program.
		  ProgramOption funcs = functions(Overload.Function("get", args =>
		  {
		  if (args.length != 3)
		  {
			  return newErr("invalid arguments to 'get'");
		  }
		  if (!(args[0] is Mapper))
		  {
			  return newErr("invalid operand of type '%s' to obj.get(key, def)", args[0].type());
		  }
		  Mapper attrs = (Mapper) args[0];
		  if (!(args[1] is StringT))
		  {
			  return newErr("invalid key of type '%s' to obj.get(key, def)", args[1].type());
		  }
		  StringT key = (StringT) args[1];
		  Val defVal = args[2];
		  if (attrs.Contains(key) == True)
		  {
			  return attrs.Get(key);
		  }
		  return defVal;
		  }));
  
		  // Global variables can be configured as a ProgramOption and optionally overridden on Eval.
		  Program prg = e.Program(astIss.Ast, funcs, globals(mapOf("default", "third")));
  
		  // t.Run("global_default", func(t *testing.T) {
		  object vars = mapOf("attrs", mapOf());
		  Program_EvalResult @out = prg.Eval(vars);
		  assertThat(@out.Val.Equal(stringOf("third"))).isSameAs(True);
		  // })
  
		  // t.Run("attrs_alt", func(t *testing.T) {
		  vars = mapOf("attrs", mapOf("second", "yep"));
		  @out = prg.Eval(vars);
		  assertThat(@out.Val.Equal(stringOf("yep"))).isSameAs(True);
		  // })
  
		  // t.Run("local_default", func(t *testing.T) {
		  vars = mapOf("attrs", mapOf(), "default", "fourth");
		  @out = prg.Eval(vars);
		  assertThat(@out.Val.Equal(stringOf("fourth"))).isSameAs(True);
		  // })
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void CustomMacro()
		internal virtual void CustomMacro()
		{
		  Macro joinMacro = newReceiverMacro("join", 1, (eh, target, args) =>
		  {
		  Expr delim = args.get(0);
		  Expr iterIdent = eh.ident("__iter__");
		  Expr accuIdent = eh.ident("__result__");
		  Expr init = eh.literalString("");
		  Expr condition = eh.literalBool(true);
		  Expr step = eh.globalCall(Operator.Conditional.id, eh.globalCall(Operator.Greater.id, eh.receiverCall("size", accuIdent, emptyList()), eh.literalInt(0)), eh.globalCall(Operator.Add.id, eh.globalCall(Operator.Add.id, accuIdent, delim), iterIdent), iterIdent);
		  return eh.fold("__iter__", target, "__result__", init, condition, step, accuIdent);
		  });
		  Env e = newEnv(macros(joinMacro));
		  AstIssuesTuple astIss = e.Compile("['hello', 'cel', 'friend'].join(',')");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = e.Program(astIss.Ast, evalOptions(OptExhaustiveEval));
		  Program_EvalResult @out = prg.Eval(noVars());
		  assertThat(@out.Val.Equal(stringOf("hello,cel,friend"))).isSameAs(True);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void AstIsChecked()
		internal virtual void AstIsChecked()
		{
		  Env e = newEnv();
		  AstIssuesTuple astIss = e.Compile("true");
		  assertThat(astIss.HasIssues()).isFalse();
  //JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		  assertThat(astIss.Ast).extracting(Ast::isChecked).isEqualTo(true);
		  CheckedExpr ce = astToCheckedExpr(astIss.Ast);
		  Ast ast2 = checkedExprToAst(ce);
  //JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		  assertThat(ast2).extracting(Ast::isChecked).isEqualTo(true);
		  assertThat(astIss.Ast.Expr).isEqualTo(ast2.Expr);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void EvalOptions()
		internal virtual void EvalOptions()
		{
		  Env e = newEnv(declarations(Decls.NewVar("k", Decls.String), Decls.NewVar("v", Decls.Bool)));
		  AstIssuesTuple astIss = e.Compile("{k: true}[k] || v != false");
  
		  Program prg = e.Program(astIss.Ast, evalOptions(OptExhaustiveEval));
		  Program_EvalResult outDetails = prg.Eval(mapOf("k", "key", "v", true));
		  assertThat(outDetails.Val).isSameAs(True);
  
		  // Test to see whether 'v != false' was resolved to a value.
		  // With short-circuiting it normally wouldn't be.
		  EvalState s = outDetails.EvalDetails.State;
		  Val lhsVal = s.Value(astIss.Ast.Expr.getCallExpr().getArgs(0).getId());
		  assertThat(lhsVal).isSameAs(True);
		  Val rhsVal = s.Value(astIss.Ast.Expr.getCallExpr().getArgs(1).getId());
		  assertThat(rhsVal).isSameAs(True);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void EvalRecover()
		internal virtual void EvalRecover()
		{
		  Env e = newEnv(declarations(Decls.NewFunction("panic", Decls.NewOverload("panic", singletonList(Type.getDefaultInstance()), Decls.Bool))));
		  ProgramOption funcs = functions(Overload.Function("panic", args =>
		  {
		  throw new Exception("watch me recover");
		  }));
		  // Test standard evaluation.
		  AstIssuesTuple pAst = e.Parse("panic()");
		  Program prgm1 = e.Program(pAst.Ast, funcs);
		  assertThatThrownBy(() => prgm1.Eval(emptyMap())).isExactlyInstanceOf(typeof(Exception)).hasMessage("internal error: watch me recover");
		  // Test the factory-based evaluation.
		  Program prgm2 = e.Program(pAst.Ast, funcs, evalOptions(OptTrackState));
		  assertThatThrownBy(() => prgm2.Eval(emptyMap())).isExactlyInstanceOf(typeof(Exception)).hasMessage("internal error: watch me recover");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void ResidualAst()
		internal virtual void ResidualAst()
		{
		  Env e = newEnv(declarations(Decls.NewVar("x", Decls.Int), Decls.NewVar("y", Decls.Int)));
		  Activation_PartialActivation unkVars = e.UnknownVars;
		  AstIssuesTuple astIss = e.Parse("x < 10 && (y == 0 || 'hello' != 'goodbye')");
		  Program prg = e.Program(astIss.Ast, evalOptions(OptTrackState, OptPartialEval));
		  Program_EvalResult outDet = prg.Eval(unkVars);
		  assertThat(outDet.Val).matches(UnknownT.isUnknown);
		  Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
		  string expr = astToString(residual);
		  assertThat(expr).isEqualTo("x < 10");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void ResidualAst_Complex()
		internal virtual void ResidualAstComplex()
		{
		  Env e = newEnv(declarations(Decls.NewVar("resource.name", Decls.String), Decls.NewVar("request.time", Decls.Timestamp), Decls.NewVar("request.auth.claims", Decls.NewMapType(Decls.String, Decls.String))));
		  Activation_PartialActivation unkVars = partialVars(mapOf("resource.name", "bucket/my-bucket/objects/private", "request.auth.claims", mapOf("email_verified", "true")), attributePattern("request.auth.claims").qualString("email"));
		  AstIssuesTuple astIss = e.Compile("resource.name.startsWith(\"bucket/my-bucket\") &&\n" + "\t\t bool(request.auth.claims.email_verified) == true &&\n" + "\t\t request.auth.claims.email == \"wiley@acme.co\"");
		  assertThat(astIss.HasIssues()).isFalse();
		  Program prg = e.Program(astIss.Ast, evalOptions(OptTrackState, OptPartialEval));
		  Program_EvalResult outDet = prg.Eval(unkVars);
		  assertThat(outDet.Val).matches(UnknownT.isUnknown);
		  Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
		  string expr = astToString(residual);
		  assertThat(expr).isEqualTo("request.auth.claims.email == \"wiley@acme.co\"");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void EnvExtension()
		internal virtual void EnvExtension()
		{
		  Env e = newEnv(container("google.api.expr.v1alpha1"), types(Expr.getDefaultInstance()), declarations(Decls.NewVar("expr", Decls.NewObjectType("google.api.expr.v1alpha1.Expr"))));
		  Env e2 = e.Extend(customTypeAdapter(DefaultTypeAdapter.Instance), types(com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()));
		  assertThat(e).isNotEqualTo(e2);
		  assertThat(e.TypeAdapter).isNotEqualTo(e2.TypeAdapter);
		  assertThat(e.TypeProvider).isNotEqualTo(e2.TypeProvider);
		  Env e3 = e2.Extend();
		  assertThat(e2.TypeAdapter).isEqualTo(e3.TypeAdapter);
		  assertThat(e2.TypeProvider).isEqualTo(e3.TypeProvider);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void EnvExtensionIsolation()
		internal virtual void EnvExtensionIsolation()
		{
		  Env baseEnv = newEnv(container("google.api.expr.test.v1"), declarations(Decls.NewVar("age", Decls.Int), Decls.NewVar("gender", Decls.String), Decls.NewVar("country", Decls.String)));
		  Env env1 = baseEnv.Extend(types(com.google.api.expr.test.v1.proto2.TestAllTypesProto.TestAllTypes.getDefaultInstance()), declarations(Decls.NewVar("name", Decls.String)));
		  Env env2 = baseEnv.Extend(types(com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()), declarations(Decls.NewVar("group", Decls.String)));
		  AstIssuesTuple astIss = env2.Compile("size(group) > 10 && !has(proto3.TestAllTypes{}.single_int32)");
		  assertThat(astIss.HasIssues()).isFalse();
		  astIss = env2.Compile("size(name) > 10");
		  assertThat(astIss.Issues.Err()).withFailMessage("env2 contains 'name', but should not").isNotNull();
		  astIss = env2.Compile("!has(proto2.TestAllTypes{}.single_int32)");
		  assertThat(astIss.HasIssues()).isTrue();
  
		  astIss = env1.Compile("size(name) > 10 && !has(proto2.TestAllTypes{}.single_int32)");
		  assertThat(astIss.HasIssues()).isFalse();
		  astIss = env1.Compile("size(group) > 10");
		  assertThat(astIss.HasIssues()).isTrue();
		  astIss = env1.Compile("!has(proto3.TestAllTypes{}.single_int32)");
		  assertThat(astIss.HasIssues()).isTrue();
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @SuppressWarnings("rawtypes") @Test void ParseAndCheckConcurrently() throws Exception
  //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		internal virtual void ParseAndCheckConcurrently()
		{
		  Env e = newEnv(container("google.api.expr.v1alpha1"), types(Expr.getDefaultInstance()), declarations(Decls.NewVar("expr", Decls.NewObjectType("google.api.expr.v1alpha1.Expr"))));
  
		  System.Action<string> parseAndCheck = expr =>
		  {
			AstIssuesTuple xIss = e.Compile(expr);
			assertThat(xIss.HasIssues()).isFalse();
		  };
  
		  int concurrency = 10;
		  ExecutorService executor = Executors.newFixedThreadPool(concurrency);
		  try
		  {
  //JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			CompletableFuture[] futures = IntStream.range(0, concurrency).mapToObj(i => CompletableFuture.runAsync(() => parseAndCheck(string.Format("expr.id + {0:D}", i)), executor)).toArray(CompletableFuture[]::new);
			CompletableFuture.allOf(futures).get(30, TimeUnit.SECONDS);
		  }
		  finally
		  {
			executor.shutdown();
			assertThat(executor.awaitTermination(30, TimeUnit.SECONDS)).isTrue();
		  }
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void CustomInterpreterDecorator()
		internal virtual void CustomInterpreterDecorator()
		{
		  AtomicReference<Interpretable> lastInstruction = new AtomicReference<Interpretable>();
		  InterpretableDecorator optimizeArith = i =>
		  {
			lastInstruction.set(i);
			// Only optimize the instruction if it is a call.
			if (!(i is InterpretableCall))
			{
			  return i;
			}
			Interpretable_InterpretableCall call = (Interpretable_InterpretableCall) i;
			// Only optimize the math functions when they have constant arguments.
			switch (call.Function())
			{
			  case "_+_":
			  case "_-_":
			  case "_*_":
			  case "_/_":
				// These are all binary operators so they should have to arguments
				Interpretable[] args = call.Args();
				// When the values are constant then the call can be evaluated with
				// an empty activation and the value returns as a constant.
				if (!(args[0] is InterpretableConst) || !(args[1] is InterpretableConst))
				{
				  return i;
				}
				Val val = call.Eval(emptyActivation());
				if (isError(val))
				{
				  throw new Exception(val.ToString());
				}
				return newConstValue(call.Id(), val);
			  default:
				return i;
			}
		  };
  
		  Env env = newEnv(declarations(Decls.NewVar("foo", Decls.Int)));
		  AstIssuesTuple astIss = env.Compile("foo == -1 + 2 * 3 / 3");
		  env.Program(astIss.Ast, evalOptions(OptPartialEval), customDecorator(optimizeArith));
		  assertThat(lastInstruction.get()).isInstanceOf(typeof(Interpretable_InterpretableCall));
		  Interpretable_InterpretableCall call = (Interpretable_InterpretableCall) lastInstruction.get();
		  Interpretable[] args = call.Args();
		  Interpretable lhs = args[0];
		  assertThat(lhs).isInstanceOf(typeof(Interpretable_InterpretableAttribute));
		  Interpretable_InterpretableAttribute lastAttr = (Interpretable_InterpretableAttribute) lhs;
		  AttributeFactory_NamespacedAttribute absAttr = (AttributeFactory_NamespacedAttribute) lastAttr.Attr();
		  string[] varNames = absAttr.CandidateVariableNames();
		  assertThat(varNames).containsExactly("foo");
		  Interpretable rhs = args[1];
		  assertThat(rhs).isInstanceOf(typeof(Interpretable_InterpretableConst));
		  Interpretable_InterpretableConst lastConst = (Interpretable_InterpretableConst) rhs;
		  // This is the last number produced by the optimization.
		  assertThat(lastConst.Value()).isSameAs(IntOne);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void Cost()
		internal virtual void Cost()
		{
		  Env e = newEnv();
		  AstIssuesTuple astIss = e.Compile("\"Hello, World!\"");
		  assertThat(astIss.HasIssues()).isFalse();
  
		  Coster_Cost wantedCost = Coster_Cost.None;
  
		  // Test standard evaluation cost.
		  Program prg = e.Program(astIss.Ast);
		  Coster_Cost c = estimateCost(prg);
		  assertThat(c).isEqualTo(wantedCost);
  
		  // Test the factory-based evaluation cost.
		  prg = e.Program(astIss.Ast, evalOptions(OptExhaustiveEval));
		  c = estimateCost(prg);
		  assertThat(c).isEqualTo(wantedCost);
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void ResidualAst_AttributeQualifiers()
		internal virtual void ResidualAstAttributeQualifiers()
		{
		  Env e = newEnv(declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Dyn)), Decls.NewVar("y", Decls.NewListType(Decls.Int)), Decls.NewVar("u", Decls.Int)));
		  AstIssuesTuple astIss = e.Parse("x.abc == u && x[\"abc\"] == u && x[x.string] == u && y[0] == u && y[x.zero] == u && (true ? x : y).abc == u && (false ? y : x).abc == u");
		  Program prg = e.Program(astIss.Ast, evalOptions(OptTrackState, OptPartialEval));
		  Activation_PartialActivation vars = partialVars(mapOf("x", mapOf("zero", 0, "abc", 123, "string", "abc"), "y", singletonList(123)), attributePattern("u"));
		  Program_EvalResult outDet = prg.Eval(vars);
		  assertThat(outDet.Val).matches(UnknownT.isUnknown);
		  Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
		  string expr = astToString(residual);
		  assertThat(expr).isEqualTo("123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u");
		}
  
  //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
  //ORIGINAL LINE: @Test void residualAst_Modified()
		internal virtual void ResidualAstModified()
		{
		  Env e = newEnv(declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Int)), Decls.NewVar("y", Decls.Int)));
		  AstIssuesTuple astIss = e.Parse("x == y");
		  Program prg = e.Program(astIss.Ast, evalOptions(OptTrackState, OptPartialEval));
		  for (int x = 123; x < 456; x++)
		  {
			Activation_PartialActivation vars = partialVars(mapOf("x", x), attributePattern("y"));
			Program_EvalResult outDet = prg.Eval(vars);
			assertThat(outDet.Val).matches(UnknownT.isUnknown);
			Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
			string orig = astToString(astIss.Ast);
			assertThat(orig).isEqualTo("x == y");
			string expr = astToString(residual);
			string want = string.Format("{0:D} == y", x);
			assertThat(expr).isEqualTo(want);
		  }
		}
  
		//  @SuppressWarnings("rawtypes")
		//  static void Example() {
		//    // Create the CEL environment with declarations for the input attributes and
		//    // the desired extension functions. In many cases the desired functionality will
		//    // be present in a built-in function.
		//    EnvOption decls =
		//        declarations(
		//            // Identifiers used within this expression.
		//            Decls.newVar("i", Decls.String),
		//            Decls.newVar("you", Decls.String),
		//            // Function to generate a greeting from one person to another.
		//            //    i.greet(you)
		//            Decls.newFunction(
		//                "greet",
		//                Decls.newInstanceOverload(
		//                    "string_greet_string", asList(Decls.String, Decls.String), Decls.String)));
		//    Env e = newEnv(decls);
		//
		//    // Compile the expression.
		//    AstIssuesTuple astIss = e.compile("i.greet(you)");
		//    assertThat(astIss.hasIssues()).isFalse();
		//
		//    // Create the program.
		//    ProgramOption funcs =
		//        functions(
		//            Overload.binary(
		//                "string_greet_string",
		//                (lhs, rhs) ->
		//                    stringOf(String.format("Hello %s! Nice to meet you, I'm %s.\n", rhs,
		// lhs))));
		//    Program prg = e.program(astIss.getAst(), funcs);
		//
		//    // Evaluate the program against some inputs. Note: the details return is not used.
		//    EvalResult out =
		//        prg.eval(
		//            mapOf(
		//                // Native values are converted to CEL values under the covers.
		//                "i",
		//                "CEL",
		//                // Values may also be lazily supplied.
		//                "you",
		//                (Supplier) () -> stringOf("world")));
		//
		//    System.out.println(out);
		//    // Output:Hello world! Nice to meet you, I'm CEL.
		//  }
  
		//  // ExampleGlobalOverload demonstrates how to define global overload function.
		//  @SuppressWarnings("rawtypes")
		//  static void Example_globalOverload() {
		//    // Create the CEL environment with declarations for the input attributes and
		//    // the desired extension functions. In many cases the desired functionality will
		//    // be present in a built-in function.
		//    EnvOption decls =
		//        declarations(
		//            // Identifiers used within this expression.
		//            Decls.newVar("i", Decls.String),
		//            Decls.newVar("you", Decls.String),
		//            // Function to generate shake_hands between two people.
		//            //    shake_hands(i,you)
		//            Decls.newFunction(
		//                "shake_hands",
		//                Decls.newOverload(
		//                    "shake_hands_string_string",
		//                    asList(Decls.String, Decls.String),
		//                    Decls.String)));
		//    Env e = newEnv(decls);
		//
		//    // Compile the expression.
		//    AstIssuesTuple astIss = e.compile("shake_hands(i,you)");
		//    assertThat(astIss.hasIssues()).isFalse();
		//
		//    // Create the program.
		//    ProgramOption funcs =
		//        functions(
		//            Overload.binary(
		//                "shake_hands_string_string",
		//                (lhs, rhs) -> {
		//                  if (!(lhs instanceof StringT)) {
		//                    return valOrErr(lhs, "unexpected type '%s' passed to shake_hands",
		// lhs.type());
		//                  }
		//                  if (!(rhs instanceof StringT)) {
		//                    return valOrErr(rhs, "unexpected type '%s' passed to shake_hands",
		// rhs.type());
		//                  }
		//                  StringT s1 = (StringT) lhs;
		//                  StringT s2 = (StringT) rhs;
		//                  return stringOf(String.format("%s and %s are shaking hands.\n", s1, s2));
		//                }));
		//    Program prg = e.program(astIss.getAst(), funcs);
		//
		//    // Evaluate the program against some inputs. Note: the details return is not used.
		//    EvalResult out = prg.eval(mapOf("i", "CEL", "you", (Supplier) () -> stringOf("world")));
		//
		//    System.out.println(out);
		//    // Output:CEL and world are shaking hands.
		//  }
	  */
	  }
}