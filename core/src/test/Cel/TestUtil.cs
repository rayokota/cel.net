using System.Collections;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using NUnit.Framework;

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

public class TestUtil
{
    public static IDictionary<object, object> MapOf(object k, object v, params object[] kvPairs)
    {
        IDictionary<object, object> map = new Dictionary<object, object>();
        Assert.That(kvPairs.Length % 2, Is.EqualTo(0));
        map[k] = v;
        for (var i = 0; i < kvPairs.Length; i += 2) map[kvPairs[i]] = kvPairs[i + 1];
        return map;
    }

    public static IDictionary<object, object> MapOf()
    {
        return new Dictionary<object, object>();
    }

    public static void DeepEquals(string context, object a, object b)
    {
        if (a == null && b == null) return;
        if (a == null) Assert.Fail("deepEquals({0}), a==null, b!=null", context);
        if (b == null) Assert.Fail("deepEquals({0}), a!=null, b==null", context);
        if (!a.GetType().IsAssignableFrom(b.GetType()))
        {
            if (a is Val && !(b is Val))
            {
                DeepEquals(context, ((Val)a).Value(), b);
                return;
            }

            if (!(a is Val) && b is Val)
            {
                DeepEquals(context, a, ((Val)b).Value());
                return;
            }

            Assert.Fail("deepEquals({0}), a.class({1}) is not assignable from b.class({2})", context,
                a.GetType().FullName, b.GetType().FullName);
        }

        if (a.GetType().IsArray)
        {
            var aa = (object[])a;
            var ba = (object[])b;
            var al = aa.Length;
            var bl = ba.Length;
            if (al != bl)
                Assert.Fail("deepEquals({0}), {1}.length({2:D}) != {3}.length({4:D})", context, a.GetType().FullName,
                    al, b.GetType().FullName, bl);
            for (var i = 0; i < al; i++)
            {
                var av = aa[i];
                var bv = ba[i];
                DeepEquals(context + '[' + i + ']', av, bv);
            }
        }
        else if (a is IList)
        {
            var al = (IList)a;
            var bl = (IList)b;
            var @as = al.Count;
            var bs = bl.Count;
            if (@as != bs)
                Assert.Fail("deepEquals({0}), {1}.size({2:D}) != {3}.size({4:D})", context, a.GetType().FullName, @as,
                    b.GetType().FullName, bs);
            for (var i = 0; i < @as; i++) DeepEquals(context + '[' + i + ']', al[i], bl[i]);
        }
        else if (a is IDictionary)
        {
            var am = (IDictionary)a;
            var bm = (IDictionary)b;
            var @as = am.Count;
            var bs = bm.Count;
            if (@as != bs)
                Assert.Fail("deepEquals({0}), {1}.size({2:D}) != {3}.size({4:D})", context, a.GetType().FullName, @as,
                    b.GetType().FullName, bs);
            foreach (var k in am.Keys)
            {
                var ak = k;
                var av = am[ak];
                if (!bm.Contains(ak))
                {
                    var f = true;
                    if (ak is Val)
                    {
                        var an = ((Val)ak).Value();
                        if (bm.Contains(an))
                        {
                            ak = an;
                            f = false;
                        }
                    }
                    else
                    {
                        var aval = DefaultTypeAdapter.Instance.NativeToValue(ak);
                        if (bm.Contains(aval))
                        {
                            ak = aval;
                            f = false;
                        }
                    }

                    if (f)
                        Assert.Fail("deepEquals({0}), {1}({2}) contains {3}, but {4}({5}) does not", context,
                            a.GetType().FullName, @as, ak, b.GetType().FullName, bs);
                }

                var bv = bm[ak];
                DeepEquals(context + '[' + ak + ']', av, bv);
            }
        }
        else if (!a.Equals(b))
        {
            Assert.Fail("deepEquals({0}), {1}({2}) != {3}({4})", context, a.GetType().FullName, a, b.GetType().FullName,
                b);
        }
    }
}