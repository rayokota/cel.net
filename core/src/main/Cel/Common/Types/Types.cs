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
namespace Cel.Common.Types
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BytesT.BytesType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.DoubleT.DoubleType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.IntType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.ListT.ListType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.MapT.MapType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.NullT.NullType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.StringType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.TypeT.TypeType;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.UintType;

    using Type = global::Cel.Common.Types.Ref.Type;

    public sealed class Types
    {
        private Types()
        {
        }

        private static readonly IDictionary<string, Type> typeNameToTypeValue = new Dictionary<string, Type>();

        static Types()
        {
            typeNameToTypeValue[BoolT.BoolType.TypeName()] = BoolT.BoolType;
            typeNameToTypeValue[BytesT.BytesType.TypeName()] = BytesT.BytesType;
            typeNameToTypeValue[DoubleT.DoubleType.TypeName()] = DoubleT.DoubleType;
            typeNameToTypeValue[NullT.NullType.TypeName()] = NullT.NullType;
            typeNameToTypeValue[IntT.IntType.TypeName()] = IntT.IntType;
            typeNameToTypeValue[ListT.ListType.TypeName()] = ListT.ListType;
            typeNameToTypeValue[MapT.MapType.TypeName()] = MapT.MapType;
            typeNameToTypeValue[StringT.StringType.TypeName()] = StringT.StringType;
            typeNameToTypeValue[TypeT.TypeType.TypeName()] = TypeT.TypeType;
            typeNameToTypeValue[UintT.UintType.TypeName()] = UintT.UintType;
        }

        public static Type GetTypeByName(string typeName)
        {
            return null;
        }

        public static BoolT BoolOf(bool b)
        {
            return b ? BoolT.True : BoolT.False;
        }
    }
}