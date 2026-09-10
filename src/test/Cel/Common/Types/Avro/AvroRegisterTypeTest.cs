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

using Avro;
using Cel.Checker;
using Cel.Common.Types;
using Cel.Common.Types.Avro;
using Cel.Common.Types.Ref;
using JsonRegistry = Cel.Common.Types.Json.JsonRegistry;
using Cel.Tools;
using NUnit.Framework;

namespace Cel.Types.Avro;

/// <summary>
///     RegisterType on the Avro registry, which used to throw NotSupportedException.
///     <para>
///         A caller that owns the CEL representation of an Avro value - the customAdapter on
///         NewRegistry - gives it a type name that no Avro schema declares, so nothing in the
///         registry's schema-derived tables can resolve it. Without RegisterType that name was
///         unusable in an expression here while ProtoTypeRegistry accepted it, so the same rule
///         compiled against a protobuf message and failed the check against an Avro value.
///     </para>
/// </summary>
internal class AvroRegisterTypeTest
{
    private const string CarriedTypeName = "example.Carried";

    /// <summary>A caller-owned CEL value, standing in for a decimal or any other carried type.</summary>
    private static IVal Carry(object value)
    {
        return value is AvroDecimal dec ? OpaqueT.Of(dec, CarriedTypeName) : null;
    }

    private static Script Build(bool registerType, string expr)
    {
        ITypeRegistry registry = AvroRegistry.NewRegistry(Carry);
        if (registerType)
        {
            registry.RegisterType(TypeT.NewObjectTypeValue(CarriedTypeName));
        }

        return ScriptHost.NewBuilder().Registry(registry).Build()
            .BuildScript(expr)
            .WithDeclarations(Decls.NewVar("x", Decls.NewObjectType(CarriedTypeName)))
            .Build();
    }

    /// <summary>The name is usable in an expression once registered.</summary>
    [Test]
    public virtual void ARegisteredTypeNameResolves()
    {
        Script script = Build(true, "type(x) == " + CarriedTypeName);
        Assert.That(
            script.Execute<bool>(new Dictionary<string, object> { ["x"] = new AvroDecimal(1234, 2) }),
            Is.True);
    }

    /// <summary>...and is not, without it. This is what threw NotSupportedException before.</summary>
    [Test]
    public virtual void AnUnregisteredTypeNameDoesNot()
    {
        ScriptCreateException e = Assert.Throws<ScriptCreateException>(
            () => Build(false, "type(x) == " + CarriedTypeName));
        Assert.That(e!.ToString(), Does.Contain("undeclared reference"));
    }

    /// <summary>
    ///     The JSON registry needs the same thing, and for a sharper reason: it is the registry
    ///     selected for a value that is neither a record nor a message, so a caller-owned type
    ///     bound on its own lands here rather than on the Avro or protobuf registry.
    /// </summary>
    [Test]
    public virtual void TheJsonRegistryRegistersTypesToo()
    {
        ITypeRegistry registry = JsonRegistry.NewRegistry();
        registry.RegisterType(TypeT.NewObjectTypeValue(CarriedTypeName));
        Assert.That(registry.FindIdent(CarriedTypeName), Is.Not.Null);
        Assert.That(registry.FindType(CarriedTypeName), Is.Not.Null);
    }

    /// <summary>...and refused it before, as the Avro registry did.</summary>
    [Test]
    public virtual void TheJsonRegistryResolvesNothingUnregistered()
    {
        ITypeRegistry registry = JsonRegistry.NewRegistry();
        Assert.That(registry.FindIdent(CarriedTypeName), Is.Null);
        Assert.That(registry.FindType(CarriedTypeName), Is.Null);
    }

    /// <summary>
    ///     Copy() isolates mutable state, as ITypeRegistry.Copy promises and Env.Extend relies
    ///     on. It returned <c>this</c> before, so registering anything in a derived environment
    ///     mutated the parent and its siblings — the registered-type map added here and the
    ///     schema-derived tables that predate it alike.
    /// </summary>
    [Test]
    public virtual void CopyIsolatesRegisteredTypes()
    {
        ITypeRegistry parent = AvroRegistry.NewRegistry(Carry);
        ITypeRegistry child = parent.Copy();

        Assert.That(ReferenceEquals(parent, child), Is.False);

        child.RegisterType(TypeT.NewObjectTypeValue(CarriedTypeName));
        Assert.That(child.FindIdent(CarriedTypeName), Is.Not.Null);
        Assert.That(parent.FindIdent(CarriedTypeName), Is.Null, "leaked to the parent");

        // The tables that predate RegisterType leak the same way if Copy() shares them.
        ITypeRegistry other = AvroRegistry.NewRegistry(Carry);
        ITypeRegistry otherChild = other.Copy();
        otherChild.Register(Example.Avro.User._SCHEMA);
        Assert.That(otherChild.FindType(Example.Avro.User._SCHEMA.Fullname), Is.Not.Null);
        Assert.That(other.FindType(Example.Avro.User._SCHEMA.Fullname), Is.Null,
            "schema registration leaked to the parent");
    }

    /// <summary>The custom adapter survives the copy, or it silently stops applying.</summary>
    [Test]
    public virtual void CopyKeepsTheCustomAdapter()
    {
        ITypeRegistry child = AvroRegistry.NewRegistry(Carry).Copy();
        Assert.That(child.NativeToValue(new AvroDecimal(1234, 2)).Type().TypeName(),
            Is.EqualTo(CarriedTypeName));
    }

    /// <summary>The JSON registry is the same contract.</summary>
    [Test]
    public virtual void TheJsonRegistryCopyIsolatesToo()
    {
        ITypeRegistry parent = JsonRegistry.NewRegistry();
        ITypeRegistry child = parent.Copy();

        Assert.That(ReferenceEquals(parent, child), Is.False);
        child.RegisterType(TypeT.NewObjectTypeValue(CarriedTypeName));
        Assert.That(child.FindIdent(CarriedTypeName), Is.Not.Null);
        Assert.That(parent.FindIdent(CarriedTypeName), Is.Null, "leaked to the parent");
    }

    /// <summary>
    ///     Registering a name does not disturb the schema-derived resolution around it: the
    ///     built-in type names still resolve through the same FindIdent.
    /// </summary>
    [Test]
    public virtual void TheBuiltInNamesStillResolve()
    {
        Script script = Build(true, "type(x) == " + CarriedTypeName + " && type('s') == string");
        Assert.That(
            script.Execute<bool>(new Dictionary<string, object> { ["x"] = new AvroDecimal(1234, 2) }),
            Is.True);
    }
}
