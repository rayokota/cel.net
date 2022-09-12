using Antlr4.Runtime.Sharpen;
using Cel.Checker;
using Cel.Common.Operators;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Cel.Interpreter;
using Cel.Interpreter.Functions;
using Cel.Parser;
using Google.Api.Expr.Test.V1.Proto3;
using Google.Api.Expr.V1Alpha1;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel;

[TestFixture]
public class CELTest
{
    [Test]
    public virtual void AstToProto()
    {
        var stdEnv =
            Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("a", Decls.Dyn), Decls.NewVar("b", Decls.Dyn)));
        var astIss = stdEnv.Parse("a + b");
        Assert.That(astIss.HasIssues(), Is.False);
        var parsed = Cel.AstToParsedExpr(astIss.Ast!);
        var ast2 = Cel.ParsedExprToAst(parsed);
        Assert.That(ast2.Expr, Is.EqualTo(astIss.Ast!.Expr));

        Assert.That(() => Cel.AstToCheckedExpr(astIss.Ast!),
            Throws.Exception.TypeOf(typeof(ArgumentException)));
        var astIss2 = stdEnv.Check(astIss.Ast!);
        Assert.That(astIss2.HasIssues(), Is.False);
        // Assert.That(astIss.hasIssues(), Is.False);
        var @checked = Cel.AstToCheckedExpr(astIss2.Ast!);
        var ast3 = Cel.CheckedExprToAst(@checked);
        Assert.That(ast3.Expr, Is.EqualTo(astIss2.Ast!.Expr));
    }

    [Test]
    public virtual void AstToString()
    {
        var stdEnv = Env.NewEnv();
        var @in = "a + b - (c ? (-d + 4) : e)";
        var astIss = stdEnv.Parse(@in);
        Assert.That(astIss.HasIssues(), Is.False);
        var expr = Cel.AstToString(astIss.Ast!);
        Assert.That(expr, Is.EqualTo(@in));
    }

    [Test]
    public virtual void CheckedExprToAstConstantExpr()
    {
        var stdEnv = Env.NewEnv();
        var @in = "10";
        var astIss = stdEnv.Compile(@in);
        Assert.That(astIss.HasIssues(), Is.False);
        var expr = Cel.AstToCheckedExpr(astIss.Ast!);
        var ast2 = Cel.CheckedExprToAst(expr);
        Assert.That(ast2.Expr, Is.EqualTo(astIss.Ast!.Expr));
    }


    [Test]
    public virtual void ExampleWithBuiltins()
    {
        // Variables used within this expression environment.
        var decls =
            IEnvOption.Declarations(Decls.NewVar("i", Decls.String), Decls.NewVar("you", Decls.String));
        var env = Env.NewEnv(decls);

        // Compile the expression.
        var astIss = env.Compile("\"Hello \" + you + \"! I'm \" + i + \".\"");
        Assert.That(astIss.HasIssues(), Is.False);

        // Create the program, and evaluate it against some input.
        var prg = env.Program(astIss.Ast!);

        // If the Eval() call were provided with cel.evalOptions(OptTrackState) the details response
        // (2nd return) would be non-nil.
        var @out = prg.Eval(TestUtil.BindingsOf("i", "CEL", "you", "world"));

        Assert.That(@out.Val.Equal(StringT.StringOf("Hello world! I'm CEL.")), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void AbbrevsCompiled()
    {
        // Test whether abbreviations successfully resolve at type-check time (compile time).
        var env = Env.NewEnv(IEnvOption.Abbrevs("qualified.identifier.name"),
            IEnvOption.Declarations(Decls.NewVar("qualified.identifier.name.first", Decls.String)));
        var astIss = env.Compile("\"hello \"+ name.first"); // abbreviation resolved here.
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = env.Program(astIss.Ast!);
        var @out = prg.Eval(TestUtil.BindingsOf("qualified.identifier.name.first", "Jim"));
        Assert.That(@out.Val.Value(), Is.EqualTo("hello Jim"));
    }

    [Test]
    public virtual void AbbrevsParsed()
    {
        // Test whether abbreviations are resolved properly at evaluation time.
        var env = Env.NewEnv(IEnvOption.Abbrevs("qualified.identifier.name"));
        var astIss = env.Parse("\"hello \" + name.first");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = env.Program(astIss.Ast!); // abbreviation resolved here.
        var @out =
            prg.Eval(TestUtil.BindingsOf("qualified.identifier.name",
                TestUtil.BindingsOf("first", "Jim")));
        Assert.That(@out.Val.Value(), Is.EqualTo("hello Jim"));
    }

    [Test]
    public virtual void AbbrevsDisambiguation()
    {
        var env = Env.NewEnv(IEnvOption.Abbrevs("external.Expr"), IEnvOption.Container("google.api.expr.v1alpha1"),
            IEnvOption.Types(new Expr()),
            IEnvOption.Declarations(Decls.NewVar("test", Decls.Bool), Decls.NewVar("external.Expr", Decls.String)));
        // This expression will return either a string or a protobuf Expr value depending on the value
        // of the 'test' argument. The fully qualified type name is used indicate that the protobuf
        // typed 'Expr' should be used rather than the abbreviatation for 'external.Expr'.
        var astIss = env.Compile("test ? dyn(Expr) : google.api.expr.v1alpha1.Expr{id: 1}");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = env.Program(astIss.Ast!);
        var @out = prg.Eval(TestUtil.BindingsOf("test", true, "external.Expr", "string expr"));
        Assert.That(@out.Val.Value(), Is.EqualTo("string expr"));
        @out = prg.Eval(TestUtil.BindingsOf("test", false, "external.Expr", "wrong expr"));
        var want = new Expr();
        want.Id = 1;
        var got = (Expr)@out.Val.ConvertToNative(typeof(Expr));
        Assert.That(got, Is.EqualTo(want));
    }

    [Test]
    public virtual void CustomEnvError()
    {
        var e = Env.NewCustomEnv(ILibrary.StdLib(), ILibrary.StdLib());
        var xIss = e.Compile("a.b.c == true");
        Assert.That(xIss.HasIssues(), Is.True);
    }

    [Test]
    public virtual void CustomEnv()
    {
        var e = Env.NewCustomEnv(IEnvOption.Declarations(Decls.NewVar("a.b.c", Decls.Bool)));

        // t.Run("err", func(t *testing.T) {
        var xIss = e.Compile("a.b.c == true");
        Assert.That(xIss.HasIssues(), Is.True);

        // t.Run("ok", func(t *testing.T) {
        var astIss = e.Compile("a.b.c");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = e.Program(astIss.Ast!);
        var @out = prg.Eval(TestUtil.BindingsOf("a.b.c", true));
        Assert.That(@out.Val, Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void HomogeneousAggregateLiterals()
    {
        var e = Env.NewCustomEnv(
            IEnvOption.Declarations(Decls.NewVar("name", Decls.String),
                Decls.NewFunction(Operator.In.Id,
                    Decls.NewOverload(Overloads.InList,
                        new List<Type> { Decls.String, Decls.NewListType(Decls.String) }, Decls.Bool),
                    Decls.NewOverload(Overloads.InMap,
                        new List<Type> { Decls.String, Decls.NewMapType(Decls.String, Decls.Bool) }, Decls.Bool))),
            IEnvOption.HomogeneousAggregateLiterals());

        // t.Run("err_list", func(t *testing.T) {
        var xIss = e.Compile("name in ['hello', 0]");
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

        var funcs = IProgramOption.Functions(Overload.Binary(Operator.In.Id, (lhs, rhs) =>
        {
            if (rhs.Type().HasTrait(Trait.ContainerType)) return ((IContainer)rhs).Contains(lhs);

            return Err.ValOrErr(rhs, "no such overload");
        }));
        // t.Run("ok_list", func(t *testing.T) {
        var astIss = e.Compile("name in ['hello', 'world']");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = e.Program(astIss.Ast!, funcs);
        var @out = prg.Eval(TestUtil.BindingsOf("name", "world"));
        Assert.That(@out.Val, Is.SameAs(BoolT.True));
        // })
        // t.Run("ok_map", func(t *testing.T) {
        astIss = e.Compile("name in {'hello': false, 'world': true}");
        Assert.That(astIss.HasIssues(), Is.False);
        prg = e.Program(astIss.Ast!, funcs);
        @out = prg.Eval(TestUtil.BindingsOf("name", "world"));
        Assert.That(@out.Val, Is.SameAs(BoolT.True));
        // })
    }

    [Test]
    public virtual void Customtypes()
    {
        var exprType = Decls.NewObjectType("google.api.expr.v1alpha1.Expr");
        ITypeRegistry reg = ProtoTypeRegistry.NewEmptyRegistry();
        var e = Env.NewEnv(IEnvOption.CustomTypeAdapter(reg.ToTypeAdapter()), IEnvOption.CustomTypeProvider(reg),
            IEnvOption.Container("google.api.expr.v1alpha1"),
            IEnvOption.Types(new Expr(), BoolT.BoolType, IntT.IntType, StringT.StringType),
            IEnvOption.Declarations(Decls.NewVar("expr", exprType)));

        var astIss = e.Compile("expr == Expr{id: 2,\n" + "\t\t\tcall_expr: Expr.Call{\n" +
                               "\t\t\t\tfunction: \"_==_\",\n" + "\t\t\t\targs: [\n" +
                               "\t\t\t\t\tExpr{id: 1, ident_expr: Expr.Ident{ name: \"a\" }},\n" +
                               "\t\t\t\t\tExpr{id: 3, ident_expr: Expr.Ident{ name: \"b\" }}]\n" +
                               "\t\t\t}}");
        Assert.That(astIss.Ast!.ResultType, Is.EqualTo(Decls.Bool));
        var prg = e.Program(astIss.Ast);

        var ident1 = new Expr.Types.Ident();
        ident1.Name = "a";
        var ident2 = new Expr.Types.Ident();
        ident2.Name = "b";
        var expr1 = new Expr();
        expr1.Id = 1;
        expr1.IdentExpr = ident1;
        var expr3 = new Expr();
        expr3.Id = 3;
        expr3.IdentExpr = ident2;
        var call = new Expr.Types.Call();
        call.Function = "_==_";
        call.Args.Add(new List<Expr> { expr1, expr3 });
        var expr2 = new Expr();
        expr2.Id = 2;
        expr2.CallExpr = call;
        object vars = TestUtil.BindingsOf("expr", expr2);
        var @out = prg.Eval(vars);
        Assert.That(@out.Val, Is.SameAs(BoolT.True));
    }

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

    [Test]
    public virtual void GlobalVars()
    {
        var mapStrDyn = Decls.NewMapType(Decls.String, Decls.Dyn);
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("attrs", mapStrDyn),
            Decls.NewVar("default", Decls.Dyn),
            Decls.NewFunction("get",
                Decls.NewInstanceOverload("get_map", new List<Type> { mapStrDyn, Decls.String, Decls.Dyn },
                    Decls.Dyn))));
        var astIss = e.Compile("attrs.get(\"first\", attrs.get(\"second\", default))");

        // Create the program.
        var funcs = IProgramOption.Functions(Overload.Function("get", args =>
        {
            if (args.Length != 3) return Err.NewErr("invalid arguments to 'get'");

            if (!(args[0] is IMapper))
                return Err.NewErr("invalid operand of type '{0}' to obj.get(key, def)", args[0].Type());

            var attrs = (IMapper)args[0];
            if (!(args[1] is StringT))
                return Err.NewErr("invalid key of type '{0}' to obj.get(key, def)", args[1].Type());

            var key = (StringT)args[1];
            var defVal = args[2];
            if (attrs.Contains(key) == BoolT.True) return attrs.Get(key);

            return defVal;
        }));

        // Global variables can be configured as a ProgramOption and optionally overridden on Eval.
        var prg = e.Program(astIss.Ast!, funcs,
            IProgramOption.Globals(TestUtil.BindingsOf("default", "third")));

        // t.Run("global_default", func(t *testing.T) {
        object vars = TestUtil.BindingsOf("attrs", TestUtil.BindingsOf());
        var @out = prg.Eval(vars);
        Assert.That(@out.Val.Equal(StringT.StringOf("third")), Is.SameAs(BoolT.True));
        // })

        // t.Run("attrs_alt", func(t *testing.T) {
        vars = TestUtil.BindingsOf("attrs", TestUtil.BindingsOf("second", "yep"));
        @out = prg.Eval(vars);
        Assert.That(@out.Val.Equal(StringT.StringOf("yep")), Is.SameAs(BoolT.True));
        // })

        // t.Run("local_default", func(t *testing.T) {
        vars = TestUtil.BindingsOf("attrs", TestUtil.BindingsOf(), "default", "fourth");
        @out = prg.Eval(vars);
        Assert.That(@out.Val.Equal(StringT.StringOf("fourth")), Is.SameAs(BoolT.True));
        // })
    }

    [Test]
    public virtual void CustomMacro()
    {
        var joinMacro = Macro.NewReceiverMacro("join", 1, (eh, target, args) =>
        {
            var delim = args[0];
            var iterIdent = eh.Ident("__iter__");
            var accuIdent = eh.Ident("__result__");
            var init = eh.LiteralString("");
            var condition = eh.LiteralBool(true);
            var step = eh.GlobalCall(Operator.Conditional.Id,
                eh.GlobalCall(Operator.Greater.Id, eh.ReceiverCall("size", accuIdent, new List<Expr>()),
                    eh.LiteralInt(0)),
                eh.GlobalCall(Operator.Add.Id, eh.GlobalCall(Operator.Add.Id, accuIdent, delim), iterIdent),
                iterIdent);
            return eh.Fold("__iter__", target, "__result__", init, condition, step, accuIdent);
        });
        var e = Env.NewEnv(IEnvOption.Macros(joinMacro));
        var astIss = e.Compile("['hello', 'cel', 'friend'].join(',')");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = e.Program(astIss.Ast!, IProgramOption.EvalOptions(EvalOption.OptExhaustiveEval));
        var @out = prg.Eval(Cel.NoVars());
        Assert.That(@out.Val.Equal(StringT.StringOf("hello,cel,friend")), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void AstIsChecked()
    {
        var e = Env.NewEnv();
        var astIss = e.Compile("true");
        Assert.That(astIss.HasIssues(), Is.False);
        Assert.That(astIss.Ast!.Checked, Is.EqualTo(true));
        var ce = Cel.AstToCheckedExpr(astIss.Ast);
        var ast2 = Cel.CheckedExprToAst(ce);
        Assert.That(ast2.Checked, Is.EqualTo(true));
        Assert.That(astIss.Ast.Expr, Is.EqualTo(ast2.Expr));
    }

    [Test]
    public virtual void EvalOptions()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("k", Decls.String), Decls.NewVar("v", Decls.Bool)));
        var astIss = e.Compile("{k: true}[k] || v != false");

        var prg = e.Program(astIss.Ast!, IProgramOption.EvalOptions(EvalOption.OptExhaustiveEval));
        var outDetails = prg.Eval(TestUtil.BindingsOf("k", "key", "v", true));
        Assert.That(outDetails.Val, Is.SameAs(BoolT.True));

        // Test to see whether 'v != false' was resolved to a value.
        // With short-circuiting it normally wouldn't be.
        var s = outDetails.EvalDetails.State;
        var lhsVal = s.Value(astIss.Ast!.Expr.CallExpr.Args[0].Id);
        Assert.That(lhsVal, Is.SameAs(BoolT.True));
        var rhsVal = s.Value(astIss.Ast!.Expr.CallExpr.Args[1].Id);
        Assert.That(rhsVal, Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void EvalRecover()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewFunction("panic",
            Decls.NewOverload("panic", new List<Type> { new() }, Decls.Bool))));
        var funcs =
            IProgramOption.Functions(Overload.Function("panic",
                args => { throw new Exception("watch me recover"); }));
        // Test standard evaluation.
        var pAst = e.Parse("panic()");
        var prgm1 = e.Program(pAst.Ast!, funcs);
        Assert.That(() => prgm1.Eval(new Dictionary<object, object>()), Throws.Exception.TypeOf(typeof(Exception)));
        // Test the factory-based evaluation.
        var prgm2 = e.Program(pAst.Ast!, funcs, IProgramOption.EvalOptions(EvalOption.OptTrackState));
        Assert.That(() => prgm2.Eval(new Dictionary<object, object>()), Throws.Exception.TypeOf(typeof(Exception)));
    }

    [Test]
    public virtual void ResidualAst()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("x", Decls.Int), Decls.NewVar("y", Decls.Int)));
        var unkVars = e.UnknownVars;
        var astIss = e.Parse("x < 10 && (y == 0 || 'hello' != 'goodbye')");
        var prg = e.Program(astIss.Ast!,
            IProgramOption.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
        var outDet = prg.Eval(unkVars);
        Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
        var residual = e.ResidualAst(astIss.Ast!, outDet.EvalDetails);
        var expr = Cel.AstToString(residual!);
        Assert.That(expr, Is.EqualTo("x < 10"));
    }

    [Test]
    public virtual void ResidualAstComplex()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("resource.name", Decls.String),
            Decls.NewVar("request.time", Decls.Timestamp),
            Decls.NewVar("request.auth.claims", Decls.NewMapType(Decls.String, Decls.String))));
        var unkVars = Cel.PartialVars(
            TestUtil.BindingsOf("resource.name", "bucket/my-bucket/objects/private", "request.auth.claims",
                TestUtil.BindingsOf("email_verified", "true")),
            Cel.NewAttributePattern("request.auth.claims").QualString("email"));
        var astIss = e.Compile("resource.name.startsWith(\"bucket/my-bucket\") &&\n" +
                               "\t\t bool(request.auth.claims.email_verified) == true &&\n" +
                               "\t\t request.auth.claims.email == \"wiley@acme.co\"");
        Assert.That(astIss.HasIssues(), Is.False);
        var prg = e.Program(astIss.Ast!,
            IProgramOption.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
        var outDet = prg.Eval(unkVars);
        Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
        var residual = e.ResidualAst(astIss.Ast!, outDet.EvalDetails);
        var expr = Cel.AstToString(residual!);
        Assert.That(expr, Is.EqualTo("request.auth.claims.email == \"wiley@acme.co\""));
    }

    [Test]
    public virtual void EnvExtension()
    {
        var e = Env.NewEnv(IEnvOption.Container("google.api.expr.v1alpha1"), IEnvOption.Types(new Expr()),
            IEnvOption.Declarations(Decls.NewVar("expr", Decls.NewObjectType("google.api.expr.v1alpha1.Expr"))));
        var e2 = e.Extend(IEnvOption.CustomTypeAdapter(DefaultTypeAdapter.Instance.ToTypeAdapter()),
            IEnvOption.Types(new TestAllTypes()));
        Assert.That(e, Is.Not.EqualTo(e2));
        Assert.That(e.TypeAdapter, Is.Not.EqualTo(e2.TypeAdapter));
        Assert.That(e.TypeProvider, Is.Not.EqualTo(e2.TypeProvider));
        var e3 = e2.Extend();
        Assert.That(e2.TypeAdapter, Is.EqualTo(e3.TypeAdapter));
        // TODO fix?
        //Assert.That(e2.TypeProvider, Is.EqualTo(e3.TypeProvider));
    }

    [Test]
    public virtual void EnvExtensionIsolation()
    {
        var baseEnv = Env.NewEnv(IEnvOption.Container("google.api.expr.test.v1"),
            IEnvOption.Declarations(Decls.NewVar("age", Decls.Int), Decls.NewVar("gender", Decls.String),
                Decls.NewVar("country", Decls.String)));
        var env1 = baseEnv.Extend(IEnvOption.Types(new Google.Api.Expr.Test.V1.Proto2.TestAllTypes()),
            IEnvOption.Declarations(Decls.NewVar("name", Decls.String)));
        var env2 = baseEnv.Extend(IEnvOption.Types(new TestAllTypes()),
            IEnvOption.Declarations(Decls.NewVar("group", Decls.String)));
        var astIss = env2.Compile("size(group) > 10 && !has(proto3.TestAllTypes{}.single_int32)");
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

    [Test]
    public virtual void CustomInterpreterDecorator()
    {
        var lastInstruction = new AtomicReference<IInterpretable>();
        InterpretableDecorator optimizeArith = i =>
        {
            lastInstruction.Set(i!);
            // Only optimize the instruction if it is a call.
            if (!(i is IInterpretableCall)) return i;

            var call = (IInterpretableCall)i;
            // Only optimize the math functions when they have constant arguments.
            switch (call.Function())
            {
                case "_+_":
                case "_-_":
                case "_*_":
                case "_/_":
                    // These are all binary operators so they should have to arguments
                    var args = call.Args();
                    // When the values are constant then the call can be evaluated with
                    // an empty activation and the value returns as a constant.
                    if (!(args[0] is IInterpretableConst) ||
                        !(args[1] is IInterpretableConst))
                        return i;

                    var val = call.Eval(IActivation.EmptyActivation());
                    if (Err.IsError(val)) throw new Exception(val.ToString());

                    return IInterpretable.NewConstValue(call.Id(), val);
                default:
                    return i;
            }
        };

        var env = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("foo", Decls.Int)));
        var astIss = env.Compile("foo == -1 + 2 * 3 / 3");
        env.Program(astIss.Ast!, IProgramOption.EvalOptions(EvalOption.OptPartialEval),
            IProgramOption.CustomDecorator(optimizeArith));
        Assert.That(lastInstruction.Get(), Is.InstanceOf(typeof(IInterpretableCall)));
        var call = (IInterpretableCall)lastInstruction.Get();
        var args = call.Args();
        var lhs = args[0];
        Assert.That(lhs, Is.InstanceOf(typeof(IInterpretableAttribute)));
        var lastAttr = (IInterpretableAttribute)lhs;
        var absAttr = (INamespacedAttribute)lastAttr.Attr();
        var varNames = absAttr.CandidateVariableNames();
        Assert.That(varNames, Has.Exactly(1).EqualTo("foo"));
        var rhs = args[1];
        Assert.That(rhs, Is.InstanceOf(typeof(IInterpretableConst)));
        var lastConst = (IInterpretableConst)rhs;
        // This is the last number produced by the optimization.
        Assert.That(lastConst.Value(), Is.SameAs(IntT.IntOne));
    }

    [Test]
    public virtual void Cost()
    {
        var e = Env.NewEnv();
        var astIss = e.Compile("\"Hello, World!\"");
        Assert.That(astIss.HasIssues(), Is.False);

        var wantedCost = Interpreter.Cost.None;

        // Test standard evaluation cost.
        var prg = e.Program(astIss.Ast!);
        var c = Interpreter.Cost.EstimateCost(prg);
        Assert.That(c, Is.EqualTo(wantedCost));

        // Test the factory-based evaluation cost.
        prg = e.Program(astIss.Ast!, IProgramOption.EvalOptions(EvalOption.OptExhaustiveEval));
        c = Interpreter.Cost.EstimateCost(prg);
        Assert.That(c, Is.EqualTo(wantedCost));
    }

    [Test]
    public virtual void ResidualAstAttributeQualifiers()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Dyn)),
            Decls.NewVar("y", Decls.NewListType(Decls.Int)), Decls.NewVar("u", Decls.Int)));
        var astIss =
            e.Parse(
                "x.abc == u && x[\"abc\"] == u && x[x.string] == u && y[0] == u && y[x.zero] == u && (true ? x : y).abc == u && (false ? y : x).abc == u");
        var prg = e.Program(astIss.Ast!,
            IProgramOption.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
        var vars = Cel.PartialVars(
            TestUtil.BindingsOf("x", TestUtil.BindingsOf("zero", 0, "abc", 123, "string", "abc"),
                "y",
                new List<int> { 123 }), Cel.NewAttributePattern("u"));
        var outDet = prg.Eval(vars);
        Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
        var residual = e.ResidualAst(astIss.Ast!, outDet.EvalDetails);
        var expr = Cel.AstToString(residual!);
        Assert.That(expr,
            Is.EqualTo("123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u && 123 == u"));
    }

    [Test]
    public virtual void ResidualAstModified()
    {
        var e = Env.NewEnv(IEnvOption.Declarations(Decls.NewVar("x", Decls.NewMapType(Decls.String, Decls.Int)),
            Decls.NewVar("y", Decls.Int)));
        var astIss = e.Parse("x == y");
        var prg = e.Program(astIss.Ast!,
            IProgramOption.EvalOptions(EvalOption.OptTrackState, EvalOption.OptPartialEval));
        for (var x = 123; x < 456; x++)
        {
            var vars =
                Cel.PartialVars(TestUtil.BindingsOf("x", x), Cel.NewAttributePattern("y"));
            var outDet = prg.Eval(vars);
            Assert.That(outDet.Val, Is.Not.Null.And.Matches<object>(o => UnknownT.IsUnknown(o)));
            var residual = e.ResidualAst(astIss.Ast!, outDet.EvalDetails);
            var orig = Cel.AstToString(astIss.Ast!);
            Assert.That(orig, Is.EqualTo("x == y"));
            var expr = Cel.AstToString(residual!);
            var want = string.Format("{0:D} == y", x);
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
    //                    StringT.StringOf(String.format("Hello {0}! Nice to meet you, I'm {1}.\n", rhs,
    // lhs))));
    //    Program prg = e.program(astIss.getAst(), funcs);
    //
    //    // Evaluate the program against some inputs. Note: the details return is not used.
    //    EvalResult out =
    //        prg.eval(
    //            TestUtil.MapOf(
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
    //                    return Err.ValOrErr(lhs, "unexpected type '{0}' passed to shake_hands",
    // lhs.type());
    //                  }
    //                  if (!(rhs instanceof StringT)) {
    //                    return Err.ValOrErr(rhs, "unexpected type '{0}' passed to shake_hands",
    // rhs.type());
    //                  }
    //                  StringT s1 = (StringT) lhs;
    //                  StringT s2 = (StringT) rhs;
    //                  return StringT.StringOf(String.format("{0} and {1} are shaking hands.\n", s1, s2));
    //                }));
    //    Program prg = e.program(astIss.getAst(), funcs);
    //
    //    // Evaluate the program against some inputs. Note: the details return is not used.
    //    EvalResult out = prg.eval(TestUtil.MapOf("i", "CEL", "you", (Supplier) () -> StringT.StringOf("world")));
    //
    //    System.out.println(out);
    //    // Output:CEL and world are shaking hands.
    //  }
}