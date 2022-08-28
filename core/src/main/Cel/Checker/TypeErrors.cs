﻿using System.Collections.Generic;
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
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Checker.Types.formatCheckedType;

	using Type = Google.Api.Expr.V1Alpha1.Type;
	using Errors = global::Cel.Common.Errors;
	using Location = global::Cel.Common.Location;
	using Source = global::Cel.Common.Source;

	/// <summary>
	/// TypeErrors is a specialization of Errors. </summary>
	public sealed class TypeErrors : Errors
	{

	  public TypeErrors(Source source) : base(source)
	  {
	  }

	  internal void UndeclaredReference(Location l, string container, string name)
	  {
		ReportError(l, "undeclared reference to '%s' (in container '%s')", name, container);
	  }

	  internal void ExpressionDoesNotSelectField(Location l)
	  {
		ReportError(l, "expression does not select a field");
	  }

	  internal void TypeDoesNotSupportFieldSelection(Location l, Type t)
	  {
		ReportError(l, "type '%s' does not support field selection", Types.FormatCheckedType(t));
	  }

	  internal void UndefinedField(Location l, string field)
	  {
		ReportError(l, "undefined field '%s'", field);
	  }

	  internal void FieldDoesNotSupportPresenceCheck(Location l, string field)
	  {
		ReportError(l, "field '%s' does not support presence check", field);
	  }

	  internal void OverlappingOverload(Location l, string name, string overloadID1, Type f1, string overloadID2, Type f2)
	  {
		ReportError(l, "overlapping overload for name '%s' (type '%s' with overloadId: '%s' cannot be distinguished from '%s' with " + "overloadId: '%s')", name, Types.FormatCheckedType(f1), overloadID1, Types.FormatCheckedType(f2), overloadID2);
	  }

	  internal void OverlappingMacro(Location l, string name, int args)
	  {
		ReportError(l, "overload for name '%s' with %d argument(s) overlaps with predefined macro", name, args);
	  }

	  internal void NoMatchingOverload(Location l, string name, IList<Type> args, bool isInstance)
	  {
		string signature = FormatFunction(null, args, isInstance);
		ReportError(l, "found no matching overload for '%s' applied to '%s'", name, signature);
	  }

	  internal void AggregateTypeMismatch(Location l, Type aggregate, Type member)
	  {
		ReportError(l, "type '%s' does not match previous type '%s' in aggregate. Use 'dyn(x)' to make the aggregate dynamic.", Types.FormatCheckedType(member), Types.FormatCheckedType(aggregate));
	  }

	  internal void NotAType(Location l, Type t)
	  {
		ReportError(l, "'%s' is not a type", Types.FormatCheckedType(t), t);
	  }

	  internal void NotAMessageType(Location l, Type t)
	  {
		ReportError(l, "'%s' is not a message type", Types.FormatCheckedType(t));
	  }

	  internal void FieldTypeMismatch(Location l, string name, Type field, Type value)
	  {
		ReportError(l, "expected type of field '%s' is '%s' but provided type is '%s'", name, Types.FormatCheckedType(field), Types.FormatCheckedType(value));
	  }

	  internal void UnexpectedFailedResolution(Location l, string typeName)
	  {
		ReportError(l, "[internal] unexpected failed resolution of '%s'", typeName);
	  }

	  internal void NotAComprehensionRange(Location l, Type t)
	  {
		ReportError(l, "expression of type '%s' cannot be range of a comprehension (must be list, map, or dynamic)", Types.FormatCheckedType(t));
	  }

	  internal void TypeMismatch(Location l, Type expected, Type actual)
	  {
		ReportError(l, "expected type '%s' but found '%s'", Types.FormatCheckedType(expected), Types.FormatCheckedType(actual));
	  }

	  public void UnknownType(Location l, string info)
	  {
		//    reportError(l, "unknown type%s", info != null ? " for: " + info : "");
	  }

	  internal static string FormatFunction(Type resultType, IList<Type> argTypes, bool isInstance)
	  {
		StringBuilder result = new StringBuilder();
		FormatFunction(result, resultType, argTypes, isInstance);
		return result.ToString();
	  }

	  internal static void FormatFunction(StringBuilder result, Type resultType, IList<Type> argTypes, bool isInstance)
	  {
		if (isInstance)
		{
		  Type target = argTypes[0];
		  argTypes = argTypes.Where((value, index) => index > 0).ToList();

		  Types.FormatCheckedType(result, target);
		  result.Append(".");
		}

		result.Append("(");
		for (int i = 0; i < argTypes.Count; i++)
		{
		  Type arg = argTypes[i];
		  if (i > 0)
		  {
			result.Append(", ");
		  }
		  Types.FormatCheckedType(result, arg);
		}
		result.Append(")");
		if (resultType != null)
		{
		  result.Append(" -> ");
		  Types.FormatCheckedType(result, resultType);
		}
	  }
	}

}