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
    private static readonly IDictionary<string, IType> TypeNameToTypeValue = new Dictionary<string, IType>();

    static Types()
    {
        TypeNameToTypeValue[BoolT.BoolType.TypeName()] = BoolT.BoolType;
        TypeNameToTypeValue[BytesT.BytesType.TypeName()] = BytesT.BytesType;
        TypeNameToTypeValue[DoubleT.DoubleType.TypeName()] = DoubleT.DoubleType;
        TypeNameToTypeValue[NullT.NullType.TypeName()] = NullT.NullType;
        TypeNameToTypeValue[IntT.IntType.TypeName()] = IntT.IntType;
        TypeNameToTypeValue[ListT.ListType.TypeName()] = ListT.ListType;
        TypeNameToTypeValue[MapT.MapType.TypeName()] = MapT.MapType;
        TypeNameToTypeValue[StringT.StringType.TypeName()] = StringT.StringType;
        TypeNameToTypeValue[TypeT.TypeType.TypeName()] = TypeT.TypeType;
        TypeNameToTypeValue[UintT.UintType.TypeName()] = UintT.UintType;
    }

    private Types()
    {
    }

    public static IType? GetTypeByName(string typeName)
    {
        return null;
    }

    public static BoolT BoolOf(bool b)
    {
        return b ? BoolT.True : BoolT.False;
    }
}