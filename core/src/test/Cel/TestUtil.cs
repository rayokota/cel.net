using System.Collections.Generic;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using NUnit.Framework;

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
namespace Cel
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;

	public class TestUtil
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings({"unchecked"}) public static <K, V> java.util.Map<K, V> mapOf(K k, V v, Object... kvPairs)
	  public static IDictionary<K, V> MapOf<K, V>(K k, V v, params object[] kvPairs)
	  {
		IDictionary<K, V> map = new Dictionary<K, V>();
		Assert.That(kvPairs.Length % 2, Is.EqualTo(0));
		map[k] = v;
		for (int i = 0; i < kvPairs.Length; i += 2)
		{
		  map[(K) kvPairs[i]] = (V) kvPairs[i + 1];
		}
		return map;
	  }

	  public static IDictionary<K, V> MapOf<K, V>()
	  {
		return new Dictionary<K, V>();
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") public static void deepEquals(String context, Object a, Object b)
	  public static void DeepEquals(string context, object a, object b)
	  {
		if (a == null && b == null)
		{
		  return;
		}
		if (a == null)
		{
		  Assert.Fail(string.Format("deepEquals({0}), a==null, b!=null", context));
		}
		if (b == null)
		{
		  Assert.Fail(string.Format("deepEquals({0}), a!=null, b==null", context));
		}
		if (!a.GetType().IsAssignableFrom(b.GetType()))
		{
		  if (a is Val && !(b is Val))
		  {
			DeepEquals(context, ((Val) a).Value(), b);
			return;
		  }
		  if (!(a is Val) && b is Val)
		  {
			DeepEquals(context, a, ((Val) b).Value());
			return;
		  }
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  Assert.Fail(string.Format("deepEquals({0}), a.class({1}) is not assignable from b.class({2})", context, a.GetType().FullName, b.GetType().FullName));
		}
		if (a.GetType().IsArray)
		{
			object[] aa = (object[])a;
			object[] ba = (object[])b;
			int al = aa.Length;
			int bl = ba.Length;
		  if (al != bl)
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			Assert.Fail(string.Format("deepEquals({0}), {1}.length({2:D}) != {3}.length({4:D})", context, a.GetType().FullName, al, b.GetType().FullName, bl));
		  }
		  for (int i = 0; i < al; i++)
		  {
			  object av = aa[i];
			  object bv = ba[i];
			DeepEquals(context + '[' + i + ']', av, bv);
		  }
		}
		else if (a is System.Collections.IList)
		{
		  System.Collections.IList al = (System.Collections.IList) a;
		  System.Collections.IList bl = (System.Collections.IList) b;
		  int @as = al.Count;
		  int bs = bl.Count;
		  if (@as != bs)
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			Assert.Fail(string.Format("deepEquals({0}), {1}.size({2:D}) != {3}.size({4:D})", context, a.GetType().FullName, @as, b.GetType().FullName, bs));
		  }
		  for (int i = 0; i < @as; i++)
		  {
			DeepEquals(context + '[' + i + ']', al[i], bl[i]);
		  }
		}
		else if (a is System.Collections.IDictionary)
		{
		  System.Collections.IDictionary am = (System.Collections.IDictionary) a;
		  System.Collections.IDictionary bm = (System.Collections.IDictionary) b;
		  int @as = am.Count;
		  int bs = bm.Count;
		  if (@as != bs)
		  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			Assert.Fail(string.Format("deepEquals({0}), {1}.size({2:D}) != {3}.size({4:D})", context, a.GetType().FullName, @as, b.GetType().FullName, bs));
		  }
		  foreach (object k in am.Keys)
		  {
		  object ak = k;
			object av = am[ak];
			if (!bm.Contains(ak))
			{
			  bool f = true;
			  if (ak is Val)
			  {
				object an = ((Val) ak).Value();
				if (bm.Contains(an))
				{
				  ak = an;
				  f = false;
				}
			  }
			  else
			  {
				Val aval = DefaultTypeAdapter.Instance.NativeToValue(ak);
				if (bm.Contains(aval))
				{
				  ak = aval;
				  f = false;
				}
			  }
			  if (f)
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				Assert.Fail(string.Format("deepEquals({0}), {1}({2}) contains {3}, but {4}({5}) does not", context, a.GetType().FullName, @as, ak, b.GetType().FullName, bs));
			  }
			}
			object bv = bm[ak];
			DeepEquals(context + '[' + ak + ']', av, bv);
		  }
		}
		else if (!a.Equals(b))
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		  Assert.Fail(string.Format("deepEquals({0}), {1}({2}) != {3}({4})", context, a.GetType().FullName, a, b.GetType().FullName, b));
		}
	  }
	}

}