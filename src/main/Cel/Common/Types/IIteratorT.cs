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

/// <summary>
///     Iterator permits safe traversal over the contents of an aggregate type.
/// </summary>
public interface IIteratorT : IVal
{
    static IIteratorT Iterator<T1>(TypeAdapter adapter, IEnumerator<T1> iterator)
    {
        return new IteratorAdapter<T1>(adapter, iterator);
    }

    /// <summary>
    ///     HasNext returns true if there are unvisited elements in the Iterator.
    /// </summary>
    IVal HasNext();

    /// <summary>
    ///     Next returns the next element.
    /// </summary>
    IVal Next();
}

internal class IteratorAdapter<T> : BaseVal, IIteratorT
{
    private readonly TypeAdapter adapter;
    private readonly IEnumerator<T> iterator;

    private bool? hasNext;

    public IteratorAdapter(TypeAdapter adapter, IEnumerator<T> iterator)
    {
        this.adapter = adapter;
        this.iterator = iterator;
    }

    public IVal HasNext()
    {
        if (hasNext == null)
            // we have no idea if there's a next element or not
            // we have to call MoveNext and remember its result
            hasNext = iterator.MoveNext();

        return Types.BoolOf(hasNext.Value);
    }

    public IVal Next()
    {
        // call HasNext, it will call MoveNext if needed
        if (!HasNext().BooleanValue())
            throw new InvalidOperationException();

        // we have to clear hasNext so next time it is called MoveNext is also called
        hasNext = null;

        object? val = iterator.Current;
        if (val is IVal) return (IVal)val;

        return adapter(val);
    }

    public override object? ConvertToNative(Type typeDesc)
    {
        throw new NotSupportedException();
    }

    public override IVal ConvertToType(IType typeValue)
    {
        throw new NotSupportedException();
    }

    public override IVal Equal(IVal other)
    {
        throw new NotSupportedException();
    }

    public override IType Type()
    {
        throw new NotSupportedException();
    }

    public override object Value()
    {
        throw new NotSupportedException();
    }
}