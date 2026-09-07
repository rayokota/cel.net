/*
 * Copyright (C) 2026 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Cel.Common.Types.Ref;
using NUnit.Framework;

namespace Cel.Common.Types;

/// <summary>
///     <see cref="OpaqueT" /> carries a host value under a name the host chooses, which is what
///     makes a name collision with a built-in CEL type reachable. The carrier this generalises
///     compared type names alone and was safe only because its name was fixed.
/// </summary>
internal class OpaqueTTest
{
    private static readonly object Carried = new();

    /// <summary>
    ///     Every built-in whose name a host could plausibly pick. Answering any of these
    ///     conversions with the opaque value hands it on as a <see cref="StringT" />, a
    ///     <see cref="ListT" /> and so on, and it fails later with no connection to the cause.
    /// </summary>
    [Test]
    public virtual void ConvertingToASameNamedBuiltinIsAnError()
    {
        foreach (var (name, builtin) in new (string, IType)[]
                 {
                     ("string", StringT.StringType), ("int", IntT.IntType),
                     ("bool", BoolT.BoolType), ("double", DoubleT.DoubleType),
                     ("bytes", BytesT.BytesType), ("uint", UintT.UintType),
                     ("list", ListT.ListType), ("map", MapT.MapType),
                     ("null_type", NullT.NullType),
                 })
        {
            var result = OpaqueT.Of(Carried, name).ConvertToType(builtin);

            Assert.That(Err.IsError(result), Is.True,
                $"converting an opaque '{name}' to the builtin of the same name must be an error");
        }
    }

    [Test]
    public virtual void ConvertingToItsOwnTypeReturnsItself()
    {
        var opaque = OpaqueT.Of(Carried, "my.pkg.Money");

        Assert.That(opaque.ConvertToType(opaque.Type()), Is.SameAs(opaque));
    }

    [Test]
    public virtual void ConvertingToTypeReportsTheOpaqueType()
    {
        var opaque = OpaqueT.Of(Carried, "my.pkg.Money");

        Assert.That(opaque.ConvertToType(TypeT.TypeType), Is.EqualTo(opaque.Type()));
    }

    // Same reasoning one method over: equality is by the whole type, not the name.
    [Test]
    public virtual void EqualityRequiresTheSameTypeName()
    {
        var a = OpaqueT.Of("shared", "my.pkg.A");
        var sameValueOtherType = OpaqueT.Of("shared", "my.pkg.B");

        Assert.That(a.Equal(OpaqueT.Of("shared", "my.pkg.A")), Is.EqualTo(BoolT.True));
        Assert.That(a.Equal(sameValueOtherType), Is.EqualTo(BoolT.False));
        Assert.That(a.Equals(OpaqueT.Of("shared", "my.pkg.A")), Is.True);
        Assert.That(a.Equals(sameValueOtherType), Is.False);
    }

    [Test]
    public virtual void ValueAndNativeConversionReturnTheCarriedObject()
    {
        var carried = new List<string> { "a" };
        var opaque = OpaqueT.Of(carried, "my.pkg.Names");

        Assert.That(opaque.Value(), Is.SameAs(carried));
        Assert.That(opaque.ConvertToNative(typeof(object)), Is.SameAs(carried));
        Assert.That(opaque.ConvertToNative(typeof(List<string>)), Is.SameAs(carried));
    }

    /// <summary>
    ///     A rejected argument has to say which one and why. ArgumentNullException's
    ///     single-argument overload takes a paramName and ArgumentException's takes a message, so
    ///     the two guards read as parallel and are not.
    /// </summary>
    [Test]
    public virtual void RejectedArgumentsNameThemselves()
    {
        var nullValue = Assert.Throws<ArgumentNullException>(() => OpaqueT.Of(null!, "my.pkg.A"))!;
        Assert.That(nullValue.ParamName, Is.EqualTo("value"));

        foreach (var bad in new[] { "", null })
        {
            var emptyName = Assert.Throws<ArgumentException>(() => OpaqueT.Of(Carried, bad!))!;
            Assert.That(emptyName.ParamName, Is.EqualTo("typeName"));
            Assert.That(emptyName.Message, Does.Contain("must not be null or empty"));
        }
    }
}
