using Cel.Checker;
using Cel.Common.Types.Json;
using NUnit.Framework;

namespace Cel.Tools;

public class MyClass
{
    [Test]
    public void MyScriptUsage()
    {
        // Build the script factory
        ScriptHost scriptHost = ScriptHost.NewBuilder().Build();

        // create the script, will be parsed and checked
        Script script = scriptHost.BuildScript("x + ' ' + y")
            .WithDeclarations(
                // Variable declarations - we need `x` and `y` in this example
                Decls.NewVar("x", Decls.String),
                Decls.NewVar("y", Decls.String))
            .Build();

        IDictionary<string, object> arguments = new Dictionary<string, object>();
        arguments.Add("x", "hello");
        arguments.Add("y", "world");

        string result = script.Execute<string>(arguments);

        Console.WriteLine(result); // Prints "hello world"
    }

    [Test]
    public void MyJsonScriptUsage()
    {
        MyInput input = new MyInput(123, "John");
        Assert.That(EvalWithJsonObject(input, "John"), Is.True);
    }

    public bool EvalWithJsonObject(MyInput input, string checkName)
    {
        // Build the script factory
        ScriptHost scriptHost = ScriptHost.NewBuilder()
            .Registry(JsonRegistry.NewRegistry())
            .Build();

        // Create the script, will be parsed and checked.
        // It checks whether the property `name` in the "Json-ized" class `MyInput` is
        // equal to the value of `checkName`.
        Script script = scriptHost.BuildScript("inp.Name == checkName")
            // Variable declarations - we need `inp` +  `checkName` in this example
            .WithDeclarations(
                Decls.NewVar("inp", Decls.NewObjectType(typeof(MyInput).FullName)),
                Decls.NewVar("checkName", Decls.String))
            // Register our Json-NET object input type
            .WithTypes(typeof(MyInput))
            .Build();

        IDictionary<string, object> arguments = new Dictionary<string, object>();
        arguments.Add("inp", input);
        arguments.Add("checkName", checkName);

        bool result = script.Execute<bool>(arguments);

        return result;
    }

    public class MyInput
    {

        public MyInput(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}