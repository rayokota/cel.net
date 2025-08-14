using Cel.Checker;
using Cel.Common.Types;
using Cel.Interpreter.Functions;
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
namespace Cel.Tools;

internal class ScriptHostTest
{
    [Test]
    public virtual void Basic()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        // create the script, will be parsed and checked
        var script = scriptHost.BuildScript("x + ' ' + y")
            .WithDeclarations(Decls.NewVar("x", Decls.String), Decls.NewVar("y", Decls.String)).Build();

        IDictionary<string, object> arguments = new Dictionary<string, object>();
        arguments["x"] = "hello";
        arguments["y"] = "world";

        var result = script.Execute<string>(arguments);

        Assert.That(result, Is.EqualTo("hello world"));
    }

    [Test]
    public virtual void Function()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        // create the script, will be parsed and checked
        var script =
            scriptHost
                .BuildScript("x + ' ' + y")
                // Variable declarations - we need `x` and `y` in this example
                .WithDeclarations(Decls.NewVar("x", Decls.String), Decls.NewVar("y", Decls.String))
                .Build();

        var result =
            script.Execute<string>(
                arg =>
                {
                    if ("x".Equals(arg))
                        return "hello";
                    if ("y".Equals(arg))
                        return "world";
                    return null;
                });

        Assert.That(result, Is.EqualTo("hello world"));
    }

    [Test]
    public virtual void ExecFail()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        // create the script, will be parsed and checked
        var script = scriptHost.BuildScript("1/0 != 0").Build();

        Assert.That(() => script.Execute<string>(new Dictionary<string, object> { { "x", "hello world" } }),
            Throws.Exception.InstanceOf(typeof(ScriptExecutionException)));
    }

    [Test]
    public virtual void BadSyntax()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        Assert.That(() => scriptHost.BuildScript("-.,").Build(),
            Throws.Exception.InstanceOf(typeof(ScriptCreateException)));
    }

    [Test]
    public virtual void CheckFailure()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        Assert.That(() => scriptHost.BuildScript("x").Build(),
            Throws.Exception.InstanceOf(typeof(ScriptCreateException)));
    }

    [Test]
    public virtual void Library()
    {
        var scriptHost = ScriptHost.NewBuilder().Build();

        var script = scriptHost.BuildScript("foo()").WithLibraries(new MyLib()).Build();

        Assert.That(script.Execute<int>(new Dictionary<string, object>()), Is.EqualTo(42));
    }
}

internal class MyLib : ILibrary
{
    /// <summary>
    ///     EnvOptions returns options for the standard CEL function declarations and macros.
    /// </summary>
    public IList<EnvOption> CompileOptions =>
        new List<EnvOption>
        {
            EnvOptions.Declarations(Decls.NewFunction("foo",
                Decls.NewOverload("foo_void", new List<Type>(), Decls.Int)))
        };

    /// <summary>
    ///     ProgramOptions returns function implementations for the standard CEL functions.
    /// </summary>
    public IList<ProgramOption> ProgramOptions =>
        new List<ProgramOption> { global::Cel.ProgramOptions.Functions(Overload.Function("foo", e => IntT.IntOf(42))) };
}