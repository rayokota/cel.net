using System.Collections.Generic;
using System.Text;
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
namespace Cel.Checker
{
    using Type = Google.Api.Expr.V1Alpha1.Type;

    public sealed class Mapping
    {
        private readonly IDictionary<string, Type> mapping;
        private readonly IDictionary<Type, string> typeKeys;

        private Mapping(IDictionary<string, Type> srcMapping, IDictionary<Type, string> srcTypeKeys)
        {
            // Looks overly complicated, but prevents a bunch of j.u.HashMap.resize() operations.
            // The copy() operation is called very often when a script's being checked, so this saves
            // quite a lot.
            // The formula "* 4 / 3 + 1" prevents the HashMap from resizing, assuming the
            // default-load-factor of .75 (-> 3/4).
            this.mapping = new Dictionary<string, Type>(srcMapping.Count * 4 / 3 + 1);
            foreach (var entry in srcMapping)
            {
                mapping.Add(entry.Key, entry.Value);
            }

            this.typeKeys = new Dictionary<Type, string>(srcTypeKeys.Count * 4 / 3 + 1);
            foreach (var entry in srcTypeKeys)
            {
                typeKeys.Add(entry.Key, entry.Value);
            }
        }

        internal static Mapping NewMapping()
        {
            return new Mapping(new Dictionary<string, Type>(), new Dictionary<Type, string>());
        }

        private string KeyForType(Type t)
        {
            // The lookup by `Type` called very often when a script's being checked, so this saves
            // quite a lot.
            if (!typeKeys.TryGetValue(t, out string? value))
            {
                value = Types.TypeKey(t);
                typeKeys.Add(t, value);
            }

            return value;
        }

        internal void Add(Type from, Type to)
        {
            mapping[KeyForType(from)] = to;
        }

        internal Type Find(Type from)
        {
            return mapping[KeyForType(from)];
        }

        internal Mapping Copy()
        {
            return new Mapping(mapping, typeKeys);
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder("{");

            foreach (var entry in mapping)
            {
                result.Append(entry.Key).Append(" => ").Append(entry.Value);
            }

            result.Append("}");
            return result.ToString();
        }
    }
}