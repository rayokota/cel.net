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
//	import static Cel.Common.Types.UnknownT.UnknownType;

	using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
	using Type = global::Cel.Common.Types.Ref.Type;
	using TypeEnum = global::Cel.Common.Types.Ref.TypeEnum;
	using Val = global::Cel.Common.Types.Ref.Val;

	/// <summary>
	/// Err type which extends the built-in go error and implements ref.Val. </summary>
	public sealed class Err : BaseVal
	{

	  /// <summary>
	  /// ErrType singleton. </summary>
	  public static readonly Type ErrType = TypeT.NewTypeValue(TypeEnum.Err);

	  /// <summary>
	  /// errIntOverflow is an error representing integer overflow. </summary>
	  public static readonly Val ErrIntOverflow = NewErr("integer overflow");
	  /// <summary>
	  /// errUintOverflow is an error representing unsigned integer overflow. </summary>
	  public static readonly Val ErrUintOverflow = NewErr("unsigned integer overflow");
	  /// <summary>
	  /// errDurationOverflow is an error representing duration overflow. </summary>
	  public static readonly Val ErrDurationOverflow = NewErr("duration overflow");
	  /// <summary>
	  /// errDurationOutOfRange is an error representing duration out of range. </summary>
	  public static readonly Val ErrDurationOutOfRange = NewErr("duration out of range");
	  /// <summary>
	  /// errTimestampOverflow is an error representing timestamp overflow. </summary>
	  public static readonly Val ErrTimestampOverflow = NewErr("timestamp overflow");
	  /// <summary>
	  /// errTimestampOutOfRange is an error representing duration out of range. </summary>
	  public static readonly Val ErrTimestampOutOfRange = NewErr("timestamp out of range");

	  private readonly string error;
	  private readonly Exception cause;

	  private Err(string error) : this(error, null)
	  {
	  }

	  private Err(string error, Exception cause)
	  {
		this.error = error;
		this.cause = cause;
	  }

	  public static Val NoSuchOverload(Val val, string function, Val other)
	  {
		string otName = (other != null) ? ((other is Type) ? (Type) other : other.Type()).TypeName() : "*";
		if (val != null)
		{
		  Type vt = (val is Type) ? (Type) val : val.Type();
		  return ValOrErr(other, "no such overload: %s.%s(%s)", vt.TypeName(), function, otName);
		}
		else
		{
		  return ValOrErr(other, "no such overload: *.%s(%s)", function, otName);
		}
	  }

	  public static Val NoSuchOverload(Val val, string function, Type argA, Type argB)
	  {
		return NewErr("no such overload: %s.%s(%s,%s,...)", val.Type().TypeName(), function, argA, argB);
	  }

	  public static Val NoSuchOverload(Val val, string function, string overload, Val[] args)
	  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
		  return NewErr("no such overload: %s.%s[%s](%s)", val.Type().TypeName(), function, overload,
			  string.Join(", ", args.Select(a => a.Type().TypeName())));
	  }

	  /// <summary>
	  /// MaybeNoSuchOverloadErr returns the error or unknown if the input ref.Val is one of these types,
	  /// else a new no such overload error.
	  /// </summary>
	  public static Val MaybeNoSuchOverloadErr(Val val)
	  {
		return ValOrErr(val, "no such overload");
	  }

	  /// <summary>
	  /// NewErr creates a new Err described by the format string and args. TODO: Audit the use of this
	  /// function and standardize the error messages and codes.
	  /// </summary>
	  public static Val NewErr(string format, params object[] args)
	  {
		return new Err(String.Format(format, args));
	  }

	  /// <summary>
	  /// NewErr creates a new Err described by the format string and args. TODO: Audit the use of this
	  /// function and standardize the error messages and codes.
	  /// </summary>
	  public static Val NewErr(Exception cause, string format, params object[] args)
	  {
		if (cause is ErrException)
		{
		  return ((ErrException) cause).Err;
		}
		return new Err(String.Format(format, args), cause);
	  }

	  /// <summary>
	  /// UnsupportedRefValConversionErr returns a types.NewErr instance with a no such conversion
	  /// message that indicates that the native value could not be converted to a CEL ref.Val.
	  /// </summary>
	  public static Val UnsupportedRefValConversionErr(object val)
	  {
		return NewErr("unsupported conversion to ref.Val: (%s)%s", val.GetType().Name, val);
	  }

	  /// <summary>
	  /// ValOrErr either returns the existing error or create a new one. TODO: Audit the use of this
	  /// function and standardize the error messages and codes.
	  /// </summary>
	  public static Val ValOrErr(Val val, string format, params object[] args)
	  {
		if (val == null)
		{
		  return NewErr(format, args);
		}
		if (val.Type() == ErrType || val.Type() == UnknownT.UnknownType)
		{
		  return val;
		}
		return NewErr(format, args);
	  }

	  public static Val NoSuchField(object field)
	  {
		return NewErr("no such field '%s'", field);
	  }

	  public static Val UnknownType(object field)
	  {
		return NewErr("unknown type '%s'", field);
	  }

	  public static Val AnyWithEmptyType()
	  {
		return NewErr("conversion error: got Any with empty type-url");
	  }

	  public static Val DivideByZero()
	  {
		return NewErr("divide by zero");
	  }

	  public static Val NoMoreElements()
	  {
		return NewErr("no more elements");
	  }

	  public static Val ModulusByZero()
	  {
		return NewErr("modulus by zero");
	  }

	  public static Val RangeError(object from, object to)
	  {
		return NewErr("range error converting %s to %s", from, to);
	  }

	  public static Val NewTypeConversionError(object from, object to)
	  {
		return NewErr("type conversion error from '%s' to '%s'", from, to);
	  }

	  public static Exception NoSuchAttributeException(object context)
	  {
		return new ErrException("undeclared reference to '%s' (in container '')", context);
	  }

	  public static Val NoSuchKey(object key)
	  {
		return NewErr("no such key: %s", key);
	  }

	  public static Exception NoSuchKeyException(object key)
	  {
		return new ErrException("no such key: %s", key);
	  }

	  public static Exception IndexOutOfBoundsException(object i)
	  {
		return new System.InvalidOperationException(String.Format("index out of bounds: %s", i));
	  }

	  public sealed class ErrException : System.ArgumentException
	  {
		internal readonly string format;
		internal readonly object[] args;

		public ErrException(string format, params object[] args) : base(String.Format(format, args))
		{
		  this.format = format;
		  this.args = args;
		}

		public Val Err
		{
			get
			{
			  return NewErr(format, args);
			}
		}
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
	  public override object? ConvertToNative(System.Type typeDesc)
	  {
		throw new System.NotSupportedException(error);
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
	  public override Val ConvertToType(Type typeVal)
	  {
		// Errors are not convertible to other representations.
		return this;
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public override Val Equal(Val other)
	  {
		// An error cannot be equal to any other value, so it returns itself.
		return this;
	  }

	  /// <summary>
	  /// String implements fmt.Stringer. </summary>
	  public override string ToString()
	  {
		return error;
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public override Type Type()
	  {
		return ErrType;
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public override object Value()
	  {
		return error;
	  }

	  public override bool BooleanValue()
	  {
		throw new System.NotSupportedException();
	  }

	  public override long IntValue()
	  {
		throw new System.NotSupportedException();
	  }

	  /// <summary>
	  /// IsError returns whether the input element ref.Type or ref.Val is equal to the ErrType
	  /// singleton.
	  /// </summary>
	  public static bool IsError(Val val)
	  {
		return val != null && val.Type() == ErrType;
	  }

	  public bool HasCause()
	  {
		return cause != null;
	  }

	  public Exception Cause
	  {
		  get
		  {
			return cause;
		  }
	  }

	  public Exception ToRuntimeException()
	  {
		if (cause != null)
		{
			throw new Exception(this.error, this.cause);
		}
		throw new Exception(this.error);
	  }

	  public static void ThrowErrorAsIllegalStateException(Val val)
	  {
		if (val is Err)
		{
		  Err e = (Err) val;
		  if (e.cause != null)
		  {
			throw new System.InvalidOperationException(e.error, e.cause);
		  }
		  else
		  {
			throw new System.InvalidOperationException(e.error);
		  }
		}
	  }
	}

}