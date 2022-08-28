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

using Cel.Common.Types.Ref;

namespace Cel.Common.Types.Pb
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.anyWithEmptyType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.Err.unknownType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.pb.PbTypeDescription.typeNameFromMessage;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.common.types.@ref.TypeAdapterSupport.maybeNativeToValue;

	using Value = Google.Api.Expr.V1Alpha1.Value;
	using Message = Google.Protobuf.IMessage;
	using Err = Cel.Common.Types.Err;
	using Types = Cel.Common.Types.Types;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeAdapterProvider = Cel.Common.Types.Ref.TypeAdapterProvider;
	using Val = Cel.Common.Types.Ref.Val;

	/// <summary>
	/// defaultTypeAdapter converts go native types to CEL values. </summary>
	public sealed class DefaultTypeAdapter : TypeAdapterProvider
	{
	  /// <summary>
	  /// DefaultTypeAdapter adapts canonical CEL types from their equivalent Go values. </summary>
	  public static readonly DefaultTypeAdapter Instance = new DefaultTypeAdapter(Db.defaultDb);

	  private readonly Db db;

	  private DefaultTypeAdapter(Db db)
	  {
		this.db = db;
	  }

	  public TypeAdapter ToTypeAdapter()
	  {
		  return NativeToValue;
	  }

	  /// <summary>
	  /// NativeToValue implements the ref.TypeAdapter interface. </summary>
	  public Val NativeToValue(object value)
	  {
		Val val = NativeToValue(db, this.ToTypeAdapter(), value);
		if (val != null)
		{
		  return val;
		}
		return Err.UnsupportedRefValConversionErr(value);
	  }

	  /// <summary>
	  /// nativeToValue returns the converted (ref.Val, true) of a conversion is found, otherwise (nil,
	  /// false)
	  /// </summary>
	  public static Val NativeToValue(Db db, TypeAdapter a, object value)
	  {
		Val v = TypeAdapterSupport.MaybeNativeToValue(a, value);
		if (v != null)
		{
		  return v;
		}

		// additional specializations may be added upon request / need.
		if (value is Val)
		{
		  return (Val) value;
		}
		if (value is Message)
		{
		  Message msg = (Message) value;
		  string typeName = PbTypeDescription.TypeNameFromMessage(msg);
		  if (typeName.Length == 0)
		  {
			return Err.AnyWithEmptyType();
		  }
		  PbTypeDescription type = db.DescribeType(typeName);
		  if (type == null)
		  {
			return Err.UnknownType(typeName);
		  }
		  value = type.MaybeUnwrap(db, msg);
		  if (value is Message)
		  {
			value = type.MaybeUnwrap(db, value);
		  }
		  return a(value);
		}
		return Err.NewErr("unsupported conversion from '%s' to value", value.GetType());
	  }

	  internal static object MaybeUnwrapValue(object value)
	  {
		if (value is Value)
		{
		  Value v = (Value) value;
		  switch (v.KindCase)
		  {
			case Value.KindOneofCase.BoolValue:
				return v.BoolValue;
			case Value.KindOneofCase.BytesValue:
			  return v.BytesValue;
			case Value.KindOneofCase.DoubleValue:
			  return v.DoubleValue;
			case Value.KindOneofCase.Int64Value:
			  return v.Int64Value;
			case Value.KindOneofCase.ListValue:
			  return v.ListValue;
			case Value.KindOneofCase.NullValue:
			  return v.NullValue;
			case Value.KindOneofCase.MapValue:
			  return v.MapValue;
			case Value.KindOneofCase.StringValue:
			  return v.StringValue;
			case Value.KindOneofCase.TypeValue:
			  return Types.GetTypeByName(v.TypeValue);
			case Value.KindOneofCase.Uint64Value:
				return v.Uint64Value;
			case Value.KindOneofCase.ObjectValue:
			  return v.ObjectValue;
		  }
		}

		return value;
	  }
	}

}