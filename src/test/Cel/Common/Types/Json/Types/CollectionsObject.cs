using System.Collections.Generic;
using NodaTime;

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
namespace Cel.Common.Types.Json.Types
{

	using ByteString = Google.Protobuf.ByteString;
	using Duration = Google.Protobuf.WellKnownTypes.Duration;
	using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;

	public class CollectionsObject
	{
	  public IDictionary<string, bool> stringBooleanMap;
	  public IDictionary<byte, short> byteShortMap;
	  public IDictionary<int, long> intLongMap;
	  public IDictionary<ulong, Timestamp> ulongTimestampMap;
	  public IDictionary<ulong, ZonedDateTime> ulongZonedDateTimeMap;
	  public IDictionary<string, Duration> stringProtoDurationMap;
	  public IDictionary<string, Period> stringPeriodMap;
	  public IDictionary<string, ByteString> stringBytesMap;
	  public IDictionary<float, double> floatDoubleMap;

	  public IList<string> stringList;
	  public IList<bool> booleanList;
	  public IList<byte> byteList;
	  public IList<short> shortList;
	  public IList<int> intList;
	  public IList<long> longList;
	  public IList<ulong> ulongList;
	  public IList<Timestamp> timestampList;
	  public IList<ZonedDateTime> zonedDateTimeList;
	  public IList<Duration> durationList;
	  public IList<Period> periodList;
	  public IList<ByteString> bytesList;
	  public IList<float> floatList;
	  public IList<double> doubleList;

	  public IDictionary<string, InnerType> stringInnerMap;
	  public IList<InnerType> innerTypes;

	  public AnEnum anEnum;
	  public IList<AnEnum> anEnumList;
	  public IDictionary<AnEnum, string> anEnumStringMap;
	  public IDictionary<string, AnEnum> stringAnEnumMap;

	  public static readonly IList<string> ALL_PROPERTIES = new List<string> {"stringBooleanMap", "byteShortMap", "intLongMap", "ulongTimestampMap", "ulongZonedDateTimeMap", "stringProtoDurationMap", "stringPeriodMap", "stringBytesMap", "floatDoubleMap", "stringList", "booleanList", "byteList", "shortList", "intList", "longList", "ulongList", "timestampList", "zonedDateTimeList", "durationList", "periodList", "bytesList", "floatList", "doubleList", "stringInnerMap", "innerTypes", "anEnum", "anEnumList", "anEnumStringMap", "stringAnEnumMap"};
	}

}