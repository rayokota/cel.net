using System.Collections.Generic;
using Cel.Common.Types;

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
namespace Cel.Checker
{

	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Decl = Google.Api.Expr.V1Alpha1.Decl;
	using FunctionDecl = Google.Api.Expr.V1Alpha1.Decl.Types.FunctionDecl;
	using Overload = Google.Api.Expr.V1Alpha1.Decl.Types.FunctionDecl.Types.Overload;
	using IdentDecl = Google.Api.Expr.V1Alpha1.Decl.Types.IdentDecl;
	using Type = Google.Api.Expr.V1Alpha1.Type;
	using AbstractType = Google.Api.Expr.V1Alpha1.Type.Types.AbstractType;
	using FunctionType = Google.Api.Expr.V1Alpha1.Type.Types.FunctionType;
	using ListType = Google.Api.Expr.V1Alpha1.Type.Types.ListType;
	using MapType = Google.Api.Expr.V1Alpha1.Type.Types.MapType;
	using PrimitiveType = Google.Api.Expr.V1Alpha1.Type.Types.PrimitiveType;
	using WellKnownType = Google.Api.Expr.V1Alpha1.Type.Types.WellKnownType;
	using Empty = Google.Protobuf.WellKnownTypes.Empty;
	using NullValue = Google.Protobuf.WellKnownTypes.NullValue;

	public sealed class Decls
	{

		/// <summary>
		/// Error type used to communicate issues during type-checking. </summary>
		public static readonly Type Error;

		/// <summary>
		/// Dyn is a top-type used to represent any value. </summary>
		public static readonly Type Dyn;

	  // Commonly used types.
	  public static readonly Type Bool = NewPrimitiveType(PrimitiveType.Bool);
	  public static readonly Type Bytes = NewPrimitiveType(PrimitiveType.Bytes);
	  public static readonly Type Double = NewPrimitiveType(PrimitiveType.Double);
	  public static readonly Type Int = NewPrimitiveType(PrimitiveType.Int64);
	  public static readonly Type Null;
	  public static readonly Type String = NewPrimitiveType(PrimitiveType.String);
	  public static readonly Type Uint = NewPrimitiveType(PrimitiveType.Uint64);

	  // Well-known types.
	  // TODO: Replace with an abstract type registry.
	  public static readonly Type Any = NewWellKnownType(WellKnownType.Any);
	  public static readonly Type Duration = NewWellKnownType(WellKnownType.Duration);
	  public static readonly Type Timestamp = NewWellKnownType(WellKnownType.Timestamp);

	  static Decls()
	  {
		  Type type = new Type();
		  type.Error = new Empty();
		  Error = type;

		  type = new Type();
		  type.Dyn = new Empty();
		  Dyn = type;

		  type = new Type();
		  type.Null = NullValue.NullValue;
		  Null = type;
	  }

	  /// <summary>
	  /// NewAbstractType creates an abstract type declaration which references a proto message name and
	  /// may also include type parameters.
	  /// </summary>
	  public static Type NewAbstractType(string name, IList<Type> paramTypes)
	  {
		  AbstractType abstractType = new AbstractType();
		  abstractType.Name = name;
		  abstractType.ParameterTypes.Add(paramTypes);
		  Type type = new Type();
		  type.AbstractType = abstractType;
		  return type;
	  }

	  /// <summary>
	  /// NewFunctionType creates a function invocation contract, typically only used by type-checking
	  /// steps after overload resolution.
	  /// </summary>
	  public static Type NewFunctionType(Type resultType, IList<Type> argTypes)
	  {
		  FunctionType functionType = new FunctionType();
		  functionType.ResultType = resultType;
		  functionType.ArgTypes.Add(argTypes);
		  Type type = new Type();
		  type.Function = functionType;
		  return type;
	  }

	  /// <summary>
	  /// NewFunction creates a named function declaration with one or more overloads. </summary>
	  public static Decl NewFunction(string name, params Decl.Types.FunctionDecl.Types.Overload[] overloads)
	  {
		  return NewFunction(name, new List<Overload>(overloads));
	  }

	  /// <summary>
	  /// NewFunction creates a named function declaration with one or more overloads. </summary>
	  public static Decl NewFunction(string name, IList<Decl.Types.FunctionDecl.Types.Overload> overloads)
	  {
		  FunctionDecl functionDecl = new FunctionDecl();
		  functionDecl.Overloads.Add(overloads);
		  Decl decl = new Decl();
		  decl.Name = name;
		  decl.Function = functionDecl;
		  return decl;
	  }

	  /// <summary>
	  /// NewIdent creates a named identifier declaration with an optional literal value.
	  /// 
	  /// <para>Literal values are typically only associated with enum identifiers.
	  /// 
	  /// </para>
	  /// <para>Deprecated: Use NewVar or NewConst instead.
	  /// </para>
	  /// </summary>
	  public static Decl NewIdent(string name, Type t, Constant v)
	  {
		  IdentDecl ident = new IdentDecl();
		  ident.Type = t;
		if (v != null)
		{
		  ident.Value = v;
		}

		Decl decl = new Decl();
		decl.Name = name;
		decl.Ident = ident;
		return decl;
	  }

