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
//	import static Cel.Common.Types.Err.noMoreElements;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;

	using BaseVal = Cel.Common.Types.Ref.BaseVal;
	using Type = Cel.Common.Types.Ref.Type;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using Val = Cel.Common.Types.Ref.Val;

	/// <summary>
	/// Iterator permits safe traversal over the contents of an aggregate type. </summary>
	public interface IteratorT : Val
	{

	  static IteratorT JavaIterator<T1>(TypeAdapter adapter, IEnumerator<T1> iterator)
	  {
		return new IteratorT_JavaIteratorT(adapter, iterator);
	  }

	  /// <summary>
	  /// HasNext returns true if there are unvisited elements in the Iterator. </summary>
	  Val HasNext();
	  /// <summary>
	  /// Next returns the next element. </summary>
	  Val Next();
	}

	  public sealed class IteratorT_JavaIteratorT : BaseVal, IteratorT
	  {
	internal readonly TypeAdapter adapter;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: final java.util.Iterator<?> iterator;
	internal readonly IEnumerator<object> iterator;

//JAVA TO C# CONVERTER TODO TASK: Wildcard generics in constructor parameters are not converted. Move the generic type parameter and constraint to the class header:
//ORIGINAL LINE: IteratorT_JavaIteratorT(Cel.Common.Types.ref.TypeAdapter adapter, java.util.Iterator<?> iterator)
	internal IteratorT_JavaIteratorT(TypeAdapter adapter, IEnumerator<T1> iterator)
	{
	  this.adapter = adapter;
	  this.iterator = iterator;
	}

	public Val HasNext()
	{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
	  return boolOf(iterator.hasNext());
	}

	public Val Next()
	{
	  object n;
	  try
	  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
		n = iterator.next();
	  }
	  catch (NoSuchElementException)
	  {
		return noMoreElements();
	  }
	  if (n is Val)
	  {
		return (Val) n;
	  }
	  return adapter.NativeToValue(n);
	}

	public override object? ConvertToNative(System.Type typeDesc)
	{
	  throw new System.NotSupportedException();
	}

	public override Val ConvertToType(Type typeValue)
	{
	  throw new System.NotSupportedException();
	}

	public override Val Equal(Val other)
	{
	  throw new System.NotSupportedException();
	}

	public override Type Type()
	{
	  throw new System.NotSupportedException();
	}

	public override object Value()
	{
	  throw new System.NotSupportedException();
	}
	  }

}