using Cel.Common.Types.Ref;

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
namespace Cel.Common.Types;

/// <summary>
///     Err type which extends the built-in go error and implements ref.Val.
/// </summary>
public sealed class Err : BaseVal
{
    /// <summary>
    ///     ErrType singleton.
    /// </summary>
    public static readonly IType ErrType = TypeT.NewTypeValue(TypeEnum.Err);

    /// <summary>
    ///     errIntOverflow is an error representing integer overflow.
    /// </summary>
    public static readonly IVal ErrIntOverflow = NewErr("integer overflow");

    /// <summary>
    ///     errUintOverflow is an error representing unsigned integer overflow.
    /// </summary>
    public static readonly IVal ErrUintOverflow = NewErr("unsigned integer overflow");

    /// <summary>
    ///     errDurationOverflow is an error representing duration overflow.
    /// </summary>
    public static readonly IVal ErrDurationOverflow = NewErr("duration overflow");

    /// <summary>
    ///     errDurationOutOfRange is an error representing duration out of range.
    /// </summary>
    public static readonly IVal ErrDurationOutOfRange = NewErr("duration out of range");

    /// <summary>
    ///     errTimestampOverflow is an error representing timestamp overflow.
    /// </summary>
    public static readonly IVal ErrTimestampOverflow = NewErr("timestamp overflow");

    /// <summary>
    ///     errTimestampOutOfRange is an error representing duration out of range.
    /// </summary>
    public static readonly IVal ErrTimestampOutOfRange = NewErr("timestamp out of range");

    private readonly string error;

    private Err(string error) : this(error, null)
    {
    }

    private Err(string error, Exception? cause)
    {
        this.error = error;
        Cause = cause;
    }

    public Exception? Cause { get; }

    public static IVal NoSuchOverload(IVal? val, string function, IVal other)
    {
        var otName = other != null ? (other is IType ? (IType)other : other.Type()).TypeName() : "*";
        if (val != null)
        {
            var vt = val is IType ? (IType)val : val.Type();
            return ValOrErr(other, "no such overload: {0}.{1}({2})", vt.TypeName(), function, otName);
        }

        return ValOrErr(other, "no such overload: *.{0}({1})", function, otName);
    }

    public static IVal NoSuchOverload(IVal val, string function, IType argA, IType argB)
    {
        return NewErr("no such overload: {0}.{1}({2},{3},...)", val.Type().TypeName(), function, argA, argB);
    }

    public static IVal NoSuchOverload(IVal val, string function, string overload, IVal[] args)
    {
        return NewErr("no such overload: {0}.{1}[{2}]({3})", val.Type().TypeName(), function, overload,
            string.Join(", ", args.Select(a => a.Type().TypeName())));
    }

    /// <summary>
    ///     MaybeNoSuchOverloadErr returns the error or unknown if the input ref.Val is one of these types,
    ///     else a new no such overload error.
    /// </summary>
    public static IVal MaybeNoSuchOverloadErr(IVal val)
    {
        return ValOrErr(val, "no such overload");
    }

    /// <summary>
    ///     NewErr creates a new Err described by the format string and args. TODO: Audit the use of this
    ///     function and standardize the error messages and codes.
    /// </summary>
    public static IVal NewErr(string format, params object[] args)
    {
        return new Err(string.Format(format, args));
    }

    /// <summary>
    ///     NewErr creates a new Err described by the format string and args. TODO: Audit the use of this
    ///     function and standardize the error messages and codes.
    /// </summary>
    public static IVal NewErr(Exception cause, string format, params object[] args)
    {
        if (cause is ErrException) return ((ErrException)cause).Err;

        return new Err(string.Format(format, args), cause);
    }

    /// <summary>
    ///     UnsupportedRefValConversionErr returns a types.NewErr instance with a no such conversion
    ///     message that indicates that the native value could not be converted to a CEL ref.Val.
    /// </summary>
    public static IVal UnsupportedRefValConversionErr(object val)
    {
        return NewErr("unsupported conversion to ref.Val: ({0}){1}", val.GetType().FullName, val);
    }

