using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
using Duration = Google.Protobuf.WellKnownTypes.Duration;

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
namespace Cel.Common.Types.Json.Types;

public class CollectionsObject
{
    public static readonly IList<string> ALL_PROPERTIES = new List<string>
    {
        "stringBooleanMap", "byteShortMap", "intLongMap", "ulongTimestampMap", "ulongZonedDateTimeMap",
        "stringProtoDurationMap", "stringPeriodMap", "stringBytesMap", "floatDoubleMap", "stringList", "booleanList",
        "byteList", "shortList", "intList", "longList", "ulongList", "timestampList", "zonedDateTimeList",
        "durationList", "periodList", "bytesList", "floatList", "doubleList", "stringInnerMap", "innerTypes", "anEnum",
        "anEnumList", "anEnumStringMap", "stringAnEnumMap"
    };

    public AnEnum? anEnum;
    public IList<AnEnum> anEnumList;
    public IDictionary<AnEnum, string> anEnumStringMap;
    public IList<bool> booleanList;
    public IList<byte> byteList;
    public IDictionary<byte, short> byteShortMap;
    public IList<ByteString> bytesList;
    public IList<double> doubleList;
    public IList<Duration> durationList;
    public IDictionary<float, double> floatDoubleMap;
    public IList<float> floatList;
    public IList<InnerType> innerTypes;
    public IList<int> intList;
    public IDictionary<int, long> intLongMap;
    public IList<long> longList;
    public IList<Period> periodList;
    public IList<short> shortList;
    public IDictionary<string, AnEnum> stringAnEnumMap;
    public IDictionary<string, bool> stringBooleanMap;
    public IDictionary<string, ByteString> stringBytesMap;

    public IDictionary<string, InnerType> stringInnerMap;

    public IList<string> stringList;
    public IDictionary<string, Period> stringPeriodMap;
    public IDictionary<string, Duration> stringProtoDurationMap;
    public IList<Timestamp> timestampList;
    public IList<ulong> ulongList;
    public IDictionary<ulong, Timestamp> ulongTimestampMap;
    public IDictionary<ulong, ZonedDateTime> ulongZonedDateTimeMap;
    public IList<ZonedDateTime> zonedDateTimeList;
}