using Cel.Common.Types.Ref;

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
namespace Cel.Common.Types;

public sealed class Types
{
    private static readonly IDictionary<string, IType> typeNameToTypeValue = new Dictionary<string, IType>();

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

    private Types()
    {
    }

    public static IType GetTypeByName(string typeName)
    {
        return null;
    }

    public static BoolT BoolOf(bool b)
    {
        return b ? BoolT.True : BoolT.False;
    }
}