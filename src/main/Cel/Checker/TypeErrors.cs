using System.Text;
using Cel.Common;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Checker;

/// <summary>
///     TypeErrors is a specialization of Errors.
/// </summary>
public sealed class TypeErrors : Errors
{
    public TypeErrors(ISource source) : base(source)
    {
    }

    internal void UndeclaredReference(ILocation l, string container, string name)
    {
        ReportError(l, "undeclared reference to '{0}' (in container '{1}')", name, container);
    }

    internal void ExpressionDoesNotSelectField(ILocation l)
    {
        ReportError(l, "expression does not select a field");
    }

    internal void TypeDoesNotSupportFieldSelection(ILocation l, Type t)
    {
        ReportError(l, "type '{0}' does not support field selection", Types.FormatCheckedType(t));
    }

    internal void UndefinedField(ILocation l, string field)
    {
        ReportError(l, "undefined field '{0}'", field);
    }

    internal void FieldDoesNotSupportPresenceCheck(ILocation l, string field)
    {
        ReportError(l, "field '{0}' does not support presence check", field);
    }

    internal void OverlappingOverload(ILocation l, string name, string overloadId1, Type f1, string overloadId2,
        Type f2)
    {
        ReportError(l,
            "overlapping overload for name '{0}' (type '{1}' with overloadId: '{2}' cannot be distinguished from '{3}' with " +
            "overloadId: '{4}')", name, Types.FormatCheckedType(f1), overloadId1, Types.FormatCheckedType(f2),
            overloadId2);
    }

    internal void OverlappingMacro(ILocation l, string name, int args)
    {
        ReportError(l, "overload for name '{0}' with %d argument(s) overlaps with predefined macro", name, args);
    }

    internal void NoMatchingOverload(ILocation l, string name, IList<Type> args, bool isInstance)
    {
        var signature = FormatFunction(null, args, isInstance);
        ReportError(l, "found no matching overload for '{0}' applied to '{1}'", name, signature);
    }

    internal void AggregateTypeMismatch(ILocation l, Type aggregate, Type member)
    {
        ReportError(l,
            "type '{0}' does not match previous type '{1}' in aggregate. Use 'dyn(x)' to make the aggregate dynamic.",
            Types.FormatCheckedType(member), Types.FormatCheckedType(aggregate));
    }

    internal void NotAType(ILocation l, Type t)
    {
        ReportError(l, "'{0}' is not a type", Types.FormatCheckedType(t), t);
    }

    internal void NotAMessageType(ILocation l, Type t)
    {
        ReportError(l, "'{0}' is not a message type", Types.FormatCheckedType(t));
    }

    internal void FieldTypeMismatch(ILocation l, string name, Type field, Type? value)
    {
        ReportError(l, "expected type of field '{0}' is '{1}' but provided type is '{2}'", name,
            Types.FormatCheckedType(field), Types.FormatCheckedType(value));
    }

    internal void UnexpectedFailedResolution(ILocation l, string typeName)
    {
        ReportError(l, "[internal] unexpected failed resolution of '{0}'", typeName);
    }

    internal void NotAComprehensionRange(ILocation l, Type t)
    {
        ReportError(l,
            "expression of type '{0}' cannot be range of a comprehension (must be list, map, or dynamic)",
            Types.FormatCheckedType(t));
    }

    internal void TypeMismatch(ILocation l, Type? expected, Type? actual)
    {
        ReportError(l, "expected type '{0}' but found '{1}'", Types.FormatCheckedType(expected),
            Types.FormatCheckedType(actual));
    }

    public void UnknownType(ILocation l, string info)
    {
        //    reportError(l, "unknown type{0}", info != null ? " for: " + info : "");
    }

    internal static string FormatFunction(Type? resultType, IList<Type> argTypes, bool isInstance)
    {
        var result = new StringBuilder();
        FormatFunction(result, resultType, argTypes, isInstance);
        return result.ToString();
    }

    internal static void FormatFunction(StringBuilder result, Type? resultType, IList<Type> argTypes,
        bool isInstance)
    {
        if (isInstance)
        {
            var target = argTypes[0];
            argTypes = argTypes.Where((value, index) => index > 0).ToList();

            Types.FormatCheckedType(result, target);
            result.Append(".");
        }

        result.Append("(");
        for (var i = 0; i < argTypes.Count; i++)
        {
            var arg = argTypes[i];
            if (i > 0) result.Append(", ");

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