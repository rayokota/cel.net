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

namespace Cel.Common.Types
{
    public sealed class Overloads
    {
        private Overloads()
        {
        }

        // Boolean logic overloads
        public const string Conditional = "conditional";
        public const string LogicalAnd = "logical_and";
        public const string LogicalOr = "logical_or";
        public const string LogicalNot = "logical_not";
        public const string NotStrictlyFalse = "not_strictly_false";
        public const string Equals = "equals";
        public const string NotEquals = "not_equals";
        public const string LessBool = "less_bool";
        public const string LessInt64 = "less_int64";
        public const string LessUint64 = "less_uint64";
        public const string LessDouble = "less_double";
        public const string LessString = "less_string";
        public const string LessBytes = "less_bytes";
        public const string LessTimestamp = "less_timestamp";
        public const string LessDuration = "less_duration";
        public const string LessEqualsBool = "less_equals_bool";
        public const string LessEqualsInt64 = "less_equals_int64";
        public const string LessEqualsUint64 = "less_equals_uint64";
        public const string LessEqualsDouble = "less_equals_double";
        public const string LessEqualsString = "less_equals_string";
        public const string LessEqualsBytes = "less_equals_bytes";
        public const string LessEqualsTimestamp = "less_equals_timestamp";
        public const string LessEqualsDuration = "less_equals_duration";
        public const string GreaterBool = "greater_bool";
        public const string GreaterInt64 = "greater_int64";
        public const string GreaterUint64 = "greater_uint64";
        public const string GreaterDouble = "greater_double";
        public const string GreaterString = "greater_string";
        public const string GreaterBytes = "greater_bytes";
        public const string GreaterTimestamp = "greater_timestamp";
        public const string GreaterDuration = "greater_duration";
        public const string GreaterEqualsBool = "greater_equals_bool";
        public const string GreaterEqualsInt64 = "greater_equals_int64";
        public const string GreaterEqualsUint64 = "greater_equals_uint64";
        public const string GreaterEqualsDouble = "greater_equals_double";
        public const string GreaterEqualsString = "greater_equals_string";
        public const string GreaterEqualsBytes = "greater_equals_bytes";
        public const string GreaterEqualsTimestamp = "greater_equals_timestamp";
        public const string GreaterEqualsDuration = "greater_equals_duration";

        // Math overloads
        public const string AddInt64 = "add_int64";
        public const string AddUint64 = "add_uint64";
        public const string AddDouble = "add_double";
        public const string AddString = "add_string";
        public const string AddBytes = "add_bytes";
        public const string AddList = "add_list";
        public const string AddTimestampDuration = "add_timestamp_duration";
        public const string AddDurationTimestamp = "add_duration_timestamp";
        public const string AddDurationDuration = "add_duration_duration";
        public const string SubtractInt64 = "subtract_int64";
        public const string SubtractUint64 = "subtract_uint64";
        public const string SubtractDouble = "subtract_double";
        public const string SubtractTimestampTimestamp = "subtract_timestamp_timestamp";
        public const string SubtractTimestampDuration = "subtract_timestamp_duration";
        public const string SubtractDurationDuration = "subtract_duration_duration";
        public const string MultiplyInt64 = "multiply_int64";
        public const string MultiplyUint64 = "multiply_uint64";
        public const string MultiplyDouble = "multiply_double";
        public const string DivideInt64 = "divide_int64";
        public const string DivideUint64 = "divide_uint64";
        public const string DivideDouble = "divide_double";
        public const string ModuloInt64 = "modulo_int64";
        public const string ModuloUint64 = "modulo_uint64";
        public const string NegateInt64 = "negate_int64";
        public const string NegateDouble = "negate_double";

        // Index overloads
        public const string IndexList = "index_list";
        public const string IndexMap = "index_map";
        public const string IndexMessage = "index_message"; // TODO: introduce concept of types.Message

        // In operators
        public const string DeprecatedIn = "in";
        public const string InList = "in_list";
        public const string InMap = "in_map";
        public const string InMessage = "in_message"; // TODO: introduce concept of types.Message

        // Size overloads
        public const string Size = "size";
        public const string SizeString = "size_string";
        public const string SizeBytes = "size_bytes";
        public const string SizeList = "size_list";
        public const string SizeMap = "size_map";
        public const string SizeStringInst = "string_size";
        public const string SizeBytesInst = "bytes_size";
        public const string SizeListInst = "list_size";
        public const string SizeMapInst = "map_size";

        // String function names.
        public const string Contains = "contains";
        public const string EndsWith = "endsWith";
        public const string Matches = "matches";
        public const string StartsWith = "startsWith";

        // String function overload names.
        public const string ContainsString = "contains_string";
        public const string EndsWithString = "ends_with_string";
        public const string MatchesString = "matches_string";
        public const string StartsWithString = "starts_with_string";

        // Time-based functions.
        public const string TimeGetFullYear = "getFullYear";
        public const string TimeGetMonth = "getMonth";
        public const string TimeGetDayOfYear = "getDayOfYear";
        public const string TimeGetDate = "getDate";
        public const string TimeGetDayOfMonth = "getDayOfMonth";
        public const string TimeGetDayOfWeek = "getDayOfWeek";
        public const string TimeGetHours = "getHours";
        public const string TimeGetMinutes = "getMinutes";
        public const string TimeGetSeconds = "getSeconds";
        public const string TimeGetMilliseconds = "getMilliseconds";

