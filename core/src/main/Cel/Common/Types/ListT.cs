using System;
using System.Collections.Generic;
using System.Linq;
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
//	import static Cel.Common.Types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.newTypeConversionError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noMoreElements;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.valOrErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Any = Google.Protobuf.WellKnownTypes.Any;
	using ListValue = Google.Protobuf.WellKnownTypes.ListValue;
	using Value = Google.Protobuf.WellKnownTypes.Value;
	using Operator = Cel.Common.Operators.Operator;
	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
	using Val = Cel.Common.Types.Ref.Val;
	using Lister = Cel.Common.Types.Traits.Lister;
	using Trait = Cel.Common.Types.Traits.Trait;

	public abstract class ListT : BaseVal, Lister
	{
		public abstract Val Size();
		public abstract IteratorT Iterator();
		public abstract Val Get(Ref.Val index);
		public abstract Val Contains(Ref.Val value);
		public abstract Val Add(Ref.Val other);
		public abstract override object Value();
		public abstract override Val Equal(Ref.Val other);
		public abstract override Val ConvertToType(Ref.Type typeValue);
		public abstract override object? ConvertToNative(System.Type typeDesc);
	  /// <summary>
	  /// ListType singleton. </summary>
	  public static readonly Type ListType = TypeT.NewTypeValue(TypeEnum.List, Trait.AdderType, Trait.ContainerType, Trait.IndexerType, Trait.IterableType, Trait.SizerType);

	  public static Val NewStringArrayList(string[] value)
	  {
		return NewGenericArrayList(v => StringT.StringOf((string) v), value);
	  }

	  public static Val NewGenericArrayList(TypeAdapter adapter, object[] value)
	  {
		return new GenericListT(adapter, value);
	  }

	  public static Val NewValArrayList(TypeAdapter adapter, Val[] value)
	  {
		return new ValListT(adapter, value);
	  }

	  public override Type Type()
	  {
		return ListType;
	  }

	  internal abstract class BaseListT : ListT
	  {
		protected internal readonly TypeAdapter adapter;
		protected internal readonly long size;

		internal BaseListT(TypeAdapter adapter, long size)
		{
		  this.adapter = adapter;
		  this.size = size;
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public <T> T convertToNative(Class<T> typeDesc)
		public override object? ConvertToNative(System.Type typeDesc)
		{
		  if (typeDesc.IsArray)
		  {
			object array = ToJavaArray<object>(typeDesc);

			return array;
		  }
		  if (typeDesc == typeof(System.Collections.IList) || typeDesc == typeof(object))
		  {
			return ToJavaList();
		  }
		  if (typeDesc == typeof(ListValue))
		  {
			return ToPbListValue();
		  }
		  if (typeDesc == typeof(Value))
		  {
			return ToPbValue();
		  }
		  if (typeDesc == typeof(Any))
		  {
			ListValue v = ToPbListValue();
			//        Descriptor anyDesc = Any.getDescriptor();
			//        FieldDescriptor anyFieldTypeUrl = anyDesc.findFieldByName("type_url");
			//        FieldDescriptor anyFieldValue = anyDesc.findFieldByName("value");
			//        DynamicMessage dyn = DynamicMessage.newBuilder(Any.getDefaultInstance())
			//            .setField(anyFieldTypeUrl, )
			//            .setField(anyFieldValue, v.toByteString())
			//            .build();

			//        return (T) dyn;
			//        return (T)
			// Any.newBuilder().setTypeUrl("type.googleapis.com/google.protobuf.ListValue").setValue(dyn.toByteString()).build();
			Any any = new Any();
			any.TypeUrl = "type.googleapis.com/google.protobuf.ListValue";
			any.Value = v.ToByteString();
			return any;
		  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  throw new System.ArgumentException(string.Format("Unsupported conversion of '{0}' to '{1}'", ListType, typeDesc.FullName));
		}

		internal virtual Value ToPbValue()
		{
		  Value value = new Value();
			  value.ListValue = ToPbListValue();
			  return value;
		}

		internal virtual ListValue ToPbListValue()
		{
			ListValue list = new ListValue();
		  int s = (int) size;
		  for (int i = 0; i < s; i++)
		  {
			Val v = Get(IntT.IntOf(i));
			Value e = (Value)v.ConvertToNative(typeof(Value));
			list.Values.Add(e);
		  }
		  return list;
		}

		internal virtual IList<object> ToJavaList()
		{
		  return new List<object> {ConvertToNative(typeof(object[]))};
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"rawtypes", "unchecked"}) private <T> Object toJavaArray(Class<T> typeDesc)
		internal virtual object ToJavaArray<T>(System.Type typeDesc)
		{
		  int s = (int) size;
		  System.Type compType = typeDesc.GetElementType();
		  if (compType == typeof(Enum))
		  {
			// Note: cannot create `Enum` values of the right type here.
			compType = typeof(object);
		  }
		  object array = Array.CreateInstance(compType, s);

		  System.Func<object, object> fixForTarget = x => x;

		  for (int i = 0; i < s; i++)
		  {
			Val v = Get(IntT.IntOf(i));
			object e = v.ConvertToNative(compType);
			e = fixForTarget(e);
			((Array)array).SetValue(e, i);
		  }
		  return array;
		}

		public override Val ConvertToType(Type typeValue)
		{
		  switch (typeValue.TypeEnum().InnerEnumValue)
		  {
			case TypeEnum.InnerEnum.List:
			  return this;
			case TypeEnum.InnerEnum.Type:
			  return ListType;
		  }
		  return Err.NewTypeConversionError(ListType, typeValue);
		}

		public override IteratorT Iterator()
		{
		  return new ArrayListIteratorT(this);
		}

		public override Val Equal(Val other)
		{
		  if (other.Type() != ListType)
		  {
			return BoolT.False;
		  }
		  ListT o = (ListT) other;
		  if (size != o.Size().IntValue())
		  {
			return BoolT.False;
		  }
		  for (long i = 0; i < size; i++)
		  {
			IntT idx = IntT.IntOf(i);
			Val e1 = Get(idx);
			if (Err.IsError(e1))
			{
			  return e1;
			}
			Val e2 = o.Get(idx);
			if (Err.IsError(e2))
			{
			  return e2;
			}
			if (e1.Type() != e2.Type())
			{
			  return Err.NoSuchOverload(e1, Operator.Equals.id, e2);
			}
			if (e1.Equal(e2) != BoolT.True)
			{
			  return BoolT.False;
			}
		  }
		  return BoolT.True;
		}

		public override Val Contains(Val value)
		{
		  Type firstType = null;
		  Type mixedType = null;
		  for (long i = 0; i < size; i++)
		  {
			Val elem = Get(IntT.IntOf(i));
			Type elemType = elem.Type();
			if (firstType == null)
			{
			  firstType = elemType;
			}
			else if (!firstType.Equals(elemType))
			{
			  mixedType = elemType;
			}
			if (value.Equal(elem) == BoolT.True)
			{
			  return BoolT.True;
			}
		  }
		  if (mixedType != null)
		  {
			return Err.NoSuchOverload(value, Operator.In.id, firstType, mixedType);
		  }
		  return BoolT.False;
		}

		public override Val Size()
		{
		  return IntT.IntOf(size);
		}

		private sealed class ArrayListIteratorT : BaseVal, IteratorT
		{
			private readonly ListT.BaseListT outerInstance;

			public ArrayListIteratorT(ListT.BaseListT outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		  internal long index;

		  public Val HasNext()
		  {
			return Types.BoolOf(index < outerInstance.size);
		  }

		  public Val Next()
		  {
			if (index < outerInstance.size)
			{
			  return outerInstance.Get(IntT.IntOf(index++));
			}
			return Err.NoMoreElements();
		  }

		  public override object? ConvertToNative(System.Type typeDesc)
		  {
			throw new System.NotSupportedException("IMPLEMENT ME??");
		  }

		  public override Val ConvertToType(Type typeValue)
		  {
			throw new System.NotSupportedException("IMPLEMENT ME??");
		  }

		  public override Val Equal(Val other)
		  {
			throw new System.NotSupportedException("IMPLEMENT ME??");
		  }

		  public override Type Type()
		  {
			throw new System.NotSupportedException("IMPLEMENT ME??");
		  }

		  public override object Value()
		  {
			throw new System.NotSupportedException("IMPLEMENT ME??");
		  }
		}
	  }

	  internal sealed class GenericListT : BaseListT
	  {
		internal readonly object[] array;

		internal GenericListT(TypeAdapter adapter, object[] array) : base(adapter, array.Length)
		{
		  this.array = array;
		}

		public override object Value()
		{
		  return array;
		}

		public override Val Add(Val other)
		{
		  if (!(other is Lister))
		  {
			return Err.NoSuchOverload(this, "add", other);
		  }
		  Lister otherList = (Lister) other;
		  object[] otherArray = (object[]) otherList.Value();
		  object[] newArray = new object[array.Length + otherArray.Length];
		  Array.Copy(array, 0, newArray, 0, array.Length);
		  Array.Copy(otherArray, 0, newArray, array.Length, otherArray.Length);
		  return new GenericListT(adapter, newArray);
		}

		public override Val Get(Val index)
		{
		  if (!(index is IntT))
		  {
			return Err.ValOrErr(index, "unsupported index type '%s' in list", index.Type());
		  }
		  int sz = array.Length;
		  int i = (int) index.IntValue();
		  if (i < 0 || i >= sz)
		  {
			// Note: the conformance tests assert on 'invalid_argument'
			return Err.NewErr("invalid_argument: index '%d' out of range in list of size '%d'", i, sz);
		  }

		  return adapter(array[i]);
		}

		public override string ToString()
		{
		  return "GenericListT{" + "array=" + "[" + string.Join(", ", array) + "]" + ", adapter=" + adapter + ", size=" + size + '}';
		}
	  }

	  internal sealed class ValListT : BaseListT
	  {
		internal readonly Val[] array;

		internal ValListT(TypeAdapter adapter, Val[] array) : base(adapter, array.Length)
		{
		  this.array = array;
		}

		public override object Value()
		{
		  object[] nativeArray = new object[array.Length];
		  for (int i = 0; i < array.Length; i++)
		  {
			nativeArray[i] = array[i].Value();
		  }
		  return nativeArray;
		}

		public override Val Add(Val other)
		{
		  if (!(other is Lister))
		  {
			return Err.NoSuchOverload(this, "add", other);
		  }
		  if (other is ValListT)
		  {
			Val[] otherArray = ((ValListT) other).array;
			Val[] newArray = new Val[array.Length + otherArray.Length];
			Array.Copy(array, 0, newArray, 0, array.Length);
			Array.Copy(otherArray, 0, newArray, array.Length, otherArray.Length);
			return new ValListT(adapter, newArray);
		  }
		  else
		  {
			Lister otherLister = (Lister) other;
			int otherSIze = (int) otherLister.Size().IntValue();
			Val[] newArray = new Val[array.Length + otherSIze];
			Array.Copy(array, 0, newArray, 0, array.Length);
			for (int i = 0; i < otherSIze; i++)
			{
			  newArray[array.Length + i] = otherLister.Get(IntT.IntOf(i));
			}
			return new ValListT(adapter, newArray);
		  }
		}

		public override Val Get(Val index)
		{
		  if (!(index is IntT))
		  {
			return Err.ValOrErr(index, "unsupported index type '%s' in list", index.Type());
		  }
		  int sz = array.Length;
		  int i = (int) index.IntValue();
		  if (i < 0 || i >= sz)
		  {
			// Note: the conformance tests assert on 'invalid_argument'
			return Err.NewErr("invalid_argument: index '%d' out of range in list of size '%d'", i, sz);
		  }
		  return array[i];
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
		  ValListT valListT = (ValListT) o;
		  return array.SequenceEqual(valListT.array);
		}

		public override int GetHashCode()
		{
		  int result = base.GetHashCode();
		  result = 31 * result + array.GetHashCode();
		  return result;
		}

		public override string ToString()
		{
		  return "ValListT{" + "array=" + "[" + string.Join<Val>(", ", array) + "]" + ", adapter=" + adapter + ", size=" + size + '}';
		}
	  }

	  /// <summary>
	  /// NewJSONList returns a traits.Lister based on structpb.ListValue instance. </summary>
	  public static Val NewJSONList(TypeAdapter adapter, ListValue l)
	  {
		  IList<Value> vals = l.Values;
		return NewGenericArrayList(adapter, vals.ToArray());
	  }
	}

}