	  /// <summary>
	  /// NewConst creates a constant identifier with a CEL constant literal value. </summary>
	  public static Decl NewConst(string name, Type t, Constant v)
	  {
		return NewIdent(name, t, v);
	  }

	  /// <summary>
	  /// NewVar creates a variable identifier. </summary>
	  public static Decl NewVar(string name, Type t)
	  {
		return NewIdent(name, t, null);
	  }

	  /// <summary>
	  /// NewInstanceOverload creates a instance function overload contract. First element of argTypes is
	  /// instance.
	  /// </summary>
	  public static Decl.Types.FunctionDecl.Types.Overload NewInstanceOverload(string id, IList<Type> argTypes, Type resultType)
	  {
		  Overload overload = new Overload();
		  overload.OverloadId = id;
		  overload.ResultType = resultType;
		  overload.Params.Add(argTypes);
		  overload.IsInstanceFunction = true;
		  return overload;
	  }

	  /// <summary>
	  /// NewListType generates a new list with elements of a certain type. </summary>
	  public static Type NewListType(Type elem)
	  {
		  ListType listType = new ListType();
		  listType.ElemType = elem;
		  Type type = new Type();
		  type.ListType = listType;
		  return type;
	  }

	  /// <summary>
	  /// NewMapType generates a new map with typed keys and values. </summary>
	  public static Type NewMapType(Type key, Type value)
	  {
		  MapType mapType = new MapType();
		  mapType.KeyType = key;
		  mapType.ValueType = value;
		  Type type = new Type();
		  type.MapType = mapType;
		  return type;
	  }

	  /// <summary>
	  /// NewObjectType creates an object type for a qualified type name. </summary>
	  public static Type NewObjectType(string typeName)
	  {
		  Type type = new Type();
		  type.MessageType = typeName;
		  return type;
	  }

	  /// <summary>
	  /// NewOverload creates a function overload declaration which contains a unique overload id as well
	  /// as the expected argument and result types. Overloads must be aggregated within a Function
	  /// declaration.
	  /// </summary>
	  public static Decl.Types.FunctionDecl.Types.Overload NewOverload(string id, IList<Type> argTypes, Type resultType)
	  {
		  Overload overload = new Overload();
		  overload.OverloadId = id;
		  overload.ResultType = resultType;
		  overload.Params.Add(argTypes);
		  overload.IsInstanceFunction = false;
		  return overload;
	  }

	  /// <summary>
	  /// NewParameterizedInstanceOverload creates a parametric function instance overload type. </summary>
	  public static Decl.Types.FunctionDecl.Types.Overload NewParameterizedInstanceOverload(string id, IList<Type> argTypes, Type resultType, IList<string> typeParams)
	  {
		  Overload overload = new Overload();
		  overload.OverloadId = id;
		  overload.ResultType = resultType;
		  overload.Params.Add(argTypes);
		  overload.TypeParams.Add(typeParams);
		  overload.IsInstanceFunction = true;
		  return overload;
	  }

	  /// <summary>
	  /// NewParameterizedOverload creates a parametric function overload type. </summary>
	  public static Decl.Types.FunctionDecl.Types.Overload NewParameterizedOverload(string id, IList<Type> argTypes, Type resultType, IList<string> typeParams)
	  {
		  Overload overload = new Overload();
		  overload.OverloadId = id;
		  overload.ResultType = resultType;
		  overload.Params.Add(argTypes);
		  overload.TypeParams.Add(typeParams);
		  overload.IsInstanceFunction = false;
		  return overload;
	  }

	  /// <summary>
	  /// NewPrimitiveType creates a type for a primitive value. See the var declarations for Int, Uint,
	  /// etc.
	  /// </summary>
	  public static Type NewPrimitiveType(PrimitiveType primitive)
	  {
		  Type type = new Type();
		  type.Primitive = primitive;
		  return type;
	  }

	  /// <summary>
	  /// NewTypeType creates a new type designating a type. </summary>
	  public static Type NewTypeType(Type nested)
	  {
		if (nested == null)
		{
		  // must set the nested field for a valid oneof option
		  nested = new Type();
		}

		Type type = new Type();
		type.Type_ = nested;
		return type;
	  }

	  /// <summary>
	  /// NewTypeParamType creates a type corresponding to a named, contextual parameter. </summary>
	  public static Type NewTypeParamType(string name)
	  {
		  Type type = new Type();
		  type.TypeParam = name;
		  return type;
	  }

	  /// <summary>
	  /// NewWellKnownType creates a type corresponding to a protobuf well-known type value. </summary>
	  public static Type NewWellKnownType(WellKnownType wellKnown)
	  {
		  Type type = new Type();
		  type.WellKnown = wellKnown;
		  return type;
	  }

	  /// <summary>
	  /// NewWrapperType creates a wrapped primitive type instance. Wrapped types are roughly equivalent
	  /// to a nullable, or optionally valued type.
	  /// </summary>
	  public static Type NewWrapperType(Type wrapped)
	  {
		PrimitiveType primitive = wrapped.Primitive;
		if (primitive == PrimitiveType.Unspecified)
		{
		  // TODO: return an error
		  throw new System.ArgumentException(System.String.Format("Wrapped type must be a primitive, but is '{0}'", wrapped));
		}

		Type type = new Type();
		type.Wrapper = primitive;
		return type;
	  }
	}

}