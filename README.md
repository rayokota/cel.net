# C# implementation of Common-Expression-Language (CEL)

[![CI](https://github.com/projectnessie/cel-java/actions/workflows/main.yml/badge.svg)](https://github.com/projectnessie/cel-java/actions/workflows/main.yml)

This is a C# port of the [Common-Expression-Language (CEL)](https://opensource.google/projects/cel).

The CEL specification can be found [here](https://github.com/google/cel-spec).

## Getting started

A very simple start:

```csharp
using Cel.Checker;
using Cel.Tools;

public class MyClass
{
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

        String result = script.Execute<string>(arguments);

        Console.WriteLine(result); // Prints "hello world"
    }
}
```

## Protobuf and Json.NET and plain C# objects

Protobuf (via `Google.Protobuf`) objects and schema is supported out of the box.

It is also possible to use plain C# and Json.NET objects as arguments by using the 
`Cel.Common.Types.Json.JsonRegistry`.

Code sample similar to the one above. It takes a user-provided object type `MyInput`.

```csharp
using Cel.Checker;
using Cel.Tools;

public class MyClass
{
    public bool EvalWithJsonObject(MyInput input, string checkName)
    {
        // Build the script factory
        ScriptHost scriptHost = ScriptHost.NewBuilder()
            .Registry(JsonRegistry.NewRegistry())
            .Build();

        // Create the script, will be parsed and checked.
        // It checks whether the property `Name` in the "Json-ized" class `MyInput` is
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

}
```

Note that the Json.NET field-names are used as property names in Cel-CSharp. It is not necessary to
annotate "plain C#" classes with Json.NET attributes.


