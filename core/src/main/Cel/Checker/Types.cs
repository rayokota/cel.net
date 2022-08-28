using System.Collections.Generic;
using System.Text;

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
	using Type = Google.Api.Expr.V1Alpha1.Type;
	using AbstractType = Google.Api.Expr.V1Alpha1.Type.Types.AbstractType;
	using FunctionType = Google.Api.Expr.V1Alpha1.Type.Types.FunctionType;
	using MapType = Google.Api.Expr.V1Alpha1.Type.Types.MapType;
	using PrimitiveType = Google.Api.Expr.V1Alpha1.Type.Types.PrimitiveType;
	using TypeKindCase = Google.Api.Expr.V1Alpha1.Type.TypeKindOneofCase;
	using WellKnownType = Google.Api.Expr.V1Alpha1.Type.Types.WellKnownType;

	public sealed class Types
	{

	  internal enum Kind
	  {
		kindUnknown,
		kindError,
		kindFunction,
		kindDyn,
		kindPrimitive,
		kindWellKnown,
		kindWrapper,
		kindNull,
		kindAbstract, // TODO: Update the checker protos to include abstract
		kindType,
		kindList,
		kindMap,
		kindObject,
		kindTypeParam
	  }

	  /// <summary>
	  /// FormatCheckedType converts a type message into a string representation. </summary>
	  public static string FormatCheckedType(Type t)
	  {
		// This is a very hot method.

		if (t == null)
		{
		  return "(type not known)";
		}

		// short-cut the "easy" types
		switch (KindOf(t))
		{
		  case global::Cel.Checker.Types.Kind.kindDyn:
			return "dyn";
		  case global::Cel.Checker.Types.Kind.kindNull:
			return "null";
		  case global::Cel.Checker.Types.Kind.kindPrimitive:
			switch (t.Primitive)
			{
			  case PrimitiveType.Uint64:
				return "uint";
			  case PrimitiveType.Int64:
				return "int";
			  case PrimitiveType.Bool:
				return "bool";
			  case PrimitiveType.Bytes:
				return "bytes";
			  case PrimitiveType.Double:
				return "double";
			  case PrimitiveType.String:
				return "string";
			}
			// unrecognizes & not-specified - ignore above
			return t.Primitive.ToString().ToLowerInvariant().Trim();
		  case global::Cel.Checker.Types.Kind.kindWellKnown:
			switch (t.WellKnown)
			{
			  case WellKnownType.Any:
				return "any";
			  case WellKnownType.Duration:
				return "duration";
			  case WellKnownType.Timestamp:
				return "timestamp";
			}
			break;
		  case global::Cel.Checker.Types.Kind.kindError:
			return "!error!";
		}

		// complex types, use a StringBuilder, which is more efficient
		StringBuilder sb = new StringBuilder();
		FormatCheckedType(sb, t);
		return sb.ToString();
	  }

	  internal static void FormatCheckedType(StringBuilder sb, Type t)
	  {
		switch (KindOf(t))
		{
		  case global::Cel.Checker.Types.Kind.kindDyn:
			sb.Append("dyn");
			return;
		  case global::Cel.Checker.Types.Kind.kindFunction:
			TypeErrors.FormatFunction(sb, t.Function.ResultType, t.Function.ArgTypes, false);
			return;
		  case global::Cel.Checker.Types.Kind.kindList:
			sb.Append("list(");
			FormatCheckedType(sb, t.ListType.ElemType);
			sb.Append(')');
			return;
		  case global::Cel.Checker.Types.Kind.kindObject:
			sb.Append(t.MessageType);
			return;
		  case global::Cel.Checker.Types.Kind.kindMap:
			sb.Append("map(");
			FormatCheckedType(sb, t.MapType.KeyType);
			sb.Append(", ");
			FormatCheckedType(sb, t.MapType.ValueType);
			sb.Append(')');
			return;
		  case global::Cel.Checker.Types.Kind.kindNull:
			sb.Append("null");
			return;
		  case global::Cel.Checker.Types.Kind.kindPrimitive:
			FormatCheckedTypePrimitive(sb, t.Primitive);
			return;
		  case global::Cel.Checker.Types.Kind.kindType:
			if (Object.Equals(t.Type_, new Type()))
			{
			  sb.Append("type");
			  return;
			}
			sb.Append("type(");
			FormatCheckedType(sb, t.Type_);
			sb.Append(')');
			return;
		  case global::Cel.Checker.Types.Kind.kindWellKnown:
			switch (t.WellKnown)
			{
			  case WellKnownType.Any:
				sb.Append("any");
				return;
			  case WellKnownType.Duration:
				sb.Append("duration");
				return;
			  case WellKnownType.Timestamp:
				sb.Append("timestamp");
				return;
			}
			break;
		  case global::Cel.Checker.Types.Kind.kindWrapper:
			sb.Append("wrapper(");
			FormatCheckedTypePrimitive(sb, t.Wrapper);
			sb.Append(')');
			return;
		  case global::Cel.Checker.Types.Kind.kindError:
			sb.Append("!error!");
			return;
		}

		string tStr = t.ToString();
		for (int i = 0; i < tStr.Length; i++)
		{
		  char c = tStr[i];
		  if (c != '\n')
		  {
			sb.Append(c);
		  }
		}
	  }

	  private static void FormatCheckedTypePrimitive(StringBuilder sb, PrimitiveType t)
	  {
		switch (t)
		{
		  case PrimitiveType.Uint64:
			sb.Append("uint");
			return;
		  case PrimitiveType.Int64:
			sb.Append("int");
			return;
		  case PrimitiveType.Bool:
			sb.Append("bool");
			return;
		  case PrimitiveType.Bytes:
			sb.Append("bytes");
			return;
		  case PrimitiveType.Double:
			sb.Append("double");
			return;
		  case PrimitiveType.String:
			sb.Append("string");
			return;
		}
		// unrecognizes & not-specified - ignore above
		sb.Append(t.ToString().ToLowerInvariant().Trim());
	  }

	  /// <summary>
	  /// isDyn returns true if the input t is either type DYN or a well-known ANY message. </summary>
	  internal static bool IsDyn(Type t)
	  {
		// Note: object type values that are well-known and map to a DYN value in practice
		// are sanitized prior to being added to the environment.
		switch (KindOf(t))
		{
		  case global::Cel.Checker.Types.Kind.kindDyn:
			return true;
		  case global::Cel.Checker.Types.Kind.kindWellKnown:
			return t.WellKnown == WellKnownType.Any;
		  default:
			return false;
		}
	  }

	  /// <summary>
	  /// isDynOrError returns true if the input is either an Error, DYN, or well-known ANY message. </summary>
	  internal static bool IsDynOrError(Type t)
	  {
		if (KindOf(t) == Kind.kindError)
		{
		  return true;
		}
		return IsDyn(t);
	  }

	  /// <summary>
	  /// isEqualOrLessSpecific checks whether one type is equal or less specific than the other one. A
	  /// type is less specific if it matches the other type using the DYN type.
	  /// </summary>
	  internal static bool IsEqualOrLessSpecific(Type t1, Type t2)
	  {
		Kind kind1 = KindOf(t1);
		Kind kind2 = KindOf(t2);
		// The first type is less specific.
		if (IsDyn(t1) || kind1 == Kind.kindTypeParam)
		{
		  return true;
		}
		// The first type is not less specific.
		if (IsDyn(t2) || kind2 == Kind.kindTypeParam)
		{
		  return false;
		}
		// Types must be of the same kind to be equal.
		if (kind1 != kind2)
		{
		  return false;
		}

		// With limited exceptions for ANY and JSON values, the types must agree and be equivalent in
		// order to return true.
		switch (kind1)
		{
		  case global::Cel.Checker.Types.Kind.kindAbstract:
		  {
			  Type.Types.AbstractType a1 = t1.AbstractType;
			  Type.Types.AbstractType a2 = t2.AbstractType;
			  if (!a1.Name.Equals(a2.Name) || a1.ParameterTypes.Count != a2.ParameterTypes.Count)
			  {
				return false;
			  }
			  for (int i = 0; i < a1.ParameterTypes.Count; i++)
			  {
				Type p1 = a1.ParameterTypes[i];
				if (!IsEqualOrLessSpecific(p1, a2.ParameterTypes[i]))
				{
				  return false;
				}
			  }
			  return true;
		  }
		  case global::Cel.Checker.Types.Kind.kindFunction:
		  {
			  Type.Types.FunctionType fn1 = t1.Function;
			  Type.Types.FunctionType fn2 = t2.Function;
			  if (fn1.ArgTypes.Count != fn2.ArgTypes.Count)
			  {
				return false;
			  }
			  if (!IsEqualOrLessSpecific(fn1.ResultType, fn2.ResultType))
			  {
				return false;
			  }
			  for (int i = 0; i < fn1.ArgTypes.Count; i++)
			  {
				Type a1 = fn1.ArgTypes[i];
				if (!IsEqualOrLessSpecific(a1, fn2.ArgTypes[i]))
				{
				  return false;
				}
			  }
			  return true;
		  }
		  case global::Cel.Checker.Types.Kind.kindList:
			return IsEqualOrLessSpecific(t1.ListType.ElemType, t2.ListType.ElemType);
		  case global::Cel.Checker.Types.Kind.kindMap:
		  {
			  Type.Types.MapType m1 = t1.MapType;
			  Type.Types.MapType m2 = t2.MapType;
			  return IsEqualOrLessSpecific(m1.KeyType, m2.KeyType) && IsEqualOrLessSpecific(m1.ValueType, m2.ValueType);
		  }
		  case global::Cel.Checker.Types.Kind.kindType:
			return true;
		  default:
			return t1.Equals(t2);
		}
	  }

	  /// <summary>
	  /// internalIsAssignable returns true if t1 is assignable to t2. </summary>
	  internal static bool InternalIsAssignable(Mapping m, Type t1, Type t2)
	  {
		// A type is always assignable to itself.
		// Early terminate the call to avoid cases of infinite recursion.
		if (t1.Equals(t2))
		{
		  return true;
		}
		// Process type parameters.

		Kind kind1 = KindOf(t1);
		Kind kind2 = KindOf(t2);
		if (kind2 == Kind.kindTypeParam)
		{
		  Type t2Sub = m.Find(t2);
		  if (t2Sub != null)
		  {
			// If the types are compatible, pick the more general type and return true
			if (InternalIsAssignable(m, t1, t2Sub))
			{
			  m.Add(t2, MostGeneral(t1, t2Sub));
			  return true;
			}
			return false;
		  }
		  if (NotReferencedIn(m, t2, t1))
		  {
			m.Add(t2, t1);
			return true;
		  }
		}
		if (kind1 == Kind.kindTypeParam)
		{
		  // For the lower type bound, we currently do not perform adjustment. The restricted
		  // way we use type parameters in lower type bounds, it is not necessary, but may
		  // become if we generalize type unification.
		  Type t1Sub = m.Find(t1);
		  if (t1Sub != null)
		  {
			// If the types are compatible, pick the more general type and return true
			if (InternalIsAssignable(m, t1Sub, t2))
			{
			  m.Add(t1, MostGeneral(t1Sub, t2));
			  return true;
			}
			return false;
		  }
		  if (NotReferencedIn(m, t1, t2))
		  {
			m.Add(t1, t2);
			return true;
		  }
		}

		// Next check for wildcard types.
		if (IsDynOrError(t1) || IsDynOrError(t2))
		{
		  return true;
		}

		// Test for when the types do not need to agree, but are more specific than dyn.
		switch (kind1)
		{
		  case global::Cel.Checker.Types.Kind.kindNull:
			return InternalIsAssignableNull(t2);
		  case global::Cel.Checker.Types.Kind.kindPrimitive:
			return InternalIsAssignablePrimitive(t1.Primitive, t2);
		  case global::Cel.Checker.Types.Kind.kindWrapper:
			return InternalIsAssignable(m, Decls.NewPrimitiveType(t1.Wrapper), t2);
		  default:
			if (kind1 != kind2)
			{
			  return false;
			}
			break;
		}

		// Test for when the types must agree.
		switch (kind1)
		{
			// ERROR, TYPE_PARAM, and DYN handled above.
		  case global::Cel.Checker.Types.Kind.kindAbstract:
			return InternalIsAssignableAbstractType(m, t1.AbstractType, t2.AbstractType);
		  case global::Cel.Checker.Types.Kind.kindFunction:
			return InternalIsAssignableFunction(m, t1.Function, t2.Function);
		  case global::Cel.Checker.Types.Kind.kindList:
			return InternalIsAssignable(m, t1.ListType.ElemType, t2.ListType.ElemType);
		  case global::Cel.Checker.Types.Kind.kindMap:
			return InternalIsAssignableMap(m, t1.MapType, t2.MapType);
		  case global::Cel.Checker.Types.Kind.kindObject:
			return t1.MessageType.Equals(t2.MessageType);
		  case global::Cel.Checker.Types.Kind.kindType:
			// A type is a type is a type, any additional parameterization of the
			// type cannot affect method resolution or assignability.
			return true;
		  case global::Cel.Checker.Types.Kind.kindWellKnown:
			return t1.WellKnown == t2.WellKnown;
		  default:
			return false;
		}
	  }

	  /// <summary>
	  /// internalIsAssignableAbstractType returns true if the abstract type names agree and all type
	  /// parameters are assignable.
	  /// </summary>
	  internal static bool InternalIsAssignableAbstractType(Mapping m, AbstractType a1, AbstractType a2)
	  {
		if (!a1.Name.Equals(a2.Name))
		{
		  return false;
		}
		return InternalIsAssignableList(m, a1.ParameterTypes, a2.ParameterTypes);
	  }

	  /// <summary>
	  /// internalIsAssignableFunction returns true if the function return type and arg types are
	  /// assignable.
	  /// </summary>
	  internal static bool InternalIsAssignableFunction(Mapping m, FunctionType f1, FunctionType f2)
	  {
		IList<Type> f1ArgTypes = FlattenFunctionTypes(f1);
		IList<Type> f2ArgTypes = FlattenFunctionTypes(f2);
		return InternalIsAssignableList(m, f1ArgTypes, f2ArgTypes);
	  }

	  /// <summary>
	  /// internalIsAssignableList returns true if the element types at each index in the list are
	  /// assignable from l1[i] to l2[i]. The list lengths must also agree for the lists to be
	  /// assignable.
	  /// </summary>
	  internal static bool InternalIsAssignableList(Mapping m, IList<Type> l1, IList<Type> l2)
	  {
		if (l1.Count != l2.Count)
		{
		  return false;
		}
		for (int i = 0; i < l1.Count; i++)
		{
		  Type t1 = l1[i];
		  if (!InternalIsAssignable(m, t1, l2[i]))
		  {
			return false;
		  }
		}
		return true;
	  }

	  /// <summary>
	  /// internalIsAssignableMap returns true if map m1 may be assigned to map m2. </summary>
	  internal static bool InternalIsAssignableMap(Mapping m, MapType m1, MapType m2)
	  {
		return InternalIsAssignableList(m, new List<Type> {m1.KeyType, m1.ValueType}, new List<Type> {m2.KeyType, m2.ValueType});
	  }

	  /// <summary>
	  /// internalIsAssignableNull returns true if the type is nullable. </summary>
	  internal static bool InternalIsAssignableNull(Type t)
	  {
		switch (KindOf(t))
		{
		  case global::Cel.Checker.Types.Kind.kindAbstract:
		  case global::Cel.Checker.Types.Kind.kindObject:
		  case global::Cel.Checker.Types.Kind.kindNull:
		  case global::Cel.Checker.Types.Kind.kindWellKnown:
		  case global::Cel.Checker.Types.Kind.kindWrapper:
			return true;
		  default:
			return false;
		}
	  }

	  /// <summary>
	  /// internalIsAssignablePrimitive returns true if the target type is the same or if it is a wrapper
	  /// for the primitive type.
	  /// </summary>
	  internal static bool InternalIsAssignablePrimitive(PrimitiveType p, Type target)
	  {
		switch (KindOf(target))
		{
		  case global::Cel.Checker.Types.Kind.kindPrimitive:
			return p == target.Primitive;
		  case global::Cel.Checker.Types.Kind.kindWrapper:
			return p == target.Wrapper;
		  default:
			return false;
		}
	  }

	  /// <summary>
	  /// isAssignable returns an updated type substitution mapping if t1 is assignable to t2. </summary>
	  internal static Mapping IsAssignable(Mapping m, Type t1, Type t2)
	  {
		Mapping mCopy = m.Copy();
		if (InternalIsAssignable(mCopy, t1, t2))
		{
		  return mCopy;
		}
		return null;
	  }

	  /// <summary>
	  /// isAssignableList returns an updated type substitution mapping if l1 is assignable to l2. </summary>
	  internal static Mapping IsAssignableList(Mapping m, IList<Type> l1, IList<Type> l2)
	  {
		Mapping mCopy = m.Copy();
		if (InternalIsAssignableList(mCopy, l1, l2))
		{
		  return mCopy;
		}
		return null;
	  }

	  /// <summary>
	  /// kindOf returns the kind of the type as defined in the checked.proto. </summary>
	  internal static Kind KindOf(Type t)
	  {
		if (t == null || t.TypeKindCase == TypeKindCase.None)
		{
		  return Kind.kindUnknown;
		}
		switch (t.TypeKindCase)
		{
		  case TypeKindCase.Error:
			return Kind.kindError;
		  case TypeKindCase.Function:
			return Kind.kindFunction;
		  case TypeKindCase.Dyn:
			return Kind.kindDyn;
		  case TypeKindCase.Primitive:
			return Kind.kindPrimitive;
		  case TypeKindCase.WellKnown:
			return Kind.kindWellKnown;
		  case TypeKindCase.Wrapper:
			return Kind.kindWrapper;
		  case TypeKindCase.Null:
			return Kind.kindNull;
		  case TypeKindCase.Type_:
			return Kind.kindType;
		  case TypeKindCase.ListType:
			return Kind.kindList;
		  case TypeKindCase.MapType:
			return Kind.kindMap;
		  case TypeKindCase.MessageType:
			return Kind.kindObject;
		  case TypeKindCase.TypeParam:
			return Kind.kindTypeParam;
		}
		return Kind.kindUnknown;
	  }

	  /// <summary>
	  /// mostGeneral returns the more general of two types which are known to unify. </summary>
	  internal static Type MostGeneral(Type t1, Type t2)
	  {
		if (IsEqualOrLessSpecific(t1, t2))
		{
		  return t1;
		}
		return t2;
	  }

	  /// <summary>
	  /// notReferencedIn checks whether the type doesn't appear directly or transitively within the
	  /// other type. This is a standard requirement for type unification, commonly referred to as the
	  /// "occurs check".
	  /// </summary>
	  internal static bool NotReferencedIn(Mapping m, Type t, Type withinType)
	  {
		if (t.Equals(withinType))
		{
		  return false;
		}
		Kind withinKind = KindOf(withinType);
		switch (withinKind)
		{
		  case global::Cel.Checker.Types.Kind.kindTypeParam:
			Type wtSub = m.Find(withinType);
			if (wtSub == null)
			{
			  return true;
			}
			return NotReferencedIn(m, t, wtSub);
		  case global::Cel.Checker.Types.Kind.kindAbstract:
			foreach (Type pt in withinType.AbstractType.ParameterTypes)
			{
			  if (!NotReferencedIn(m, t, pt))
			  {
				return false;
			  }
			}
			return true;
		  case global::Cel.Checker.Types.Kind.kindFunction:
			FunctionType fn = withinType.Function;
			IList<Type> types = FlattenFunctionTypes(fn);
			foreach (Type a in types)
			{
			  if (!NotReferencedIn(m, t, a))
			  {
				return false;
			  }
			}
			return true;
		  case global::Cel.Checker.Types.Kind.kindList:
			return NotReferencedIn(m, t, withinType.ListType.ElemType);
		  case global::Cel.Checker.Types.Kind.kindMap:
			MapType mt = withinType.MapType;
			return NotReferencedIn(m, t, mt.KeyType) && NotReferencedIn(m, t, mt.ValueType);
		  case global::Cel.Checker.Types.Kind.kindWrapper:
			return NotReferencedIn(m, t, Decls.NewPrimitiveType(withinType.Wrapper));
		  default:
			return true;
		}
	  }

	  /// <summary>
	  /// substitute replaces all direct and indirect occurrences of bound type parameters. Unbound type
	  /// parameters are replaced by DYN if typeParamToDyn is true.
	  /// </summary>
	  internal static Type Substitute(Mapping m, Type t, bool typeParamToDyn)
	  {
		Type tSub = m.Find(t);
		if (tSub != null)
		{
		  return Substitute(m, tSub, typeParamToDyn);
		}
		Kind kind = KindOf(t);
		if (typeParamToDyn && kind == Kind.kindTypeParam)
		{
		  return Decls.Dyn;
		}
		switch (kind)
		{
		  case global::Cel.Checker.Types.Kind.kindAbstract:
			// TODO: implement!
			AbstractType at = t.AbstractType;
			IList<Type> @params = new List<Type>(at.ParameterTypes.Count);
			foreach (Type p in at.ParameterTypes)
			{
			  @params.Add(Substitute(m, p, typeParamToDyn));
			}
			return Decls.NewAbstractType(at.Name, @params);
		  case global::Cel.Checker.Types.Kind.kindFunction:
			FunctionType fn = t.Function;
			Type rt = Substitute(m, fn.ResultType, typeParamToDyn);
			IList<Type> args = new List<Type>(fn.ArgTypes.Count);
			foreach (Type a in fn.ArgTypes)
			{
			  args.Add(Substitute(m, a, typeParamToDyn));
			}
			return Decls.NewFunctionType(rt, args);
		  case global::Cel.Checker.Types.Kind.kindList:
			return Decls.NewListType(Substitute(m, t.ListType.ElemType, typeParamToDyn));
		  case global::Cel.Checker.Types.Kind.kindMap:
			MapType mt = t.MapType;
			return Decls.NewMapType(Substitute(m, mt.KeyType, typeParamToDyn), Substitute(m, mt.ValueType, typeParamToDyn));
		  case global::Cel.Checker.Types.Kind.kindType:
			if (Object.Equals(t.Type_, new Type()))
			{
			  return Decls.NewTypeType(Substitute(m, t.Type_, typeParamToDyn));
			}
			return t;
		  default:
			return t;
		}
	  }

	  internal static string TypeKey(Type t)
	  {
		return FormatCheckedType(t);
	  }

	  /// <summary>
	  /// flattenFunctionTypes takes a function with arg types T1, T2, ..., TN and result type TR and
	  /// returns a slice containing {T1, T2, ..., TN, TR}.
	  /// </summary>
	  internal static IList<Type> FlattenFunctionTypes(FunctionType f)
	  {
		IList<Type> argTypes = f.ArgTypes;
		if (argTypes.Count == 0)
		{
			return new List<Type> { f.ResultType };
		}
		IList<Type> flattened = new List<Type>(argTypes.Count + 1);
		((List<Type>)flattened).AddRange(argTypes);
		flattened.Add(f.ResultType);
		return flattened;
	  }
	}

}