using System.Collections;
using Cel.Common.Operators;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = Cel.Common.Types.Ref.Type;

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

public abstract class MapT : BaseVal, Mapper, Container, Indexer, IterableT, Sizer
{
    /// <summary>
    ///     MapType singleton.
    /// </summary>
    public static readonly Type MapType = TypeT.NewTypeValue(TypeEnum.Map, Trait.ContainerType, Trait.IndexerType,
        Trait.IterableType, Trait.SizerType);

    public abstract Val Size();
    public abstract IteratorT Iterator();
    public abstract Val Get(Val index);
    public abstract Val Contains(Val value);
    public abstract override object Value();
    public abstract override Val Equal(Val other);
    public abstract override Val ConvertToType(Type typeValue);
    public abstract override object? ConvertToNative(System.Type typeDesc);
    public abstract Val Find(Val key);

    public override Type Type()
    {
        return MapType;
    }

    public static Val NewWrappedMap(TypeAdapter adapter, IDictionary<Val, Val> value)
    {
        return new ValMapT(adapter, value);
    }

    public static Val NewMaybeWrappedMap<T1, T2>(TypeAdapter adapter, IDictionary<T1, T2> value)
    {
        IDictionary<Val, Val> newMap = new Dictionary<Val, Val>(value.Count * 4 / 3 + 1);
        foreach (var entry in value) newMap.Add(adapter(entry.Key), adapter(entry.Value));

        return NewWrappedMap(adapter, newMap);
    }

    /// <summary>
    ///     NewJSONStruct creates a traits.Mapper implementation backed by a JSON struct that has been
    ///     encoded in protocol buffer form.
    ///     <para>
    ///         The `adapter` argument provides type adaptation capabilities from proto to CEL.
    ///     </para>
    /// </summary>
    public static Val NewJSONStruct(TypeAdapter adapter, Struct value)
    {
        IDictionary<string, Value> fields = value.Fields;
        return NewMaybeWrappedMap(adapter, fields);
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

        public override object? ConvertToNative(System.Type typeDesc)
        {
            if (typeDesc.IsAssignableFrom(typeof(IDictionary)) || typeDesc == typeof(object)) return ToJavaMap();

            if (typeDesc == typeof(Struct)) return ToPbStruct();

            if (typeDesc == typeof(Value)) return ToPbValue();

            if (typeDesc == typeof(Any))
            {
                var v = ToPbStruct();
                //        DynamicMessage dyn = DynamicMessage.newBuilder(v).build();
                //        return (T) Any.newBuilder().mergeFrom(dyn).build();
                var any = new Any();
                any.TypeUrl = "type.googleapis.com/google.protobuf.Struct";
                any.Value = v.ToByteString();
                return any;
            }

            throw new Exception(string.Format("native type conversion error from '{0}' to '{1}'", MapType,
                typeDesc.FullName));
        }

        internal Value ToPbValue()
        {
            var value = new Value();
            value.StructValue = ToPbStruct();
            return value;
        }

        internal Struct ToPbStruct()
        {
            var value = new Struct();
            foreach (var entry in map)
                value.Fields.Add(
                    entry.Key.ConvertToType(StringT.StringType).Value().ToString(),
                    (Value)entry.Value.ConvertToNative(typeof(Value)));

            return value;
        }

        internal IDictionary<object, object> ToJavaMap()
        {
            IDictionary<object, object> r = new Dictionary<object, object>();
            foreach (var entry in map) r.Add(entry.Key.Value(), entry.Value.Value());

            return r;
        }

        public override Val ConvertToType(Type typeValue)
        {
            if (typeValue == MapType) return this;

            if (typeValue == TypeT.TypeType) return MapType;

            return Err.NewTypeConversionError(MapType, typeValue);
        }

        public override IteratorT Iterator()
        {
            return IteratorT.JavaIterator(adapter, map.Keys.GetEnumerator());
        }

        public override Val Equal(Val other)
        {
            // TODO this is expensive :(
            if (!(other is MapT)) return BoolT.False;

            var o = (MapT)other;
            if (!Size().Equal(o.Size()).BooleanValue()) return BoolT.False;

            var myIter = Iterator();
            while (myIter.HasNext() == BoolT.True)
            {
                var key = myIter.Next();

                var val = Get(key);
                var oVal = o.Find(key);
                if (oVal == null) return BoolT.False;

                if (Err.IsError(val)) return val;

                if (Err.IsError(oVal)) return val;

                if (val.Type() != oVal.Type()) return Err.NoSuchOverload(val, Operator.Equals.id, oVal);

                var eq = val.Equal(oVal);
                if (eq is Err) return eq;

                if (eq != BoolT.True) return BoolT.False;
            }

            return BoolT.True;
        }

        public override object Value()
        {
            // TODO this is expensive :(
            var nativeMap = ToJavaMap();
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
            if (this == o) return true;

            if (o == null || GetType() != o.GetType()) return false;

            var valMapT = (ValMapT)o;
            return Equals(map, valMapT.map);
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
}