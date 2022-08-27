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
//	import static Cel.Common.Types.Types.boolOf;

	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeDescription = Cel.Common.Types.Ref.TypeDescription;
	using Val = Cel.Common.Types.Ref.Val;
	using FieldTester = Cel.Common.Types.Traits.FieldTester;
	using Indexer = Cel.Common.Types.Traits.Indexer;

	public abstract class ObjectT : BaseVal, FieldTester, Indexer, TypeAdapter
	{
		public abstract Val get(Ref.Val index);
		public abstract Val isSet(Ref.Val field);
	  protected internal readonly TypeAdapter adapter;
	  protected internal readonly object value;
	  protected internal readonly TypeDescription typeDesc;
	  protected internal readonly Type typeValue;

	  protected internal ObjectT(TypeAdapter adapter, object value, TypeDescription typeDesc, Type typeValue)
	  {
		this.adapter = adapter;
		this.value = value;
		this.typeDesc = typeDesc;
		this.typeValue = typeValue;
	  }

	  public override Val ConvertToType(Type typeVal)
	  {
		switch (typeVal.TypeEnum().innerEnumValue)
		{
		  case Type:
			return typeValue;
		  case Cel.Common.Types.Ref.TypeEnum.InnerEnum.Object:
			if (Type().TypeName().Equals(typeVal.TypeName()))
			{
			  return this;
			}
			break;
		}
		return newTypeConversionError(typeDesc.Name(), typeVal);
	  }

	  public override Val Equal(Val other)
	  {
		if (!typeDesc.Name().Equals(other.Type().TypeName()))
		{
		  return noSuchOverload(this, "equal", other);
		}
		return boolOf(this.value.Equals(other.Value()));
	  }

	  public override Type Type()
	  {
		return typeValue;
	  }

	  public override object Value()
	  {
		return value;
	  }

	  public virtual Val NativeToValue(object value)
	  {
		return adapter.NativeToValue(value);
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
		ObjectT objectT = (ObjectT) o;
		return Objects.equals(value, objectT.value) && Objects.equals(typeDesc, objectT.typeDesc) && Objects.equals(typeValue, objectT.typeValue);
	  }

	  public override int GetHashCode()
	  {
		return Objects.hash(base.GetHashCode(), value, typeDesc, typeValue);
	  }
	}

}