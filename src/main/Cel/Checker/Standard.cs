using Cel.Common.Operators;
using Cel.Common.Types;
using Google.Api.Expr.V1Alpha1;
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

internal sealed class Standard
{
    private Standard()
    {
    }

    // StandardDeclarations returns the Decls for all functions and constants in the evaluator.
    internal static IList<Decl> MakeStandardDeclarations()
    {
        // Some shortcuts we use when building declarations.
        var paramA = Decls.NewTypeParamType("A");
        IList<string> typeParamAList = new List<string> { "A" };
        var listOfA = Decls.NewListType(paramA);
        var paramB = Decls.NewTypeParamType("B");
        IList<string> typeParamABList = new List<string> { "A", "B" };
        var mapOfAB = Decls.NewMapType(paramA, paramB);

        IList<Decl> idents = new List<Decl>();
        foreach (var t in new List<Type>
                     { Decls.Int, Decls.Uint, Decls.Bool, Decls.Double, Decls.Bytes, Decls.String })
            idents.Add(Decls.NewVar(Types.FormatCheckedType(t), Decls.NewTypeType(t)));

        idents.Add(Decls.NewVar("list", Decls.NewTypeType(listOfA)));
        idents.Add(Decls.NewVar("map", Decls.NewTypeType(mapOfAB)));
        idents.Add(Decls.NewVar("null_type", Decls.NewTypeType(Decls.Null)));
        idents.Add(Decls.NewVar("type", Decls.NewTypeType(Decls.NewTypeType(null))));

        // Booleans
        // TODO: allow the conditional to return a heterogenous type.
        idents.Add(Decls.NewFunction(Operator.Conditional.Id,
            Decls.NewParameterizedOverload(Overloads.Conditional, new List<Type> { Decls.Bool, paramA, paramA },
                paramA, typeParamAList)));

        idents.Add(Decls.NewFunction(Operator.LogicalAnd.Id,
            Decls.NewOverload(Overloads.LogicalAnd, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.LogicalOr.Id,
            Decls.NewOverload(Overloads.LogicalOr, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.LogicalNot.Id,
            Decls.NewOverload(Overloads.LogicalNot, new List<Type> { Decls.Bool }, Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.NotStrictlyFalse.Id,
            Decls.NewOverload(Overloads.NotStrictlyFalse, new List<Type> { Decls.Bool }, Decls.Bool)));

        // Relations.

        idents.Add(Decls.NewFunction(Operator.Less.Id,
            Decls.NewOverload(Overloads.LessBool, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool),
            Decls.NewOverload(Overloads.LessInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Bool),
            Decls.NewOverload(Overloads.LessUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Bool),
            Decls.NewOverload(Overloads.LessDouble, new List<Type> { Decls.Double, Decls.Double }, Decls.Bool),
            Decls.NewOverload(Overloads.LessString, new List<Type> { Decls.String, Decls.String }, Decls.Bool),
            Decls.NewOverload(Overloads.LessBytes, new List<Type> { Decls.Bytes, Decls.Bytes }, Decls.Bool),
            Decls.NewOverload(Overloads.LessTimestamp, new List<Type> { Decls.Timestamp, Decls.Timestamp },
                Decls.Bool),
            Decls.NewOverload(Overloads.LessDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.LessEquals.Id,
            Decls.NewOverload(Overloads.LessEqualsBool, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsDouble, new List<Type> { Decls.Double, Decls.Double },
                Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsBytes, new List<Type> { Decls.Bytes, Decls.Bytes }, Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsTimestamp, new List<Type> { Decls.Timestamp, Decls.Timestamp },
                Decls.Bool),
            Decls.NewOverload(Overloads.LessEqualsDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.Greater.Id,
            Decls.NewOverload(Overloads.GreaterBool, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterDouble, new List<Type> { Decls.Double, Decls.Double }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterString, new List<Type> { Decls.String, Decls.String }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterBytes, new List<Type> { Decls.Bytes, Decls.Bytes }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterTimestamp, new List<Type> { Decls.Timestamp, Decls.Timestamp },
                Decls.Bool),
            Decls.NewOverload(Overloads.GreaterDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.GreaterEquals.Id,
            Decls.NewOverload(Overloads.GreaterEqualsBool, new List<Type> { Decls.Bool, Decls.Bool }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsDouble, new List<Type> { Decls.Double, Decls.Double },
                Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsBytes, new List<Type> { Decls.Bytes, Decls.Bytes },
                Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsTimestamp, new List<Type> { Decls.Timestamp, Decls.Timestamp },
                Decls.Bool),
            Decls.NewOverload(Overloads.GreaterEqualsDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Bool)));

        idents.Add(Decls.NewFunction(Operator.Equals.Id,
            Decls.NewParameterizedOverload(Overloads.Equals, new List<Type> { paramA, paramA }, Decls.Bool,
                typeParamAList)));

        idents.Add(Decls.NewFunction(Operator.NotEquals.Id,
            Decls.NewParameterizedOverload(Overloads.NotEquals, new List<Type> { paramA, paramA }, Decls.Bool,
                typeParamAList)));

        // Algebra.

        idents.Add(Decls.NewFunction(Operator.Subtract.Id,
            Decls.NewOverload(Overloads.SubtractInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.SubtractUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Uint),
            Decls.NewOverload(Overloads.SubtractDouble, new List<Type> { Decls.Double, Decls.Double },
                Decls.Double),
            Decls.NewOverload(Overloads.SubtractTimestampTimestamp,
                new List<Type> { Decls.Timestamp, Decls.Timestamp }, Decls.Duration),
            Decls.NewOverload(Overloads.SubtractTimestampDuration,
                new List<Type> { Decls.Timestamp, Decls.Duration }, Decls.Timestamp),
            Decls.NewOverload(Overloads.SubtractDurationDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Duration)));

        idents.Add(Decls.NewFunction(Operator.Multiply.Id,
            Decls.NewOverload(Overloads.MultiplyInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.MultiplyUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Uint),
            Decls.NewOverload(Overloads.MultiplyDouble, new List<Type> { Decls.Double, Decls.Double },
                Decls.Double)));

        idents.Add(Decls.NewFunction(Operator.Divide.Id,
            Decls.NewOverload(Overloads.DivideInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.DivideUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Uint),
            Decls.NewOverload(Overloads.DivideDouble, new List<Type> { Decls.Double, Decls.Double },
                Decls.Double)));

        idents.Add(Decls.NewFunction(Operator.Modulo.Id,
            Decls.NewOverload(Overloads.ModuloInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.ModuloUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Uint)));

        idents.Add(Decls.NewFunction(Operator.Add.Id,
            Decls.NewOverload(Overloads.AddInt64, new List<Type> { Decls.Int, Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.AddUint64, new List<Type> { Decls.Uint, Decls.Uint }, Decls.Uint),
            Decls.NewOverload(Overloads.AddDouble, new List<Type> { Decls.Double, Decls.Double }, Decls.Double),
            Decls.NewOverload(Overloads.AddString, new List<Type> { Decls.String, Decls.String }, Decls.String),
            Decls.NewOverload(Overloads.AddBytes, new List<Type> { Decls.Bytes, Decls.Bytes }, Decls.Bytes),
            Decls.NewParameterizedOverload(Overloads.AddList, new List<Type> { listOfA, listOfA }, listOfA,
                typeParamAList),
            Decls.NewOverload(Overloads.AddTimestampDuration, new List<Type> { Decls.Timestamp, Decls.Duration },
                Decls.Timestamp),
            Decls.NewOverload(Overloads.AddDurationTimestamp, new List<Type> { Decls.Duration, Decls.Timestamp },
                Decls.Timestamp),
            Decls.NewOverload(Overloads.AddDurationDuration, new List<Type> { Decls.Duration, Decls.Duration },
                Decls.Duration)));

        idents.Add(Decls.NewFunction(Operator.Negate.Id,
            Decls.NewOverload(Overloads.NegateInt64, new List<Type> { Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.NegateDouble, new List<Type> { Decls.Double }, Decls.Double)));

        // Index.

        idents.Add(Decls.NewFunction(Operator.Index.Id,
            Decls.NewParameterizedOverload(Overloads.IndexList, new List<Type> { listOfA, Decls.Int }, paramA,
                typeParamAList),
            Decls.NewParameterizedOverload(Overloads.IndexMap, new List<Type> { mapOfAB, paramA }, paramB,
                typeParamABList)));
        // Decls.newOverload(Overloads.IndexMessage,
        //	[]*expr.Type{Decls.Dyn, Decls.String}, Decls.Dyn)));

        // Collections.

        idents.Add(Decls.NewFunction(Overloads.Size,
            Decls.NewInstanceOverload(Overloads.SizeStringInst, new List<Type> { Decls.String }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.SizeBytesInst, new List<Type> { Decls.Bytes }, Decls.Int),
            Decls.NewParameterizedInstanceOverload(Overloads.SizeListInst, new List<Type> { listOfA }, Decls.Int,
                typeParamAList),
            Decls.NewParameterizedInstanceOverload(Overloads.SizeMapInst, new List<Type> { mapOfAB }, Decls.Int,
                typeParamABList),
            Decls.NewOverload(Overloads.SizeString, new List<Type> { Decls.String }, Decls.Int),
            Decls.NewOverload(Overloads.SizeBytes, new List<Type> { Decls.Bytes }, Decls.Int),
            Decls.NewParameterizedOverload(Overloads.SizeList, new List<Type> { listOfA }, Decls.Int,
                typeParamAList),
            Decls.NewParameterizedOverload(Overloads.SizeMap, new List<Type> { mapOfAB }, Decls.Int,
                typeParamABList)));

        idents.Add(Decls.NewFunction(Operator.In.Id,
            Decls.NewParameterizedOverload(Overloads.InList, new List<Type> { paramA, listOfA }, Decls.Bool,
                typeParamAList),
            Decls.NewParameterizedOverload(Overloads.InMap, new List<Type> { paramA, mapOfAB }, Decls.Bool,
                typeParamABList)));

        // Deprecated 'in()' function.

        idents.Add(Decls.NewFunction(Overloads.DeprecatedIn,
            Decls.NewParameterizedOverload(Overloads.InList, new List<Type> { paramA, listOfA }, Decls.Bool,
                typeParamAList),
            Decls.NewParameterizedOverload(Overloads.InMap, new List<Type> { paramA, mapOfAB }, Decls.Bool,
                typeParamABList)));
        // Decls.newOverload(Overloads.InMessage,
        //	[]*expr.Type{Dyn, Decls.String},Decls.Bool)));

        // Conversions to type.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertType,
            Decls.NewParameterizedOverload(Overloads.TypeConvertType, new List<Type> { paramA },
                Decls.NewTypeType(paramA), typeParamAList)));

        // Conversions to int.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertInt,
            Decls.NewOverload(Overloads.IntToInt, new List<Type> { Decls.Int }, Decls.Int),
            Decls.NewOverload(Overloads.UintToInt, new List<Type> { Decls.Uint }, Decls.Int),
            Decls.NewOverload(Overloads.DoubleToInt, new List<Type> { Decls.Double }, Decls.Int),
            Decls.NewOverload(Overloads.StringToInt, new List<Type> { Decls.String }, Decls.Int),
            Decls.NewOverload(Overloads.TimestampToInt, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewOverload(Overloads.DurationToInt, new List<Type> { Decls.Duration }, Decls.Int)));

        // Conversions to uint.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertUint,
            Decls.NewOverload(Overloads.UintToUint, new List<Type> { Decls.Uint }, Decls.Uint),
            Decls.NewOverload(Overloads.IntToUint, new List<Type> { Decls.Int }, Decls.Uint),
            Decls.NewOverload(Overloads.DoubleToUint, new List<Type> { Decls.Double }, Decls.Uint),
            Decls.NewOverload(Overloads.StringToUint, new List<Type> { Decls.String }, Decls.Uint)));

        // Conversions to double.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertDouble,
            Decls.NewOverload(Overloads.DoubleToDouble, new List<Type> { Decls.Double }, Decls.Double),
            Decls.NewOverload(Overloads.IntToDouble, new List<Type> { Decls.Int }, Decls.Double),
            Decls.NewOverload(Overloads.UintToDouble, new List<Type> { Decls.Uint }, Decls.Double),
            Decls.NewOverload(Overloads.StringToDouble, new List<Type> { Decls.String }, Decls.Double)));

        // Conversions to bool.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertBool,
            Decls.NewOverload(Overloads.BoolToBool, new List<Type> { Decls.Bool }, Decls.Bool),
            Decls.NewOverload(Overloads.StringToBool, new List<Type> { Decls.String }, Decls.Bool)));