    /// <summary>
    ///     ValOrErr either returns the existing error or create a new one. TODO: Audit the use of this
    ///     function and standardize the error messages and codes.
    /// </summary>
    public static IVal ValOrErr(IVal val, string format, params object[] args)
    {
        if (val == null) return NewErr(format, args);

        if (val.Type() == ErrType || val.Type() == UnknownT.UnknownType) return val;

        return NewErr(format, args);
    }

    public static IVal NoSuchField(object field)
    {
        return NewErr("no such field '{0}'", field);
    }

    public static IVal UnknownType(object field)
    {
        return NewErr("unknown type '{0}'", field);
    }

    public static IVal AnyWithEmptyType()
    {
        return NewErr("conversion error: got Any with empty type-url");
    }

    public static IVal DivideByZero()
    {
        return NewErr("divide by zero");
    }

    public static IVal NoMoreElements()
    {
        return NewErr("no more elements");
    }

    public static IVal ModulusByZero()
    {
        return NewErr("modulus by zero");
    }

    public static IVal RangeError(object from, object to)
    {
        return NewErr("range error converting {0} to {1}", from, to);
    }

    public static IVal NewTypeConversionError(object from, object to)
    {
        return NewErr("type conversion error from '{0}' to '{1}'", from, to);
    }

    public static Exception NoSuchAttributeException(object context)
    {
        return new ErrException("undeclared reference to '{0}' (in container '')", context);
    }

    public static IVal NoSuchKey(object key)
    {
        return NewErr("no such key: {0}", key);
    }

    public static Exception NoSuchKeyException(object key)
    {
        return new ErrException("no such key: {0}", key);
    }

    public static Exception IndexOutOfBoundsException(object i)
    {
        return new InvalidOperationException(string.Format("index out of bounds: {0}", i));
    }

    /// <summary>
    ///     ConvertToNative implements ref.Val.ConvertToNative.
    /// </summary>
    public override object? ConvertToNative(System.Type typeDesc)
    {
        throw new NotSupportedException(error);
    }

    /// <summary>
    ///     ConvertToType implements ref.Val.ConvertToType.
    /// </summary>
    public override IVal ConvertToType(IType typeVal)
    {
        // Errors are not convertible to other representations.
        return this;
    }

    /// <summary>
    ///     Equal implements ref.Val.Equal.
    /// </summary>
    public override IVal Equal(IVal other)
    {
        // An error cannot be equal to any other value, so it returns itself.
        return this;
    }

    /// <summary>
    ///     String implements fmt.Stringer.
    /// </summary>
    public override string ToString()
    {
        return error;
    }

    /// <summary>
    ///     Type implements ref.Val.Type.
    /// </summary>
    public override IType Type()
    {
        return ErrType;
    }

    /// <summary>
    ///     Value implements ref.Val.Value.
    /// </summary>
    public override object Value()
    {
        return error;
    }

    public override bool BooleanValue()
    {
        throw new NotSupportedException();
    }

    public override long IntValue()
    {
        throw new NotSupportedException();
    }

    public override ulong UintValue()
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     IsError returns whether the input element ref.Type or ref.Val is equal to the ErrType
    ///     singleton.
    /// </summary>
    public static bool IsError(IVal? val)
    {
        return val != null && val.Type() == ErrType;
    }

    public bool HasCause()
    {
        return Cause != null;
    }

    public Exception ToRuntimeException()
    {
        if (Cause != null) throw new Exception(error, Cause);

        throw new Exception(error);
    }

    public static void ThrowErrorAsIllegalStateException(IVal val)
    {
        if (val is Err)
        {
            var e = (Err)val;
            if (e.Cause != null)
                throw new InvalidOperationException(e.error, e.Cause);
            throw new InvalidOperationException(e.error);
        }
    }

    public sealed class ErrException : ArgumentException
    {
        internal readonly object[] args;
        internal readonly string format;

        public ErrException(string format, params object[] args) : base(string.Format(format, args))
        {
            this.format = format;
            this.args = args;
        }

        public IVal Err => NewErr(format, args);
    }
}