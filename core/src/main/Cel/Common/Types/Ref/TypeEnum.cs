using System.Collections.Generic;

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
namespace Cel.Common.Types.Ref
{
    public sealed class TypeEnum
    {
        public static readonly TypeEnum Bool = new TypeEnum(InnerEnum.Bool, "bool");
        public static readonly TypeEnum Bytes = new TypeEnum(InnerEnum.Bytes, "bytes");
        public static readonly TypeEnum Double = new TypeEnum(InnerEnum.Double, "double");
        public static readonly TypeEnum Duration = new TypeEnum(InnerEnum.Duration, "google.protobuf.Duration");
        public static readonly TypeEnum Err = new TypeEnum(InnerEnum.Err, "error");
        public static readonly TypeEnum Int = new TypeEnum(InnerEnum.Int, "int");
        public static readonly TypeEnum List = new TypeEnum(InnerEnum.List, "list");
        public static readonly TypeEnum Map = new TypeEnum(InnerEnum.Map, "map");
        public static readonly TypeEnum Null = new TypeEnum(InnerEnum.Null, "null_type");
        public static readonly TypeEnum Object = new TypeEnum(InnerEnum.Object, "object");
        public static readonly TypeEnum String = new TypeEnum(InnerEnum.String, "string");
        public static readonly TypeEnum Timestamp = new TypeEnum(InnerEnum.Timestamp, "google.protobuf.Timestamp");
        public static readonly TypeEnum Type = new TypeEnum(InnerEnum.Type, "type");
        public static readonly TypeEnum Uint = new TypeEnum(InnerEnum.Uint, "uint");
        public static readonly TypeEnum Unknown = new TypeEnum(InnerEnum.Unknown, "unknown");

        private static readonly List<TypeEnum> valueList = new List<TypeEnum>();

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

        public readonly InnerEnum InnerEnumValue;
        private readonly int ordinalValue;
        private static int nextOrdinal = 0;

        private readonly string name;

        internal TypeEnum(InnerEnum innerEnum, string name)
        {
            this.name = name;
            ordinalValue = nextOrdinal++;
            InnerEnumValue = innerEnum;
        }

        public string Name
        {
            get { return name; }
        }

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
            return name;
        }

        public static TypeEnum ValueOf(string name)
        {
            foreach (TypeEnum enumInstance in TypeEnum.valueList)
            {
                if (enumInstance.name == name)
                {
                    return enumInstance;
                }
            }

            throw new System.ArgumentException(name);
        }
    }
}