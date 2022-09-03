using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Pb;
using NUnit.Framework;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Checker;

public class CheckerEnvTest
{
    [Test]
    public virtual void OverlappingIdentifier()
    {
        var env = CheckerEnv.NewStandardCheckerEnv(Container.DefaultContainer, ProtoTypeRegistry.NewRegistry());
        Assert.That(() => env.Add(Decls.NewVar("int", Decls.NewTypeType(null))),
            Throws.Exception.TypeOf<ArgumentException>());
    }

    [Test]
    public virtual void OverlappingMacro()
    {
        var env = CheckerEnv.NewStandardCheckerEnv(Container.DefaultContainer, ProtoTypeRegistry.NewRegistry());
        Assert.That(
            () => env.Add(Decls.NewFunction("has",
                Decls.NewOverload("has", new List<Type> { Decls.String }, Decls.Bool))),
            Throws.Exception.TypeOf<ArgumentException>());
    }

    [Test]
    public virtual void OverlappingOverload()
    {
        var env = CheckerEnv.NewStandardCheckerEnv(Container.DefaultContainer, ProtoTypeRegistry.NewRegistry());
        var paramA = Decls.NewTypeParamType("A");
        IList<string> typeParamAList = new List<string> { "A" };
        Assert.That(
            () => env.Add(Decls.NewFunction(Overloads.TypeConvertDyn,
                Decls.NewParameterizedOverload(Overloads.ToDyn, new List<Type> { paramA }, Decls.Dyn, typeParamAList))),
            Throws.Exception.TypeOf<ArgumentException>());
    }
}