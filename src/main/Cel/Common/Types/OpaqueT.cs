using Cel.Common.Types.Ref;

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

namespace Cel.Common.Types;

/// <summary>
///     Carries an arbitrary CLR value through CEL unchanged, under a caller-chosen type name.
///
///     A host application often has a domain type CEL knows nothing about - a decimal, a
///     variant, a money amount - that its own functions understand but the runtime must not
///     try to interpret. Without a carrier such a value reaches the object fallback, which
///     needs a schema for it and throws when there is none.
///
///     <see cref="Value" /> returns the original object, so a custom function can take it
///     straight back out. Equality is the CLR value's own, and only between carriers reporting
///     the same type name.
///     <para>
///         This is cel-java's <c>OpaqueValue.create(name, value)</c> and cel-rust's
///         <c>Value::Opaque</c>, whose <c>opaque_eq</c> likewise answers false for a different
///         runtime type. It is <b>not</b> cel-go's <c>NewOpaqueType</c>: that is a type-level
///         construct (an abstract parameterized type, as used by <c>optional_type</c>), and
///         cel-go has no generic value carrier at all — a host there implements <c>ref.Val</c>
///         itself.
///     </para>
///     <para>
///         One divergence from cel-java, and it is forced: its <c>celType()</c> returns an
///         <c>OpaqueType</c>, while <see cref="Type" /> here returns an *object* type, because
///         this runtime's <see cref="Ref.TypeEnum" /> has no abstract or opaque kind. The name
///         describes the role, not a type kind cel.net can express.
///     </para>
/// </summary>
public sealed class OpaqueT : BaseVal
{
    private readonly object value;
    private readonly IType type;

    private OpaqueT(object value, IType type)
    {
        this.value = value;
        this.type = type;
    }

    /// <summary>
    ///     Carries <paramref name="value" /> under <paramref name="typeName" />, which names the
    ///     runtime type a rule sees (and what <c>type(x)</c> reports).
    /// </summary>
    public static OpaqueT Of(object value, string typeName)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(typeName)) throw new ArgumentException(nameof(typeName));
        return new OpaqueT(value, TypeT.NewObjectTypeValue(typeName));
    }

    public override object Value()
    {
        return value;
    }

    public override IType Type()
    {
        return type;
    }

    public override IVal Equal(IVal other)
    {
        return other is OpaqueT o && type.TypeName().Equals(o.type.TypeName())
            ? Types.BoolOf(value.Equals(o.value))
            : BoolT.False;
    }

    public override IVal ConvertToType(IType typeValue)
    {
        if (typeValue.TypeEnum().InnerEnumValue == TypeEnum.InnerEnum.Type) return type;

        if (typeValue.TypeName().Equals(type.TypeName())) return this;

        return Err.NewTypeConversionError(type, typeValue);
    }

    public override object ConvertToNative(System.Type typeDesc)
    {
        if (typeDesc == typeof(object) || typeDesc.IsInstanceOfType(value)) return value;

        return Err.NewTypeConversionError(type, typeDesc.Name);
    }

    public override bool Equals(object? o)
    {
        return o is OpaqueT other && type.TypeName().Equals(other.type.TypeName())
                                  && value.Equals(other.value);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(type.TypeName(), value);
    }

    public override string ToString()
    {
        return type.TypeName() + "(" + value + ")";
    }
}
