using System;
using System.Collections.Generic;
using Antlr4.Runtime.Sharpen;
using Cel;
using Cel.Checker;
using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter;
using Cel.Interpreter.Functions;
using Cel.Parser;
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
namespace Cel
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.That;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.Assert.ThatThrownBy;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.astToCheckedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.astToParsedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.astToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.attributePattern;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.checkedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.estimateCost;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.noVars;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.parsedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.Cel.partialVars;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.newCustomEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.Env.NewEnv;
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
//	import static org.projectnessie.cel.Util.TestUtil.MapOf;
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

    [TestFixture]
    public class CELTest
    {
        [Test]
        public virtual void AstToProto()
        {
            Env stdEnv =
                Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("a", Decls.Dyn), Decls.NewVar("b", Decls.Dyn)));
            Env.AstIssuesTuple astIss = stdEnv.Parse("a + b");
            Assert.That(astIss.HasIssues(), Is.False);
            ParsedExpr parsed = Cel.AstToParsedExpr(astIss.Ast);
            Ast ast2 = Cel.ParsedExprToAst(parsed);
            Assert.That(ast2.Expr, Is.EqualTo(astIss.Ast.Expr));

            Assert.That(() => Cel.AstToCheckedExpr(astIss.Ast),
                Throws.Exception.TypeOf(typeof(System.ArgumentException)));
            Env.AstIssuesTuple astIss2 = stdEnv.Check(astIss.Ast);
            Assert.That(astIss2.HasIssues(), Is.False);
            // Assert.That(astIss.hasIssues(), Is.False);
            CheckedExpr @checked = Cel.AstToCheckedExpr(astIss2.Ast);
            Ast ast3 = Cel.CheckedExprToAst(@checked);
            Assert.That(ast3.Expr, Is.EqualTo(astIss2.Ast.Expr));
        }

        [Test]
        public virtual void AstToString()
        {
            Env stdEnv = Env.NewEnv();
            string @in = "a + b - (c ? (-d + 4) : e)";
            Env.AstIssuesTuple astIss = stdEnv.Parse(@in);
            Assert.That(astIss.HasIssues(), Is.False);
            string expr = Cel.AstToString(astIss.Ast);
            Assert.That(expr, Is.EqualTo(@in));
        }

        [Test]
        public virtual void CheckedExprToAstConstantExpr()
        {
            Env stdEnv = Env.NewEnv();
            string @in = "10";
            Env.AstIssuesTuple astIss = stdEnv.Compile(@in);
            Assert.That(astIss.HasIssues(), Is.False);
            CheckedExpr expr = Cel.AstToCheckedExpr(astIss.Ast);
            Ast ast2 = Cel.CheckedExprToAst(expr);
            Assert.That(ast2.Expr, Is.EqualTo(astIss.Ast.Expr));
        }


        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void exampleWithBuiltins()
        [Test]
        public virtual void ExampleWithBuiltins()
        {
            // Variables used within this expression environment.
            EnvOption decls =
                EnvOptions.Declarations(Decls.NewVar("i", Decls.String), Decls.NewVar("you", Decls.String));
            Env env = Env.NewEnv(decls);

            // Compile the expression.
            Env.AstIssuesTuple astIss = env.Compile("\"Hello \" + you + \"! I'm \" + i + \".\"");
            Assert.That(astIss.HasIssues(), Is.False);

            // Create the program, and evaluate it against some input.
            Program prg = env.Program(astIss.Ast);

            // If the Eval() call were provided with cel.evalOptions(OptTrackState) the details response
            // (2nd return) would be non-nil.
            Program_EvalResult @out = prg.Eval(TestUtil.MapOf<string, object>("i", "CEL", "you", "world"));

            Assert.That(@out.Val.Equal(StringT.StringOf("Hello world! I'm CEL.")), Is.SameAs(BoolT.True));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void Abbrevs_Compiled()
        [Test]
        public virtual void AbbrevsCompiled()
        {
            // Test whether abbreviations successfully resolve at type-check time (compile time).
            Env env = Env.NewEnv(EnvOptions.Abbrevs("qualified.identifier.name"),
                EnvOptions.Declarations(Decls.NewVar("qualified.identifier.name.first", Decls.String)));
            Env.AstIssuesTuple astIss = env.Compile("\"hello \"+ name.first"); // abbreviation resolved here.
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = env.Program(astIss.Ast);
            Program_EvalResult @out = prg.Eval(TestUtil.MapOf<string, object>("qualified.identifier.name.first", "Jim"));
            Assert.That(@out.Val.Value(), Is.EqualTo("hello Jim"));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void Abbrevs_Parsed()
        [Test]
        public virtual void AbbrevsParsed()
        {
            // Test whether abbreviations are resolved properly at evaluation time.
            Env env = Env.NewEnv(EnvOptions.Abbrevs("qualified.identifier.name"));
            Env.AstIssuesTuple astIss = env.Parse("\"hello \" + name.first");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = env.Program(astIss.Ast); // abbreviation resolved here.
            Program_EvalResult @out =
                prg.Eval(TestUtil.MapOf<string, object>("qualified.identifier.name", TestUtil.MapOf<string, object>("first", "Jim")));
            Assert.That(@out.Val.Value(), Is.EqualTo("hello Jim"));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void Abbrevs_Disambiguation()
        [Test]
        public virtual void AbbrevsDisambiguation()
        {
            Env env = Env.NewEnv(EnvOptions.Abbrevs("external.Expr"), EnvOptions.Container("google.api.expr.v1alpha1"),
                EnvOptions.Types(new Expr()),
                EnvOptions.Declarations(Decls.NewVar("test", Decls.Bool), Decls.NewVar("external.Expr", Decls.String)));
            // This expression will return either a string or a protobuf Expr value depending on the value
            // of the 'test' argument. The fully qualified type name is used indicate that the protobuf
            // typed 'Expr' should be used rather than the abbreviatation for 'external.Expr'.
            Env.AstIssuesTuple astIss = env.Compile("test ? dyn(Expr) : google.api.expr.v1alpha1.Expr{id: 1}");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = env.Program(astIss.Ast);
            Program_EvalResult @out = prg.Eval(TestUtil.MapOf<string, object>("test", true, "external.Expr", "string expr"));
            Assert.That(@out.Val.Value(), Is.EqualTo("string expr"));
            @out = prg.Eval(TestUtil.MapOf<string, object>("test", false, "external.Expr", "wrong expr"));
            Expr want = new Expr();
            want.Id = 1;
            Expr got = (Expr)@out.Val.ConvertToNative(typeof(Expr));
            Assert.That(got, Is.EqualTo(want));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void CustomEnvError()
        [Test]
        public virtual void CustomEnvError()
        {
            Env e = Env.NewCustomEnv(Library.StdLib(), Library.StdLib());
            Env.AstIssuesTuple xIss = e.Compile("a.b.c == true");
            Assert.That(xIss.HasIssues(), Is.True);
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void CustomEnv()
        [Test]
        public virtual void CustomEnv()
        {
            Env e = Env.NewCustomEnv(EnvOptions.Declarations(Decls.NewVar("a.b.c", Decls.Bool)));

            // t.Run("err", func(t *testing.T) {
            Env.AstIssuesTuple xIss = e.Compile("a.b.c == true");
            Assert.That(xIss.HasIssues(), Is.True);

            // t.Run("ok", func(t *testing.T) {
            Env.AstIssuesTuple astIss = e.Compile("a.b.c");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = e.Program(astIss.Ast);
            Program_EvalResult @out = prg.Eval(TestUtil.MapOf<string, object>("a.b.c", true));
            Assert.That(@out.Val, Is.SameAs(BoolT.True));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void HomogeneousAggregateLiterals()
        [Test]
        public virtual void HomogeneousAggregateLiterals()
        {
            Env e = Env.NewCustomEnv(
                EnvOptions.Declarations(Decls.NewVar("name", Decls.String),
                    Decls.NewFunction(Operator.In.id,
                        Decls.NewOverload(Overloads.InList,
                            new List<Type> { Decls.String, Decls.NewListType(Decls.String) }, Decls.Bool),
                        Decls.NewOverload(Overloads.InMap,
                            new List<Type> { Decls.String, Decls.NewMapType(Decls.String, Decls.Bool) }, Decls.Bool))),
                EnvOptions.HomogeneousAggregateLiterals());

            // t.Run("err_list", func(t *testing.T) {
            Env.AstIssuesTuple xIss = e.Compile("name in ['hello', 0]");
            Assert.That(xIss.Issues, Is.Not.Null);
            Assert.That(xIss.HasIssues(), Is.True);
            // })
            // t.Run("err_map_key", func(t *testing.T) {
            xIss = e.Compile("name in {'hello':'world', 1:'!'}");
            Assert.That(xIss.Issues, Is.Not.Null);
            Assert.That(xIss.HasIssues(), Is.True);
            // })
            // t.Run("err_map_val", func(t *testing.T) {
            xIss = e.Compile("name in {'hello':'world', 'goodbye':true}");
            Assert.That(xIss.Issues, Is.Not.Null);
            Assert.That(xIss.HasIssues(), Is.True);
            // })

            ProgramOption funcs = ProgramOptions.Functions(Overload.Binary(Operator.In.id, (lhs, rhs) =>
            {
                if (rhs.Type().HasTrait(Trait.ContainerType))
                {
                    return ((Container)rhs).Contains(lhs);
                }

                return Err.ValOrErr(rhs, "no such overload");
            }));
            // t.Run("ok_list", func(t *testing.T) {
            Env.AstIssuesTuple astIss = e.Compile("name in ['hello', 'world']");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = e.Program(astIss.Ast, funcs);
            Program_EvalResult @out = prg.Eval(TestUtil.MapOf<string, object>("name", "world"));
            Assert.That(@out.Val, Is.SameAs(BoolT.True));
            // })
            // t.Run("ok_map", func(t *testing.T) {
            astIss = e.Compile("name in {'hello': false, 'world': true}");
            Assert.That(astIss.HasIssues(), Is.False);
            prg = e.Program(astIss.Ast, funcs);
            @out = prg.Eval(TestUtil.MapOf<string, object>("name", "world"));
            Assert.That(@out.Val, Is.SameAs(BoolT.True));
            // })
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void Customtypes()
        [Test]
        public virtual void Customtypes()
        {
            Type exprType = Decls.NewObjectType("google.api.expr.v1alpha1.Expr");
            TypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
            Env e = Env.NewEnv(EnvOptions.CustomTypeAdapter(reg.ToTypeAdapter()), EnvOptions.CustomTypeProvider(reg),
                EnvOptions.Container("google.api.expr.v1alpha1"),
                EnvOptions.Types(new Expr(), BoolT.BoolType, IntT.IntType, StringT.StringType),
                EnvOptions.Declarations(Decls.NewVar("expr", exprType)));

            Env.AstIssuesTuple astIss = e.Compile("expr == Expr{id: 2,\n" + "\t\t\tcall_expr: Expr.Call{\n" +
                                                  "\t\t\t\tfunction: \"_==_\",\n" + "\t\t\t\targs: [\n" +
                                                  "\t\t\t\t\tExpr{id: 1, ident_expr: Expr.Ident{ name: \"a\" }},\n" +
                                                  "\t\t\t\t\tExpr{id: 3, ident_expr: Expr.Ident{ name: \"b\" }}]\n" +
                                                  "\t\t\t}}");
            Assert.That(astIss.Ast.ResultType, Is.EqualTo(Decls.Bool));
            Program prg = e.Program(astIss.Ast);

            Ident ident1 = new Ident();
            ident1.Name = "a";
            Ident ident2 = new Ident();
            ident2.Name = "b";
            Expr expr1 = new Expr();
            expr1.Id = 1;
            expr1.IdentExpr = ident1;
            Expr expr3 = new Expr();
            expr3.Id = 3;
            expr3.IdentExpr = ident2;
            Call call = new Call();
            call.Function = "_==_";
            call.Args.Add(new List<Expr> { expr1, expr3 });
            Expr expr2 = new Expr();
            expr2.Id = 2;
            expr2.CallExpr = call;
            object vars = TestUtil.MapOf<string, object>("expr", expr2);
            Program_EvalResult @out = prg.Eval(vars);
            Assert.That(@out.Val, Is.SameAs(BoolT.True));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void TypeIsolation()
        [Test]
        public virtual void TypeIsolation()
        {
            //	b = ioutil.ReadFile("testdata/team.fds")
            //	var fds descpb.FileDescriptorSet
            //	if err = proto.Unmarshal(b, &fds); err != nil {
            //		t.Fatal("can't unmarshal descriptor data: ", err)
            //	}
            //
            //	Env e = Env.NewEnv(
            //		typeDescs(&fds),
            //		EnvOptions.Declarations(
            //			Decls.newVar("myteam",
            //				Decls.newObjectType("cel.testdata.Team"))));
            //
            //	String src = "myteam.members[0].name == 'Cyclops'";
            //	AstIssuesTuple xIss = e.compile(src)
            //	Assert.That(xIss.getIssues().err()).isNull();
            //
            //	// Ensure that isolated types don't leak through.
            //	Env e2 = Env.NewEnv(
            //		EnvOptions.Declarations(
            //			Decls.newVar("myteam",
            //				Decls.newObjectType("cel.testdata.Team"))))
            //	xIss = e2.compile(src)
            //	if iss == nil || iss.Err() == nil {
            //		t.Errorf("wanted compile failure for unknown message.")
            //	}
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test @Disabled("IMPLEMENT ME") void DynamicProto()
        [Test]
        public virtual void DynamicProto()
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
            //	Env e = Env.NewEnv(
            //		EnvOptions.Container("cel"),
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
            //	Assert.That(astIss.hasIssues(), Is.False);
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
        [Test]
        public virtual void DynamicProtoInput()
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
            //	Env e = Env.NewEnv(
            //		// The following is identical to registering the FileDescriptorSet;
            //		// however, it tests a different code path which aggregates individual
            //		// FileDescriptorProto values together.
            //		typeDescs(fileCopy...),
            //		EnvOptions.Declarations(Decls.newVar("mutant", Decls.newObjectType("cel.testdata.Mutant"))),
            //	);
            //	src = `has(mutant.name) && mutant.name == 'Wolverine'`
            //	AstIssuesTuple astIss = e.compile(src);
            //	Assert.That(astIss.hasIssues(), Is.False);
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
        [Test]
        public virtual void GlobalVars()
        {
            Type mapStrDyn = Decls.NewMapType(Decls.String, Decls.Dyn);
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("attrs", mapStrDyn),
                Decls.NewVar("default", Decls.Dyn),
                Decls.NewFunction("get",
                    Decls.NewInstanceOverload("get_map", new List<Type> { mapStrDyn, Decls.String, Decls.Dyn },
                        Decls.Dyn))));
            Env.AstIssuesTuple astIss = e.Compile("attrs.get(\"first\", attrs.get(\"second\", default))");

            // Create the program.
            ProgramOption funcs = ProgramOptions.Functions(Overload.Function("get", args =>
            {
                if (args.Length != 3)
                {
                    return Err.NewErr("invalid arguments to 'get'");
                }

                if (!(args[0] is Mapper))
                {
                    return Err.NewErr("invalid operand of type '%s' to obj.get(key, def)", args[0].Type());
                }

                Mapper attrs = (Mapper)args[0];
                if (!(args[1] is StringT))
                {
                    return Err.NewErr("invalid key of type '%s' to obj.get(key, def)", args[1].Type());
                }

                StringT key = (StringT)args[1];
                Val defVal = args[2];
                if (attrs.Contains(key) == BoolT.True)
                {
                    return attrs.Get(key);
                }

                return defVal;
            }));

            // Global variables can be configured as a ProgramOption and optionally overridden on Eval.
            Program prg = e.Program(astIss.Ast, funcs, ProgramOptions.Globals(TestUtil.MapOf<string, object>("default", "third")));

            // t.Run("global_default", func(t *testing.T) {
            object vars = TestUtil.MapOf<string, object>("attrs", TestUtil.MapOf<string, object>());
            Program_EvalResult @out = prg.Eval(vars);
            Assert.That(@out.Val.Equal(StringT.StringOf("third")), Is.SameAs(BoolT.True));
            // })

            // t.Run("attrs_alt", func(t *testing.T) {
            vars = TestUtil.MapOf<string, object>("attrs", TestUtil.MapOf<string, object>("second", "yep"));
            @out = prg.Eval(vars);
            Assert.That(@out.Val.Equal(StringT.StringOf("yep")), Is.SameAs(BoolT.True));
            // })

            // t.Run("local_default", func(t *testing.T) {
            vars = TestUtil.MapOf<string, object>("attrs", TestUtil.MapOf<string, object>(), "default", "fourth");
            @out = prg.Eval(vars);
            Assert.That(@out.Val.Equal(StringT.StringOf("fourth")), Is.SameAs(BoolT.True));
            // })
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void CustomMacro()
        [Test]
        public virtual void CustomMacro()
        {
            Macro joinMacro = Macro.NewReceiverMacro("join", 1, (eh, target, args) =>
            {
                Expr delim = args[0];
                Expr iterIdent = eh.Ident("__iter__");
                Expr accuIdent = eh.Ident("__result__");
                Expr init = eh.LiteralString("");
                Expr condition = eh.LiteralBool(true);
                Expr step = eh.GlobalCall(Operator.Conditional.id,
                    eh.GlobalCall(Operator.Greater.id, eh.ReceiverCall("size", accuIdent, new List<Expr>()),
                        eh.LiteralInt(0)),
                    eh.GlobalCall(Operator.Add.id, eh.GlobalCall(Operator.Add.id, accuIdent, delim), iterIdent),
                    iterIdent);
                return eh.Fold("__iter__", target, "__result__", init, condition, step, accuIdent);
            });
            Env e = Env.NewEnv(EnvOptions.Macros(joinMacro));
            Env.AstIssuesTuple astIss = e.Compile("['hello', 'cel', 'friend'].join(',')");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = e.Program(astIss.Ast, ProgramOptions.EvalOptions(EvalOption.OptExhaustiveEval));
            Program_EvalResult @out = prg.Eval(Cel.NoVars());
            Assert.That(@out.Val.Equal(StringT.StringOf("hello,cel,friend")), Is.SameAs(BoolT.True));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void AstIsChecked()
        [Test]
        public virtual void AstIsChecked()
        {
            Env e = Env.NewEnv();
            Env.AstIssuesTuple astIss = e.Compile("true");
            Assert.That(astIss.HasIssues(), Is.False);
            //JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(astIss.Ast.Checked, Is.EqualTo(true));
            CheckedExpr ce = Cel.AstToCheckedExpr(astIss.Ast);
            Ast ast2 = Cel.CheckedExprToAst(ce);
            //JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
            Assert.That(ast2.Checked, Is.EqualTo(true));
            Assert.That(astIss.Ast.Expr, Is.EqualTo(ast2.Expr));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void EvalOptions()
        [Test]
        public virtual void EvalOptions()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("k", Decls.String), Decls.NewVar("v", Decls.Bool)));
            Env.AstIssuesTuple astIss = e.Compile("{k: true}[k] || v != false");

            Program prg = e.Program(astIss.Ast, ProgramOptions.EvalOptions(EvalOption.OptExhaustiveEval));
            Program_EvalResult outDetails = prg.Eval(TestUtil.MapOf<string, object>("k", "key", "v", true));
            Assert.That(outDetails.Val, Is.SameAs(BoolT.True));

            // Test to see whether 'v != false' was resolved to a value.
            // With short-circuiting it normally wouldn't be.
            EvalState s = outDetails.EvalDetails.State;
            Val lhsVal = s.Value(astIss.Ast.Expr.CallExpr.Args[0].Id);
            Assert.That(lhsVal, Is.SameAs(BoolT.True));
            Val rhsVal = s.Value(astIss.Ast.Expr.CallExpr.Args[1].Id);
            Assert.That(rhsVal, Is.SameAs(BoolT.True));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void EvalRecover()
        [Test]
        public virtual void EvalRecover()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewFunction("panic",
                Decls.NewOverload("panic", new List<Type> { new Type() }, Decls.Bool))));
            ProgramOption funcs =
                ProgramOptions.Functions(Overload.Function("panic",
                    args => { throw new Exception("watch me recover"); }));
            // Test standard evaluation.
            Env.AstIssuesTuple pAst = e.Parse("panic()");
            Program prgm1 = e.Program(pAst.Ast, funcs);
            Assert.That(() => prgm1.Eval(new Dictionary<object, object>()), Throws.Exception.TypeOf(typeof(Exception)));
            // Test the factory-based evaluation.
            Program prgm2 = e.Program(pAst.Ast, funcs, ProgramOptions.EvalOptions(EvalOption.OptTrackState));
            Assert.That(() => prgm2.Eval(new Dictionary<object, object>()), Throws.Exception.TypeOf(typeof(Exception)));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void ResidualAst()
        [Test]
        public virtual void ResidualAst()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("x", Decls.Int), Decls.NewVar("y", Decls.Int)));
            Activation_PartialActivation unkVars = e.UnknownVars;
            Env.AstIssuesTuple astIss = e.Parse("x < 10 && (y == 0 || 'hello' != 'goodbye')");
            Program prg = e.Program(astIss.Ast,
                ProgramOptions.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
            Program_EvalResult outDet = prg.Eval(unkVars);
            Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
            Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
            string expr = Cel.AstToString(residual);
            Assert.That(expr, Is.EqualTo("x < 10"));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void ResidualAst_Complex()
        [Test]
        public virtual void ResidualAstComplex()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("resource.name", Decls.String),
                Decls.NewVar("request.time", Decls.Timestamp),
                Decls.NewVar("request.auth.claims", Decls.NewMapType(Decls.String, Decls.String))));
            Activation_PartialActivation unkVars = Cel.PartialVars(
                TestUtil.MapOf<string, object>("resource.name", "bucket/my-bucket/objects/private", "request.auth.claims",
                    TestUtil.MapOf<string, object>("email_verified", "true")),
                Cel.NewAttributePattern("request.auth.claims").QualString("email"));
            Env.AstIssuesTuple astIss = e.Compile("resource.name.startsWith(\"bucket/my-bucket\") &&\n" +
                                                  "\t\t bool(request.auth.claims.email_verified) == true &&\n" +
                                                  "\t\t request.auth.claims.email == \"wiley@acme.co\"");
            Assert.That(astIss.HasIssues(), Is.False);
            Program prg = e.Program(astIss.Ast,
                ProgramOptions.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
            Program_EvalResult outDet = prg.Eval(unkVars);
            Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
            Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
            string expr = Cel.AstToString(residual);
            Assert.That(expr, Is.EqualTo("request.auth.claims.email == \"wiley@acme.co\""));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void EnvExtension()

        // TODO protobuf
        /*
              [Test]
public virtual void EnvExtension()
              {
                Env e = Env.NewEnv(EnvOptions.Container("google.api.expr.v1alpha1"), EnvOptions.Types(new Expr()), EnvOptions.Declarations(Decls.NewVar("expr", Decls.NewObjectType("google.api.expr.v1alpha1.Expr"))));
                Env e2 = e.Extend(EnvOptions.CustomTypeAdapter(DefaultTypeAdapter.Instance), EnvOptions.Types(com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()));
                Assert.That(e, Is.Not.EqualTo(e2));
                Assert.That(e.TypeAdapter, Is.Not.EqualTo(e2.TypeAdapter));
                Assert.That(e.TypeProvider, Is.Not.EqualTo(e2.TypeProvider));
                Env e3 = e2.Extend();
                Assert.That(e2.TypeAdapter, Is.EqualTo(e3.TypeAdapter));
                Assert.That(e2.TypeProvider, Is.EqualTo(e3.TypeProvider));
              }
              */

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void EnvExtensionIsolation()
        // TODO protobuf
        /*
              [Test]
public virtual void EnvExtensionIsolation()
              {
                Env baseEnv = Env.NewEnv(EnvOptions.Container("google.api.expr.test.v1"), EnvOptions.Declarations(Decls.NewVar("age", Decls.Int), Decls.NewVar("gender", Decls.String), Decls.NewVar("country", Decls.String)));
                Env env1 = baseEnv.Extend(types(com.google.api.expr.test.v1.proto2.TestAllTypesProto.TestAllTypes.getDefaultInstance()), EnvOptions.Declarations(Decls.NewVar("name", Decls.String)));
                Env env2 = baseEnv.Extend(types(com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()), EnvOptions.Declarations(Decls.NewVar("group", Decls.String)));
                AstIssuesTuple astIss = env2.Compile("size(group) > 10 && !has(proto3.TestAllTypes{}.single_int32)");
                Assert.That(astIss.HasIssues(), Is.False);
                astIss = env2.Compile("size(name) > 10");
                Assert.That(astIss.HasIssues(), Is.True);
                astIss = env2.Compile("!has(proto2.TestAllTypes{}.single_int32)");
                Assert.That(astIss.HasIssues(), Is.True);
        
                astIss = env1.Compile("size(name) > 10 && !has(proto2.TestAllTypes{}.single_int32)");
                Assert.That(astIss.HasIssues(), Is.False);
                astIss = env1.Compile("size(group) > 10");
                Assert.That(astIss.HasIssues(), Is.True);
                astIss = env1.Compile("!has(proto3.TestAllTypes{}.single_int32)");
                Assert.That(astIss.HasIssues(), Is.True);
              }
              */

        [Test]
        public virtual void CustomInterpreterDecorator()
        {
            AtomicReference<Interpretable> lastInstruction = new AtomicReference<Interpretable>();
            InterpretableDecorator optimizeArith = i =>
            {
                lastInstruction.Set(i);
                // Only optimize the instruction if it is a call.
                if (!(i is Interpretable_InterpretableCall))
                {
                    return i;
                }

                Interpretable_InterpretableCall call = (Interpretable_InterpretableCall)i;
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
                        if (!(args[0] is Interpretable_InterpretableConst) ||
                            !(args[1] is Interpretable_InterpretableConst))
                        {
                            return i;
                        }

                        Val val = call.Eval(Activation.EmptyActivation());
                        if (Err.IsError(val))
                        {
                            throw new Exception(val.ToString());
                        }

                        return Interpretable.NewConstValue(call.Id(), val);
                    default:
                        return i;
                }
            };

            Env env = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("foo", Decls.Int)));
            Env.AstIssuesTuple astIss = env.Compile("foo == -1 + 2 * 3 / 3");
            env.Program(astIss.Ast, ProgramOptions.EvalOptions(EvalOption.OptPartialEval),
                ProgramOptions.CustomDecorator(optimizeArith));
            Assert.That(lastInstruction.Get(), Is.InstanceOf(typeof(Interpretable_InterpretableCall)));
            Interpretable_InterpretableCall call = (Interpretable_InterpretableCall)lastInstruction.Get();
            Interpretable[] args = call.Args();
            Interpretable lhs = args[0];
            Assert.That(lhs, Is.InstanceOf(typeof(Interpretable_InterpretableAttribute)));
            Interpretable_InterpretableAttribute lastAttr = (Interpretable_InterpretableAttribute)lhs;
            AttributeFactory_NamespacedAttribute absAttr = (AttributeFactory_NamespacedAttribute)lastAttr.Attr();
            string[] varNames = absAttr.CandidateVariableNames();
            Assert.That(varNames, Has.Exactly(1).EqualTo("foo"));
            Interpretable rhs = args[1];
            Assert.That(rhs, Is.InstanceOf(typeof(Interpretable_InterpretableConst)));
            Interpretable_InterpretableConst lastConst = (Interpretable_InterpretableConst)rhs;
            // This is the last number produced by the optimization.
            Assert.That(lastConst.Value(), Is.SameAs(IntT.IntOne));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void Cost()
        [Test]
        public virtual void Cost()
        {
            Env e = Env.NewEnv();
            Env.AstIssuesTuple astIss = e.Compile("\"Hello, World!\"");
            Assert.That(astIss.HasIssues(), Is.False);

            Coster_Cost wantedCost = Coster_Cost.None;

            // Test standard evaluation cost.
            Program prg = e.Program(astIss.Ast);
            Coster_Cost c = Coster_Cost.EstimateCost(prg);
            Assert.That(c, Is.EqualTo(wantedCost));

            // Test the factory-based evaluation cost.
            prg = e.Program(astIss.Ast, ProgramOptions.EvalOptions(EvalOption.OptExhaustiveEval));
            c = Coster_Cost.EstimateCost(prg);
            Assert.That(c, Is.EqualTo(wantedCost));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void ResidualAst_AttributeQualifiers()
        [Test]
        public virtual void ResidualAstAttributeQualifiers()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Dyn)),
                Decls.NewVar("y", Decls.NewListType(Decls.Int)), Decls.NewVar("u", Decls.Int)));
            Env.AstIssuesTuple astIss =
                e.Parse(
                    "x.abc == u && x[\"abc\"] == u && x[x.string] == u && y[0] == u && y[x.zero] == u && (true ? x : y).abc == u && (false ? y : x).abc == u");
            Program prg = e.Program(astIss.Ast,
                ProgramOptions.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
            Activation_PartialActivation vars = Cel.PartialVars(
                TestUtil.MapOf<string, object>("x", TestUtil.MapOf<string, object>("zero", 0, "abc", 123, "string", "abc"), "y",
                    new List<int> { 123 }), Cel.NewAttributePattern("u"));
            Program_EvalResult outDet = prg.Eval(vars);
            Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
            Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
            string expr = Cel.AstToString(residual);
            Assert.That(expr,
                Is.EqualTo("123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u"));
        }

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Test void residualAst_Modified()
        [Test]
        public virtual void ResidualAstModified()
        {
            Env e = Env.NewEnv(EnvOptions.Declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Int)),
                Decls.NewVar("y", Decls.Int)));
            Env.AstIssuesTuple astIss = e.Parse("x == y");
            Program prg = e.Program(astIss.Ast,
                ProgramOptions.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
            for (int x = 123; x < 456; x++)
            {
                Activation_PartialActivation vars =
                    Cel.PartialVars(TestUtil.MapOf<string, object>("x", x), Cel.NewAttributePattern("y"));
                Program_EvalResult outDet = prg.Eval(vars);
                Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
                Ast residual = e.ResidualAst(astIss.Ast, outDet.EvalDetails);
                string orig = Cel.AstToString(astIss.Ast);
                Assert.That(orig, Is.EqualTo("x == y"));
                string expr = Cel.AstToString(residual);
                string want = string.Format("{0:D} == y", x);
                Assert.That(expr, Is.EqualTo(want));
            }
        }

        //  @SuppressWarnings("rawtypes")
        //  static void Example() {
        //    // Create the CEL environment with declarations for the input attributes and
        //    // the desired extension functions. In many cases the desired functionality will
        //    // be present in a built-in function.
        //    EnvOption decls =
        //        EnvOptions.Declarations(
        //            // Identifiers used within this expression.
        //            Decls.newVar("i", Decls.String),
        //            Decls.newVar("you", Decls.String),
        //            // Function to generate a greeting from one person to another.
        //            //    i.greet(you)
        //            Decls.newFunction(
        //                "greet",
        //                Decls.newInstanceOverload(
        //                    "string_greet_string", asList(Decls.String, Decls.String), Decls.String)));
        //    Env e = Env.NewEnv(decls);
        //
        //    // Compile the expression.
        //    AstIssuesTuple astIss = e.compile("i.greet(you)");
        //    Assert.That(astIss.hasIssues(), Is.False);
        //
        //    // Create the program.
        //    ProgramOption funcs =
        //        ProgramOptions.Functions(
        //            Overload.binary(
        //                "string_greet_string",
        //                (lhs, rhs) ->
        //                    StringT.StringOf(String.format("Hello %s! Nice to meet you, I'm %s.\n", rhs,
        // lhs))));
        //    Program prg = e.program(astIss.getAst(), funcs);
        //
        //    // Evaluate the program against some inputs. Note: the details return is not used.
        //    EvalResult out =
        //        prg.eval(
        //            TestUtil.MapOf<string, object>(
        //                // Native values are converted to CEL values under the covers.
        //                "i",
        //                "CEL",
        //                // Values may also be lazily supplied.
        //                "you",
        //                (Supplier) () -> StringT.StringOf("world")));
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
        //        EnvOptions.Declarations(
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
        //    Env e = Env.NewEnv(decls);
        //
        //    // Compile the expression.
        //    AstIssuesTuple astIss = e.compile("shake_hands(i,you)");
        //    Assert.That(astIss.hasIssues(), Is.False);
        //
        //    // Create the program.
        //    ProgramOption funcs =
        //        ProgramOptions.Functions(
        //            Overload.binary(
        //                "shake_hands_string_string",
        //                (lhs, rhs) -> {
        //                  if (!(lhs instanceof StringT)) {
        //                    return Err.ValOrErr(lhs, "unexpected type '%s' passed to shake_hands",
        // lhs.type());
        //                  }
        //                  if (!(rhs instanceof StringT)) {
        //                    return Err.ValOrErr(rhs, "unexpected type '%s' passed to shake_hands",
        // rhs.type());
        //                  }
        //                  StringT s1 = (StringT) lhs;
        //                  StringT s2 = (StringT) rhs;
        //                  return StringT.StringOf(String.format("%s and %s are shaking hands.\n", s1, s2));
        //                }));
        //    Program prg = e.program(astIss.getAst(), funcs);
        //
        //    // Evaluate the program against some inputs. Note: the details return is not used.
        //    EvalResult out = prg.eval(TestUtil.MapOf<string, object>("i", "CEL", "you", (Supplier) () -> StringT.StringOf("world")));
        //
        //    System.out.println(out);
        //    // Output:CEL and world are shaking hands.
        //  }
    }
}