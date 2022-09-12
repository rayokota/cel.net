using System.Collections;
using Cel.Common.Operators;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Type = System.Type;

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

public abstract class MapT : BaseVal, IMapper, IContainer, IIndexer, IIterableT, ISizer
{
    /// <summary>
    ///     MapType singleton.
    /// </summary>
    public static readonly IType MapType = TypeT.NewTypeValue(TypeEnum.Map, Trait.ContainerType, Trait.IndexerType,
        Trait.IterableType, Trait.SizerType);

    public abstract IVal Size();
    public abstract IIteratorT Iterator();
    public abstract IVal Get(IVal index);
    public abstract IVal Contains(IVal value);
    public abstract override object Value();
    public abstract override IVal Equal(IVal other);
    public abstract override IVal ConvertToType(IType typeValue);
    public abstract override object? ConvertToNative(Type typeDesc);
    public abstract IVal? Find(IVal key);

    public override IType Type()
    {
        return MapType;
    }

    public static IVal NewWrappedMap(TypeAdapter adapter, IDictionary<IVal, IVal> value)
    {
        return new ValMapT(adapter, value);
    }

    public static IVal NewMaybeWrappedMap(TypeAdapter adapter, IDictionary value)
    {
        IDictionary<IVal, IVal> newMap = new Dictionary<IVal, IVal>(value.Count * 4 / 3 + 1);
        foreach (DictionaryEntry entry in value) newMap.Add(adapter(entry.Key), adapter(entry.Value));

        return NewWrappedMap(adapter, newMap);
    }

    /// <summary>
    ///     NewJSONStruct creates a traits.Mapper implementation backed by a JSON struct that has been
    ///     encoded in protocol buffer form.
    ///     <para>
    ///         The `adapter` argument provides type adaptation capabilities from proto to CEL.
    ///     </para>
    /// </summary>
    public static IVal NewJSONStruct(TypeAdapter adapter, Struct value)
    {
        IDictionary fields = value.Fields;
        return NewMaybeWrappedMap(adapter, fields);
    }

    internal sealed class ValMapT : MapT
    {
        private readonly TypeAdapter adapter;
        private readonly IDictionary<IVal, IVal> map;

        internal ValMapT(TypeAdapter adapter, IDictionary<IVal, IVal> map)
        {
            this.adapter = adapter;
            this.map = map;
        }

        public override object? ConvertToNative(Type typeDesc)
        {
            var isGenericDict = typeDesc.IsGenericType &&
                                (typeDesc.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                                 typeDesc.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            if (isGenericDict ||
                typeof(IDictionary).IsAssignableFrom(typeDesc) ||
                typeDesc == typeof(object)) return ToHashtable();

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

        internal IDictionary ToHashtable()
        {
            IDictionary r = new Dictionary<object, object>();
            foreach (var entry in map) r.Add(entry.Key.Value(), entry.Value.Value());
            return r;
        }

        public override IVal ConvertToType(IType typeValue)
        {
            if (typeValue == MapType) return this;

            if (typeValue == TypeT.TypeType) return MapType;

            return Err.NewTypeConversionError(MapType, typeValue);
        }

        public override IIteratorT Iterator()
        {
            return IIteratorT.Iterator(adapter, map.Keys.GetEnumerator());
        }

        public override IVal Equal(IVal other)
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
            var nativeMap = ToHashtable();
            return nativeMap;
        }

        public override IVal Contains(IVal value)
        {
            return Types.BoolOf(map.ContainsKey(value));
        }

        public override IVal Get(IVal index)
        {
            map.TryGetValue(index, out var v);
            if (v == null) return Err.NoSuchField(index.Value());
            return v;
        }

        public override IVal Size()
        {
            return IntT.IntOf(map.Count);
        }

        public override IVal? Find(IVal key)
        {
            map.TryGetValue(key, out var v);
            return v;
        }

        public override bool Equals(object? o)
        {
            if (this == o) return true;

            if (o == null || GetType() != o.GetType()) return false;

            var valMapT = (ValMapT)o;
            return map.Count == valMapT.map.Count && !map.Except(valMapT.map).Any();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), map);
        }

        public override string ToString()
        {
            return "MapT{" + "adapter=" + adapter + ", map=" + map + '}';
        }
    }
}