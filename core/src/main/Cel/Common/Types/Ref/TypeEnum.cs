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

namespace Cel.Common.Types.Ref;

public sealed class TypeEnum
{
    public enum InnerEnum
    {
        Bool,
        Bytes,
        Double,
        Duration,
        Err,
        Int,
        List,
        Map,
        Null,
        Object,
        String,
        Timestamp,
        Type,
        Uint,
        Unknown
    }

    public static readonly TypeEnum Bool = new(InnerEnum.Bool, "bool");
    public static readonly TypeEnum Bytes = new(InnerEnum.Bytes, "bytes");
    public static readonly TypeEnum Double = new(InnerEnum.Double, "double");
    public static readonly TypeEnum Duration = new(InnerEnum.Duration, "google.protobuf.Duration");
    public static readonly TypeEnum Err = new(InnerEnum.Err, "error");
    public static readonly TypeEnum Int = new(InnerEnum.Int, "int");
    public static readonly TypeEnum List = new(InnerEnum.List, "list");
    public static readonly TypeEnum Map = new(InnerEnum.Map, "map");
    public static readonly TypeEnum Null = new(InnerEnum.Null, "null_type");
    public static readonly TypeEnum Object = new(InnerEnum.Object, "object");
    public static readonly TypeEnum String = new(InnerEnum.String, "string");
    public static readonly TypeEnum Timestamp = new(InnerEnum.Timestamp, "google.protobuf.Timestamp");
    public static readonly TypeEnum Type = new(InnerEnum.Type, "type");
    public static readonly TypeEnum Uint = new(InnerEnum.Uint, "uint");
    public static readonly TypeEnum Unknown = new(InnerEnum.Unknown, "unknown");

    private static readonly List<TypeEnum> valueList = new();
    private static int nextOrdinal;

    public readonly InnerEnum InnerEnumValue;

    private readonly int ordinalValue;

    static TypeEnum()
    {
        valueList.Add(Bool);
        valueList.Add(Bytes);
        valueList.Add(Double);
        valueList.Add(Duration);
        valueList.Add(Err);
        valueList.Add(Int);
        valueList.Add(List);
        valueList.Add(Map);
        valueList.Add(Null);
        valueList.Add(Object);
        valueList.Add(String);
        valueList.Add(Timestamp);
        valueList.Add(Type);
        valueList.Add(Uint);
        valueList.Add(Unknown);
    }

    internal TypeEnum(InnerEnum innerEnum, string name)
    {
        Name = name;
        ordinalValue = nextOrdinal++;
        InnerEnumValue = innerEnum;
    }

    public string Name { get; }

    public static TypeEnum[] Values()
    {
        return valueList.ToArray();
    }

    public int Ordinal()
    {
        return ordinalValue;
    }

    public override string ToString()
    {
        return Name;
    }

    public static TypeEnum ValueOf(string name)
    {
        foreach (var enumInstance in valueList)
            if (enumInstance.Name == name)
                return enumInstance;

        throw new ArgumentException(name);
    }
}