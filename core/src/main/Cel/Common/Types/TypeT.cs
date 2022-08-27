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
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using Type = Cel.Common.Types.Ref.Type;
	using TypeEnum = Cel.Common.Types.Ref.TypeEnum;
	using Val = Cel.Common.Types.Ref.Val;
	using Trait = Cel.Common.Types.Traits.Trait;

	/// <summary>
	/// TypeValue is an instance of a Value that describes a value's type. </summary>
	public class TypeT : Type, Val
	{
	  /// <summary>
	  /// TypeType is the type of a TypeValue. </summary>
	  public static readonly Type TypeType = NewTypeValue(Ref.TypeEnum.Type);

	  private readonly TypeEnum typeEnum;
	  private readonly ISet<Trait> traitMask;

	  internal TypeT(TypeEnum typeEnum, params Trait[] traits)
	  {
		this.typeEnum = typeEnum;
		this.traitMask = new HashSet<Trait>(traits);
	  }

	  /// <summary>
	  /// NewTypeValue returns *TypeValue which is both a ref.Type and ref.Val. </summary>
	  internal static Type NewTypeValue(TypeEnum typeEnum, params Trait[] traits)
	  {
		return new TypeT(typeEnum, traits);
	  }

	  /// <summary>
	  /// NewObjectTypeValue returns a *TypeValue based on the input name, which is annotated with the
	  /// traits relevant to all objects.
	  /// </summary>
	  public static Type NewObjectTypeValue(string name)
	  {
		return new ObjectTypeT(name);
	  }

	  internal sealed class ObjectTypeT : TypeT
	  {
		internal readonly string typeName;

		internal ObjectTypeT(string typeName) : base(Ref.TypeEnum.Object, Trait.FieldTesterType, Trait.IndexerType)
		{
		  this.typeName = typeName;
		}

		public override string TypeName()
		{
		  return typeName;
		}
	  }

	  public virtual bool BooleanValue()
	  {
		throw new System.NotSupportedException();
	  }

	  public virtual long IntValue()
	  {
		throw new System.NotSupportedException();
	  }

	  /// <summary>
	  /// ConvertToNative implements ref.Val.ConvertToNative. </summary>
	  public virtual object? ConvertToNative(System.Type typeDesc)
	  {
		throw new System.NotSupportedException("type conversion not supported for 'type'");
	  }

	  /// <summary>
	  /// ConvertToType implements ref.Val.ConvertToType. </summary>
	  public virtual Val ConvertToType(Type typeVal)
	  {
		switch (typeVal.TypeEnum().InnerEnumValue)
		{
		  case Ref.TypeEnum.InnerEnum.Type:
			return TypeType;
		  case Ref.TypeEnum.InnerEnum.String:
			return StringT.StringOf(TypeName());
		}
		return Err.NewTypeConversionError(TypeType, typeVal);
	  }

	  /// <summary>
	  /// Equal implements ref.Val.Equal. </summary>
	  public virtual Val Equal(Val other)
	  {
		if (TypeType != other.Type())
		{
		  return Err.NoSuchOverload(this, "equal", other);
		}
		return BoolT.BoolOf(this.Equals(other));
	  }

	  /// <summary>
	  /// HasTrait indicates whether the type supports the given trait. Trait codes are defined in the
	  /// traits package, e.g. see traits.AdderType.
	  /// </summary>
	  public virtual bool HasTrait(Trait trait)
	  {
		return traitMask.contains(trait);
	  }

	  public virtual TypeEnum TypeEnum()
	  {
		return typeEnum;
	  }

	  /// <summary>
	  /// String implements fmt.Stringer. </summary>
	  public override string ToString()
	  {
		return TypeName();
	  }

	  /// <summary>
	  /// Type implements ref.Val.Type. </summary>
	  public virtual Type Type()
	  {
		return TypeType;
	  }

	  /// <summary>
	  /// TypeName gives the type's name as a string. </summary>
	  public virtual string TypeName()
	  {
		return typeEnum.getName();
	  }

	  /// <summary>
	  /// Value implements ref.Val.Value. </summary>
	  public virtual object Value()
	  {
		return TypeName();
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
		Type typeValue = (Type) o;
		return typeEnum == typeValue.TypeEnum() && TypeName().Equals(typeValue.TypeName());
	  }

	  public override int GetHashCode()
	  {
		return TypeName().GetHashCode();
	  }
	}

}