        // Timestamp overloads for time functions without timezones.
        public const string TimestampToYear = "timestamp_to_year";
        public const string TimestampToMonth = "timestamp_to_month";
        public const string TimestampToDayOfYear = "timestamp_to_day_of_year";
        public const string TimestampToDayOfMonthZeroBased = "timestamp_to_day_of_month";
        public const string TimestampToDayOfMonthOneBased = "timestamp_to_day_of_month_1_based";
        public const string TimestampToDayOfWeek = "timestamp_to_day_of_week";
        public const string TimestampToHours = "timestamp_to_hours";
        public const string TimestampToMinutes = "timestamp_to_minutes";
        public const string TimestampToSeconds = "timestamp_to_seconds";
        public const string TimestampToMilliseconds = "timestamp_to_milliseconds";

        // Timestamp overloads for time functions with timezones.
        public const string TimestampToYearWithTz = "timestamp_to_year_with_tz";
        public const string TimestampToMonthWithTz = "timestamp_to_month_with_tz";
        public const string TimestampToDayOfYearWithTz = "timestamp_to_day_of_year_with_tz";
        public const string TimestampToDayOfMonthZeroBasedWithTz = "timestamp_to_day_of_month_with_tz";
        public const string TimestampToDayOfMonthOneBasedWithTz = "timestamp_to_day_of_month_1_based_with_tz";
        public const string TimestampToDayOfWeekWithTz = "timestamp_to_day_of_week_with_tz";
        public const string TimestampToHoursWithTz = "timestamp_to_hours_with_tz";
        public const string TimestampToMinutesWithTz = "timestamp_to_minutes_with_tz";
        public const string TimestampToSecondsWithTz = "timestamp_to_seconds_tz";
        public const string TimestampToMillisecondsWithTz = "timestamp_to_milliseconds_with_tz";

        // Duration overloads for time functions.
        public const string DurationToHours = "duration_to_hours";
        public const string DurationToMinutes = "duration_to_minutes";
        public const string DurationToSeconds = "duration_to_seconds";
        public const string DurationToMilliseconds = "duration_to_milliseconds";

        // Type conversion methods and overloads
        public const string TypeConvertInt = "int";
        public const string TypeConvertUint = "uint";
        public const string TypeConvertDouble = "double";
        public const string TypeConvertBool = "bool";
        public const string TypeConvertString = "string";
        public const string TypeConvertBytes = "bytes";
        public const string TypeConvertTimestamp = "timestamp";
        public const string TypeConvertDuration = "duration";
        public const string TypeConvertType = "type";
        public const string TypeConvertDyn = "dyn";

        // Int conversion functions.
        public const string IntToInt = "int64_to_int64";
        public const string UintToInt = "uint64_to_int64";
        public const string DoubleToInt = "double_to_int64";
        public const string StringToInt = "string_to_int64";
        public const string TimestampToInt = "timestamp_to_int64";
        public const string DurationToInt = "duration_to_int64";

        // Uint conversion functions.
        public const string UintToUint = "uint64_to_uint64";
        public const string IntToUint = "int64_to_uint64";
        public const string DoubleToUint = "double_to_uint64";
        public const string StringToUint = "string_to_uint64";

        // Double conversion functions.
        public const string DoubleToDouble = "double_to_double";
        public const string IntToDouble = "int64_to_double";
        public const string UintToDouble = "uint64_to_double";
        public const string StringToDouble = "string_to_double";

        // Bool conversion functions.
        public const string BoolToBool = "bool_to_bool";
        public const string StringToBool = "string_to_bool";

        // Bytes conversion functions.
        public const string BytesToBytes = "bytes_to_bytes";
        public const string StringToBytes = "string_to_bytes";

        // String conversion functions.
        public const string StringToString = "string_to_string";
        public const string BoolToString = "bool_to_string";
        public const string IntToString = "int64_to_string";
        public const string UintToString = "uint64_to_string";
        public const string DoubleToString = "double_to_string";
        public const string BytesToString = "bytes_to_string";
        public const string TimestampToString = "timestamp_to_string";
        public const string DurationToString = "duration_to_string";

        // Timestamp conversion functions
        public const string TimestampToTimestamp = "timestamp_to_timestamp";
        public const string StringToTimestamp = "string_to_timestamp";
        public const string IntToTimestamp = "int64_to_timestamp";

        // Convert duration from string
        public const string DurationToDuration = "duration_to_duration";
        public const string StringToDuration = "string_to_duration";
        public const string IntToDuration = "int64_to_duration";

        // Convert to dyn
        public const string ToDyn = "to_dyn";

        // Comprehensions helper methods, not directly accessible via a developer.
        public const string Iterator = "@iterator";
        public const string HasNext = "@hasNext";
        public const string Next = "@next";

        // IsTypeConversionFunction returns whether the input function is a standard library type
        // conversion function.
        public static bool IsTypeConversionFunction(string function)
        {
            switch (function)
            {
                case TypeConvertBool:
                case TypeConvertBytes:
                case TypeConvertDouble:
                case TypeConvertDuration:
                case TypeConvertDyn:
                case TypeConvertInt:
                case TypeConvertString:
                case TypeConvertTimestamp:
                case TypeConvertType:
                case TypeConvertUint:
                    return true;
                default:
                    return false;
            }
        }
    }
}