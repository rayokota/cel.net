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

using Cel.Common.Types;
using Microsoft.VisualBasic.CompilerServices;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

namespace Cel.Common.Types
{
    public class TypeTest
    {
[Test]
        public virtual void TypeConvertToType()
        {
            Ref.Type[] stdTypes = new Ref.Type[]
            {
                BoolT.BoolType, BytesT.BytesType, DoubleT.DoubleType, DurationT.DurationType, 
                IntT.IntType, ListT.ListType, MapT.MapType, NullT.NullType, StringT.StringType,
                TimestampT.TimestampType, TypeT.TypeType, UintT.UintType
            };
            foreach (Ref.Type stdType in stdTypes)
            {
                Ref.Val cnv = stdType.ConvertToType(TypeT.TypeType);
                Assert.That(cnv, Is.EqualTo(TypeT.TypeType));
            }
        }

[Test]
        public virtual void TypeType()
        {
            Assert.That(TypeT.TypeType.Type, Is.SameAs(TypeT.TypeType));
        }
    }
}