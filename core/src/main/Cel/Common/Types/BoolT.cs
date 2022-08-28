using System;

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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOfCompare;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using BoolValue = Google.Protobuf.WellKnownTypes.BoolValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
	using Type = global::Cel.Common.Types.Ref.Type;
	using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
	using Val = global::Cel.Common.Types.Ref.Val;
	using Comparer = global::Cel.Common.Types.Traits.Comparer;
	using Negater = global::Cel.Common.Types.Traits.Negater;
	using Trait = global::Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// Bool type that implements ref.Val and supports comparison and negation. </summary>
	public sealed class BoolT : BaseVal, Comparer, Negater
	{

	  /// <summary>
	  /// BoolType singleton. </summary>
	  public static readonly Type BoolType = TypeT.NewTypeValue(TypeEnum.Bool, Trait.ComparerType, Trait.NegatorType);
	  /// <summary>
	  /// Boolean constants </summary>
	  public static readonly BoolT False = new BoolT(false);

	  public static readonly BoolT True = new BoolT(true);

	  private readonly bool b;

	  internal BoolT(bool b)
	  {
		this.b = b;
	  }

	  public override bool BooleanValue()
	  {
		return b;
	  }

	  /// <summary>
	  /// Compare implements the traits.Comparer interface method. </summary>
	  public Val Compare(Val other)
	  {
		if (!(other is BoolT))
		{
		  return Err.NoSuchOverload(this, "compare", other);
		}
		return IntT.IntOfCompare(b.CompareTo(((BoolT) other).b));
	  }

	  /// <summary>
	  /// ConvertToNative implements the ref.Val interface method. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		if (typeDesc == typeof(Boolean) || typeDesc == typeof(bool) || typeDesc == typeof(object))
		{
		  return Convert.ToBoolean(b);
		}

		if (typeDesc == typeof(Any))
		{
			BoolValue value = new BoolValue();
			value.Value = b;
			return Any.Pack(value);
		}
		if (typeDesc == typeof(BoolValue))
		{
			BoolValue value = new BoolValue();
			value.Value = b;
			return value;
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(BoolT))
		{
		  return this;
		}
		if (typeDesc == typeof(Value))
		{
			Value value = new Value();
			value.BoolValue = b;
			return value;
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", BoolType, typeDesc.FullName));
	  }

	  /// <summary>
	  /// ConvertToType implements the ref.Val interface method. </summary>
	  public override Val ConvertToType(Type typeVal)
	  {
		switch (typeVal.TypeEnum().InnerEnumValue)
		{
		  case TypeEnum.InnerEnum.String:
			return StringT.StringOf(Convert.ToString(b));
		  case TypeEnum.InnerEnum.Bool:
			return this;
		  case TypeEnum.InnerEnum.Type:
			return BoolType;
		}
		return Err.NewTypeConversionError(BoolType, typeVal);
	  }

	  /// <summary>
	  /// Equal implements the ref.Val interface method. </summary>
	  public override Val Equal(Val other)
	  {
		if (!(other is BoolT))
		{
		  return Err.NoSuchOverload(this, "equal", other);
		}
		return Types.BoolOf(b == ((BoolT) other).b);
	  }

	  /// <summary>
	  /// Negate implements the traits.Negater interface method. </summary>
	  public Val Negate()
	  {
		return Types.BoolOf(!b);
	  }

	  /// <summary>
	  /// Type implements the ref.Val interface method. </summary>
	  public override Type Type()
	  {
		return BoolType;
	  }

	  /// <summary>
	  /// Value implements the ref.Val interface method. </summary>
	  public override object Value()
	  {
		return b;
	  }

	  public override bool Equals(object o)
	  {
		if (this == o)
		{
		  return true;
		}
		if (o == null || this.GetType() != o.GetType())
		{
		  return false;
		}
		BoolT boolT = (BoolT) o;
		return b == boolT.b;
	  }

	  public override int GetHashCode()
	  {
		return HashCode.Combine(base.GetHashCode(), b);
	  }
	}

}