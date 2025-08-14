using Cel.Common.Types.Ref;
using Cel.Parser;
using Google.Api.Expr.V1Alpha1;

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
namespace Cel;

public static class EnvOptions
{
    // These constants beginning with "Feature" enable optional behavior in
    // the library.  See the documentation for each constant to see its
    // effects, compatibility restrictions, and standard conformance.

    public static EnvOption ClearMacros()
    {
        return e =>
        {
            e.macros.Clear();
            return e;
        };
    }

    public static EnvOption CustomTypeAdapter(TypeAdapter adapter)
    {
        return e =>
        {
            e.adapter = adapter;
            return e;
        };
    }

    public static EnvOption CustomTypeProvider(ITypeProvider provider)
    {
        return e =>
        {
            e.provider = provider;
            return e;
        };
    }

    public static EnvOption Declarations(IList<Decl> decls)
    {
        return e =>
        {
            ((List<Decl>)e.declarations).AddRange(decls);
            return e;
        };
    }

    public static EnvOption Declarations(params Decl[] decls)
    {
        return Declarations(new List<Decl>(decls));
    }

    public static EnvOption Features(params EnvFeature[] flags)
    {
        return e =>
        {
            foreach (var flag in flags) e.Feature = flag;

            return e;
        };
    }

    public static EnvOption HomogeneousAggregateLiterals()
    {
        return Features(EnvFeature.FeatureDisableDynamicAggregateLiterals);
    }

    public static EnvOption Macros(params Macro[] macros)
    {
        return Macros(new List<Macro>(macros));
    }

    public static EnvOption Macros(IList<Macro> macros)
    {
        return e =>
        {
            ((List<Macro>)e.macros).AddRange(macros);
            return e;
        };
    }

    public static EnvOption Container(string name)
    {
        return e =>
        {
            e.container = e.container.Extend(Common.Containers.Container.Name(name))!;
            return e;
        };
    }

    public static EnvOption Abbrevs(params string[] qualifiedNames)
    {
        return e =>
        {
            e.container = e.container.Extend(Common.Containers.Container.Abbrevs(qualifiedNames))!;
            return e;
        };
    }

    public static EnvOption Types(IList<object> addTypes)
    {
        return e =>
        {
            if (!(e.provider is ITypeRegistry))
                throw new Exception(string.Format("custom types not supported by provider: {0}",
                    e.provider.GetType()));

            var reg = (ITypeRegistry)e.provider;
            foreach (var t in addTypes) reg.Register(t);

            return e;
        };
    }

    public static EnvOption Types(params object[] addTypes)
    {
        return Types(new List<object>(addTypes));
    }
}

public enum EnvFeature
{
    FeatureDisableDynamicAggregateLiterals
}

