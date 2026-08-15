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

using Cel.Common.Types.Pb;
using Cel.Common.Types.Ref;
using NUnit.Framework;

namespace Cel.Common.Types;

public class MapTest
{
    private static readonly TypeAdapter Adapter = DefaultTypeAdapter.Instance.ToTypeAdapter();

    private static IVal MapOf(params (IVal key, IVal val)[] entries)
    {
        IDictionary<IVal, IVal> m = new Dictionary<IVal, IVal>();
        foreach (var (key, val) in entries) m[key] = val;
        return MapT.NewWrappedMap(Adapter, m);
    }

    /// <summary>
    ///     Numeric map keys are normalized on lookup (matching cel-go/cel-cpp/cel-java): an int,
    ///     uint or lossless-integral double index matches a lossless-equal key of the other integer
    ///     type, while a fractional double index matches nothing.
    /// </summary>
    [Test]
    public virtual void NumericKeyNormalization()
    {
        var intKeyed = MapOf((IntT.IntOf(1), IntT.IntOf(42)));
        Assert.That(((MapT)intKeyed).Contains(UintT.UintOf(1)), Is.SameAs(BoolT.True));
        Assert.That(((MapT)intKeyed).Contains(DoubleT.DoubleOf(1)), Is.SameAs(BoolT.True));
        Assert.That(((MapT)intKeyed).Get(UintT.UintOf(1)).Equal(IntT.IntOf(42)), Is.SameAs(BoolT.True));
        Assert.That(((MapT)intKeyed).Find(DoubleT.DoubleOf(1))!.Equal(IntT.IntOf(42)), Is.SameAs(BoolT.True));

        var uintKeyed = MapOf((UintT.UintOf(1), IntT.IntOf(42)));
        Assert.That(((MapT)uintKeyed).Contains(IntT.IntOf(1)), Is.SameAs(BoolT.True));

        // A uint key above long.MaxValue (2^63) is reachable only via the double->uint fallback.
        var bigUintKeyed = MapOf((UintT.UintOf(9223372036854775808UL), IntT.IntOf(42)));
        Assert.That(((MapT)bigUintKeyed).Contains(DoubleT.DoubleOf(9223372036854775808.0)), Is.SameAs(BoolT.True));

        // A fractional index is not a lossless integer, so it matches no integer key.
        Assert.That(((MapT)intKeyed).Contains(DoubleT.DoubleOf(1.5)), Is.SameAs(BoolT.False));
    }

    /// <summary>
    ///     When a value on the right-hand map is an error, map equality propagates that same error
    ///     rather than the left value.
    /// </summary>
    [Test]
    public virtual void EqualPropagatesRightHandValueError()
    {
        var boom = Err.NewErr("boom");
        var left = MapOf((IntT.IntOf(1), IntT.IntOf(2)));
        var right = MapOf((IntT.IntOf(1), boom));
        Assert.That(left.Equal(right), Is.SameAs(boom));
    }
}
