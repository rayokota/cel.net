using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;

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
//	import static Cel.Common.Types.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.isError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TypeT.TypeType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using Struct = Google.Protobuf.WellKnownTypes.Struct;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using Operator = Cel.Common.Operators.Operator;
	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
	using Val = Cel.Common.Types.Ref.Val;
	using Container = Cel.Common.Types.Traits.Container;
	using Indexer = Cel.Common.Types.Traits.Indexer;
	using Mapper = Cel.Common.Types.Traits.Mapper;
	using Sizer = Cel.Common.Types.Traits.Sizer;
	using Trait = Cel.Common.Types.Traits.Trait;

	public abstract class MapT : BaseVal, Mapper, Container, Indexer, IterableT, Sizer
	{
		public abstract Val Size();
		public abstract IteratorT Iterator();
		public abstract Val Get(Ref.Val index);
		public abstract Val Contains(Ref.Val value);
		public override abstract object Value();
		public override abstract Val Equal(Ref.Val other);
		public override abstract Val ConvertToType(Ref.Type typeValue);
		public override abstract object? ConvertToNative(System.Type typeDesc);
		public abstract Val Find(Ref.Val key);
	  /// <summary>
	  /// MapType singleton. </summary>
	  public static readonly Type MapType = TypeT.NewTypeValue(TypeEnum.Map, Trait.ContainerType, Trait.IndexerType, Trait.IterableType, Trait.SizerType);

	  public static Val NewWrappedMap(TypeAdapter adapter, IDictionary<Val, Val> value)
	  {
		return new ValMapT(adapter, value);
	  }

	  public static Val NewMaybeWrappedMap<T1, T2>(TypeAdapter adapter, IDictionary<T1, T2> value)
	  {
		IDictionary<Val, Val> newMap = new Dictionary<Val, Val>(value.Count * 4 / 3 + 1);
		foreach (KeyValuePair<T1, T2> entry in value)
		{
			newMap.Add(adapter(entry.Key), adapter(entry.Value));
		}
		return NewWrappedMap(adapter, newMap);
	  }

	  public override Type Type()
	  {
		return MapType;
	  }

	  internal sealed class ValMapT : MapT
	  {

		internal readonly TypeAdapter adapter;
		internal readonly IDictionary<Val, Val> map;

		internal ValMapT(TypeAdapter adapter, IDictionary<Val, Val> map)
		{
		  this.adapter = adapter;
		  this.map = map;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
		public override object? ConvertToNative(System.Type typeDesc)
		{
		  if (typeDesc.IsAssignableFrom(typeof(System.Collections.IDictionary)) || typeDesc == typeof(object))
		  {
			return ToJavaMap();
		  }
		  if (typeDesc == typeof(Struct))
		  {
			return ToPbStruct();
		  }
		  if (typeDesc == typeof(Value))
		  {
			return ToPbValue();
		  }
		  if (typeDesc == typeof(Any))
		  {
			Struct v = ToPbStruct();
			//        DynamicMessage dyn = DynamicMessage.newBuilder(v).build();
			//        return (T) Any.newBuilder().mergeFrom(dyn).build();
			Any any = new Any();
			any.TypeUrl = "type.googleapis.com/google.protobuf.Struct";
			any.Value = v.ToByteString();
			return any;
		  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", MapType, typeDesc.FullName));
		}

		internal Value ToPbValue()
		{
			Value value = new Value();
			value.StructValue = ToPbStruct();
			return value;
		}

		internal Struct ToPbStruct()
		{
			Struct value = new Struct();
			foreach (KeyValuePair<Val, Val> entry in map)
			{
			  value.Fields.Add(
				  entry.Key.ConvertToType(StringT.StringType).Value().ToString(), 
				  (Value) entry.Value.ConvertToNative(typeof(Value)));
			}

			return value;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"rawtypes", "unchecked"}) private java.util.Map toJavaMap()
		internal IDictionary<object, object> ToJavaMap()
		{
		  IDictionary<object, object> r = new Dictionary<object, object>();
		  foreach (KeyValuePair<Val, Val> entry in map)
		  {
			  r.Add(entry.Key.Value(), entry.Value.Value());
			  
		  }
		  return r;
		}

		public override Val ConvertToType(Type typeValue)
		{
		  if (typeValue == MapType)
		  {
			return this;
		  }
		  if (typeValue == TypeT.TypeType)
		  {
			return MapType;
		  }
		  return Err.NewTypeConversionError(MapType, typeValue);
		}

		public override IteratorT Iterator()
		{
		  return IteratorT.JavaIterator(adapter, map.Keys.GetEnumerator());
		}

		public override Val Equal(Val other)
		{
		  // TODO this is expensive :(
		  if (!(other is MapT))
		  {
			return BoolT.False;
		  }
		  MapT o = (MapT) other;
		  if (!Size().Equal(o.Size()).BooleanValue())
		  {
			return BoolT.False;
		  }
		  IteratorT myIter = Iterator();
		  while (myIter.HasNext() == BoolT.True)
		  {
			Val key = myIter.Next();

			Val val = Get(key);
			Val oVal = o.Find(key);
			if (oVal == null)
			{
			  return BoolT.False;
			}
			if (Err.IsError(val))
			{
			  return val;
			}
			if (Err.IsError(oVal))
			{
			  return val;
			}
			if (val.Type() != oVal.Type())
			{
			  return Err.NoSuchOverload(val, Operator.Equals.id, oVal);
			}
			Val eq = val.Equal(oVal);
			if (eq is Err)
			{
			  return eq;
			}
			if (eq != BoolT.True)
			{
			  return BoolT.False;
			}
		  }
		  return BoolT.True;
		}

		public override object Value()
		{
		  // TODO this is expensive :(
		  IDictionary<object, object> nativeMap = ToJavaMap();
		  return nativeMap;
		}

		public override Val Contains(Val value)
		{
		  return Types.BoolOf(map.ContainsKey(value));
		}

		public override Val Get(Val index)
		{
		  return map[index];
		}

		public override Val Size()
		{
		  return IntT.IntOf(map.Count);
		}

		public override Val Find(Val key)
		{
		  return map[key];
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
		  ValMapT valMapT = (ValMapT) o;
		  return Object.Equals(map, valMapT.map);
		}

		public override int GetHashCode()
		{
		  return HashCode.Combine(base.GetHashCode(), map);
		}

		public override string ToString()
		{
		  return "JavaMapT{" + "adapter=" + adapter + ", map=" + map + '}';
		}
	  }

	  /// <summary>
	  /// NewJSONStruct creates a traits.Mapper implementation backed by a JSON struct that has been
	  /// encoded in protocol buffer form.
	  /// 
	  /// <para>The `adapter` argument provides type adaptation capabilities from proto to CEL.
	  /// </para>
	  /// </summary>
	  public static Val NewJSONStruct(TypeAdapter adapter, Struct value)
	  {
		  IDictionary<string, Value> fields = value.Fields;
		return NewMaybeWrappedMap(adapter, fields);
	  }
	}

}