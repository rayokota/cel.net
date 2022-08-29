using System.Collections.Generic;

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
namespace Cel.Common.Types
{
    using BaseVal = global::Cel.Common.Types.Ref.BaseVal;
    using Type = global::Cel.Common.Types.Ref.Type;
    using TypeAdapter = global::Cel.Common.Types.Ref.TypeAdapter;
    using Val = global::Cel.Common.Types.Ref.Val;

    /// <summary>
    /// Iterator permits safe traversal over the contents of an aggregate type. </summary>
    public interface IteratorT : Val
    {
        static IteratorT JavaIterator<T1>(TypeAdapter adapter, IEnumerator<T1> iterator)
        {
            return new IteratorAdapter<T1>(adapter, iterator);
        }

        /// <summary>
        /// HasNext returns true if there are unvisited elements in the Iterator. </summary>
        Val HasNext();

        /// <summary>
        /// Next returns the next element. </summary>
        Val Next();
    }

    class IteratorAdapter<T> : BaseVal, IteratorT
    {
        private readonly TypeAdapter adapter;
        private readonly IEnumerator<T> iterator;

        private bool? hasNext;

        public IteratorAdapter(TypeAdapter adapter, IEnumerator<T> iterator)
        {
            this.adapter = adapter;
            this.iterator = iterator;
        }

        public Val HasNext()
        {
            if (hasNext == null)
            {
                // we have no idea if there's a next element or not
                // we have to call MoveNext and remember its result
                hasNext = iterator.MoveNext();
            }

            return Types.BoolOf(hasNext.Value);
        }

        public Val Next()
        {
            // call HasNext, it will call MoveNext if needed
            if (!hasNext.Value)
                throw new InvalidOperationException();

            // we have to clear hasNext so next time it is called MoveNext is also called
            hasNext = null;

            object val = iterator.Current;
            if (val is Val)
            {
                return (Val)val;
            }

            return adapter(val);
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