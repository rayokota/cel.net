using System;
using System.Numerics;

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
//	import static Cel.Common.Types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.divideByZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.errUintOverflow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.modulusByZero;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.rangeError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.maxIntJSON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using UInt32Value = Google.Protobuf.WellKnownTypes.UInt32Value;
	using UInt64Value = Google.Protobuf.WellKnownTypes.UInt64Value;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using ULong = org.projectnessie.cel.common.ULong;
	using OverflowException = Cel.Common.Types.Overflow.OverflowException;
	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
	using Val = Cel.Common.Types.Ref.Val;
	using Adder = Cel.Common.Types.Traits.Adder;
	using Comparer = Cel.Common.Types.Traits.Comparer;
	using Divider = Cel.Common.Types.Traits.Divider;
	using Modder = Cel.Common.Types.Traits.Modder;
	using Multiplier = Cel.Common.Types.Traits.Multiplier;
	using Subtractor = Cel.Common.Types.Traits.Subtractor;
	using Trait = Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// Uint type implementation which supports comparison and math operators. </summary>
	public sealed class UintT : BaseVal, Adder, Comparer, Divider, Modder, Multiplier, Subtractor
	{

	  /// <summary>
	  /// UintType singleton. </summary>
	  public static readonly Type UintType = TypeT.NewTypeValue(TypeEnum.Uint, Trait.AdderType, Trait.ComparerType, Trait.DividerType, Trait.ModderType, Trait.MultiplierType, Trait.SubtractorType);

	  /// <summary>
	  /// Uint constants </summary>
	  public static readonly UintT UintZero = new UintT(0);

	  public static UintT UintOf(ULong i)
	  {
		return UintOf(i.LongValue());
	  }

	  public static UintT UintOf(long i)
	  {
		if (i == 0L)
		{
		  return UintZero;
		}
		return new UintT(i);
	  }

	  private readonly long i;

	  private UintT(long i)
	  {
		this.i = i;
	  }

	  public override long IntValue()
	  {
		return i;
	  }

	  /// <summary>
	  /// Add implements traits.Adder.Add. </summary>
	  public Val Add(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "add", other);
		}
		try
		{
		  return UintOf(Overflow.AddUint64Checked(i, ((UintT) other).i));
		}
		catch (OverflowException)
		{
		  return errUintOverflow;
		}
	  }

	  /// <summary>
	  /// Compare implements traits.Comparer.Compare. </summary>
	  public Val Compare(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "compare", other);
		}
		return intOf(Long.compareUnsigned(i, ((UintT) other).i));
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
	  public override T ConvertToNative<T>(System.Type typeDesc)
	  {
		if (typeDesc == typeof(Long) || typeDesc == typeof(long) || typeDesc == typeof(object))
		{
		  if (i < 0)
		  {
			Err.ThrowErrorAsIllegalStateException(rangeError(i, "Java long"));
		  }
		  return (T) Convert.ToInt64(i);
		}
		if (typeDesc == typeof(Integer) || typeDesc == typeof(int))
		{
		  if (i < 0 || i > int.MaxValue)
		  {
			Err.ThrowErrorAsIllegalStateException(rangeError(i, "Java int"));
		  }
		  return (T) Convert.ToInt32((int) i);
		}
		if (typeDesc == typeof(ULong))
		{
		  return (T) ULong.ValueOf(i);
		}
		if (typeDesc == typeof(Any))
		{
		  return (T) Any.pack(UInt64Value.of(i));
		}
		if (typeDesc == typeof(UInt64Value))
		{
		  return (T) UInt64Value.of(i);
		}
		if (typeDesc == typeof(UInt32Value))
		{
		  return (T) UInt32Value.of((int) i);
		}
		if (typeDesc == typeof(Val) || typeDesc == typeof(UintT))
		{
		  return (T) this;
		}
		if (typeDesc == typeof(Value))
		{
		  if (i <= maxIntJSON)
		  {
			// JSON can accurately represent 32-bit uints as floating point values.
			return (T) Value.newBuilder().setNumberValue(i).build();
		  }
		  else
		  {
			// Proto3 to JSON conversion requires string-formatted uint64 values
			// since the conversion to floating point would result in truncation.
			return (T) Value.newBuilder().setStringValue(Long.toUnsignedString(i)).build();
		  }
		}

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", UintType, typeDesc.FullName));
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
	  public override Val ConvertToType(Type typeValue)
	  {
		switch (typeValue.TypeEnum().innerEnumValue)
		{
		  case TypeEnum.InnerEnum.Int:
			if (i < 0L)
			{
			  return rangeError(Long.toUnsignedString(i), "int");
			}
			return intOf(i);
		  case TypeEnum.InnerEnum.Uint:
			return this;
		  case TypeEnum.InnerEnum.double:
			if (i < 0L)
			{
			  return doubleOf((new BigInteger(Long.toUnsignedString(i))).doubleValue());
			}
			return doubleOf(i);
		  case TypeEnum.InnerEnum.String:
			return stringOf(Long.toUnsignedString(i));
		  case Type:
			return UintType;
		}
		return newTypeConversionError(UintType, typeValue);
	  }

	  /// <summary>
	  /// Divide implements traits.Divider.Divide. </summary>
	  public Val Divide(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "divide", other);
		}
		long otherInt = ((UintT) other).i;
		if (otherInt == 0L)
		{
		  return divideByZero();
		}
		return UintOf(i / otherInt);
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "equal", other);
		}
		return boolOf(i == ((UintT) other).i);
	  }

	  /// <summary>
	  /// Modulo implements traits.Modder.Modulo. </summary>
	  public Val Modulo(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "modulo", other);
		}
		long otherInt = ((UintT) other).i;
		if (otherInt == 0L)
		{
		  return modulusByZero();
		}
		return UintOf(i % otherInt);
	  }

	  /// <summary>
	  /// Multiply implements traits.Multiplier.Multiply. </summary>
	  public Val Multiply(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "multiply", other);
		}
		try
		{
		  return UintOf(Overflow.MultiplyUint64Checked(i, ((UintT) other).i));
		}
		catch (OverflowException)
		{
		  return errUintOverflow;
		}
	  }

	  /// <summary>
	  /// Subtract implements traits.Subtractor.Subtract. </summary>
	  public Val Subtract(Val other)
	  {
		if (other.Type() != UintType)
		{
		  return noSuchOverload(this, "subtract", other);
		}
		try
		{
		  return UintOf(Overflow.SubtractUint64Checked(i, ((UintT) other).i));
		}
		catch (OverflowException)
		{
		  return errUintOverflow;
		}
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public override Type Type()
	  {
		return UintType;
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public override object Value()
	  {
		return i;
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
		UintT uintT = (UintT) o;
		return i == uintT.i;
	  }

	  public override int GetHashCode()
	  {
		return Objects.hash(base.GetHashCode(), i);
	  }

	  /// <summary>
	  /// isJSONSafe indicates whether the uint is safely representable as a floating point value in
	  /// JSON.
	  /// </summary>
	  public bool JSONSafe
	  {
		  get
		  {
			return i >= 0 && i <= IntT.maxIntJSON;
		  }
	  }
	}

}