        // Conversions to string.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertString,
            Decls.NewOverload(Overloads.StringToString, new List<Type> { Decls.String }, Decls.String),
            Decls.NewOverload(Overloads.BoolToString, new List<Type> { Decls.Bool }, Decls.String),
            Decls.NewOverload(Overloads.IntToString, new List<Type> { Decls.Int }, Decls.String),
            Decls.NewOverload(Overloads.UintToString, new List<Type> { Decls.Uint }, Decls.String),
            Decls.NewOverload(Overloads.DoubleToString, new List<Type> { Decls.Double }, Decls.String),
            Decls.NewOverload(Overloads.BytesToString, new List<Type> { Decls.Bytes }, Decls.String),
            Decls.NewOverload(Overloads.TimestampToString, new List<Type> { Decls.Timestamp }, Decls.String),
            Decls.NewOverload(Overloads.DurationToString, new List<Type> { Decls.Duration }, Decls.String)));

        // Conversions to bytes.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertBytes,
            Decls.NewOverload(Overloads.BytesToBytes, new List<Type> { Decls.Bytes }, Decls.Bytes),
            Decls.NewOverload(Overloads.StringToBytes, new List<Type> { Decls.String }, Decls.Bytes)));

        // Conversions to timestamps.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertTimestamp,
            Decls.NewOverload(Overloads.TimestampToTimestamp, new List<Type> { Decls.Timestamp }, Decls.Timestamp),
            Decls.NewOverload(Overloads.StringToTimestamp, new List<Type> { Decls.String }, Decls.Timestamp),
            Decls.NewOverload(Overloads.IntToTimestamp, new List<Type> { Decls.Int }, Decls.Timestamp)));

        // Conversions to durations.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertDuration,
            Decls.NewOverload(Overloads.DurationToDuration, new List<Type> { Decls.Duration }, Decls.Duration),
            Decls.NewOverload(Overloads.StringToDuration, new List<Type> { Decls.String }, Decls.Duration),
            Decls.NewOverload(Overloads.IntToDuration, new List<Type> { Decls.Int }, Decls.Duration)));

        // Conversions to Dyn.

        idents.Add(Decls.NewFunction(Overloads.TypeConvertDyn,
            Decls.NewParameterizedOverload(Overloads.ToDyn, new List<Type> { paramA }, Decls.Dyn, typeParamAList)));

        // String functions.

        idents.Add(Decls.NewFunction(Overloads.Contains,
            Decls.NewInstanceOverload(Overloads.ContainsString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool)));
        idents.Add(Decls.NewFunction(Overloads.EndsWith,
            Decls.NewInstanceOverload(Overloads.EndsWithString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool)));
        idents.Add(Decls.NewFunction(Overloads.Matches,
            Decls.NewInstanceOverload(Overloads.MatchesString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool)));
        idents.Add(Decls.NewFunction(Overloads.StartsWith,
            Decls.NewInstanceOverload(Overloads.StartsWithString, new List<Type> { Decls.String, Decls.String },
                Decls.Bool)));

        // Date/time functions.

        idents.Add(Decls.NewFunction(Overloads.TimeGetFullYear,
            Decls.NewInstanceOverload(Overloads.TimestampToYear, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToYearWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetMonth,
            Decls.NewInstanceOverload(Overloads.TimestampToMonth, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToMonthWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetDayOfYear,
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfYear, new List<Type> { Decls.Timestamp },
                Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfYearWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetDayOfMonth,
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfMonthZeroBased, new List<Type> { Decls.Timestamp },
                Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfMonthZeroBasedWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetDate,
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfMonthOneBased, new List<Type> { Decls.Timestamp },
                Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfMonthOneBasedWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetDayOfWeek,
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfWeek, new List<Type> { Decls.Timestamp },
                Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToDayOfWeekWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetHours,
            Decls.NewInstanceOverload(Overloads.TimestampToHours, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToHoursWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.DurationToHours, new List<Type> { Decls.Duration }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetMinutes,
            Decls.NewInstanceOverload(Overloads.TimestampToMinutes, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToMinutesWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.DurationToMinutes, new List<Type> { Decls.Duration }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetSeconds,
            Decls.NewInstanceOverload(Overloads.TimestampToSeconds, new List<Type> { Decls.Timestamp }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToSecondsWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.DurationToSeconds, new List<Type> { Decls.Duration }, Decls.Int)));

        idents.Add(Decls.NewFunction(Overloads.TimeGetMilliseconds,
            Decls.NewInstanceOverload(Overloads.TimestampToMilliseconds, new List<Type> { Decls.Timestamp },
                Decls.Int),
            Decls.NewInstanceOverload(Overloads.TimestampToMillisecondsWithTz,
                new List<Type> { Decls.Timestamp, Decls.String }, Decls.Int),
            Decls.NewInstanceOverload(Overloads.DurationToMilliseconds, new List<Type> { Decls.Duration },
                Decls.Int)));

        return idents;
    }
}