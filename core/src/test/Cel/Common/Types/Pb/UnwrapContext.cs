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

using Google.Api.Expr.Test.V1.Proto3;
using NUnit.Framework;

namespace Cel.Common.Types.Pb;

/// <summary>
///     Required by <seealso cref="UnwrapTestCase" /> et al.
/// </summary>
internal class UnwrapContext
{
    private static UnwrapContext instance;
    internal readonly PbTypeDescription msgDesc;

    internal readonly Db pbdb;

    internal UnwrapContext()
    {
        pbdb = Db.NewDb();
        pbdb.RegisterMessage(new TestAllTypes());
        var msgType = "google.protobuf.Value";
        msgDesc = pbdb.DescribeType(msgType);
        Assert.That(msgDesc, Is.Not.Null);
    }

    internal static UnwrapContext Get()
    {
        lock (typeof(UnwrapContext))
        {
            if (instance == null) instance = new UnwrapContext();
            return instance;
        }
    }
}