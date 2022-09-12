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

    private static readonly List<TypeEnum> ValueList = new();
    private static int nextOrdinal;

    private readonly int ordinalValue;

    static TypeEnum()
    {
        ValueList.Add(Bool);
        ValueList.Add(Bytes);
        ValueList.Add(Double);
        ValueList.Add(Duration);
        ValueList.Add(Err);
        ValueList.Add(Int);
        ValueList.Add(List);
        ValueList.Add(Map);
        ValueList.Add(Null);
        ValueList.Add(Object);
        ValueList.Add(String);
        ValueList.Add(Timestamp);
        ValueList.Add(Type);
        ValueList.Add(Uint);
        ValueList.Add(Unknown);
    }

    internal TypeEnum(InnerEnum innerEnum, string name)
    {
        Name = name;
        ordinalValue = nextOrdinal++;
        InnerEnumValue = innerEnum;
    }

    public string Name { get; }

    public InnerEnum InnerEnumValue { get; }
    
    public static TypeEnum[] Values()
    {
        return ValueList.ToArray();
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
        foreach (var enumInstance in ValueList)
            if (enumInstance.Name == name)
                return enumInstance;

        throw new ArgumentException(name);
    }
}