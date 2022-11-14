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
using System.Collections;
using Cel.Common.Types;
using Cel.Common.Types.Json;
using Cel.Common.Types.Json.Types;
using Cel.Common.Types.Ref;
using NodaTime;
using NUnit.Framework;

namespace Cel.Types.Json;

internal class JsonRegistryTest
{
    [Test]
    public virtual void NessieBranch()
    {
        var reg = JsonRegistry.NewRegistry();

        var refVariantB = new RefVariantB { Name = "main", Hash = "cafebabe123412341234123412341234" };

        var branchVal = reg.ToTypeAdapter()(refVariantB);
        Assert.That(branchVal, Is.InstanceOf(typeof(ObjectT)));
        Assert.That(branchVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
        Assert.That(branchVal.Type().TypeName(), Is.EqualTo(refVariantB.GetType().FullName));

        var branchObj = (ObjectT)branchVal;
        Assert.That(branchObj.IsSet(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(branchObj.IsSet(StringT.StringOf("Name")), Is.EqualTo(BoolT.True));
        Assert.That(branchObj.IsSet(StringT.StringOf("Hash")), Is.EqualTo(BoolT.True));
        Assert.That(branchObj.Get(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(branchObj.Get(StringT.StringOf("Name")), Is.EqualTo(StringT.StringOf("main")));
        Assert.That(branchObj.Get(StringT.StringOf("Hash")),
            Is.EqualTo(StringT.StringOf("cafebabe123412341234123412341234")));
    }

    [Test]
    public virtual void NessieCommitMetaFull()
    {
        var reg = JsonRegistry.NewRegistry();

        var now = new Instant();
        var nowMinus5 = now.Minus(Period.FromMinutes(5).ToDuration());

        IDictionary<string, string> props = new Dictionary<string, string>();
        props["prop-1"] = "value-1";
        props["prop-2"] = "value-2";
        var cm = new MetaTest
        {
            CommitTime = now, AuthorTime = nowMinus5, Committer = "committer@projectnessie.org",
            Author = "author@projectnessie.org", Hash = "beeffeed123412341234123412341234", Message = "Feed of beef",
            SignedOffBy = "signed-off@projectnessie.org", Properties = props
        };
        var cmVal = reg.ToTypeAdapter()(cm);
        Assert.That(cmVal, Is.InstanceOf(typeof(ObjectT)));
        Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
        Assert.That(cmVal.Type().TypeName(), Is.EqualTo(cm.GetType().FullName));
        Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
        var cmObj = (ObjectT)cmVal;
        Assert.That(cmObj.IsSet(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(cmObj.IsSet(StringT.StringOf("CommitTime")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("AuthorTime")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Committer")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Author")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Hash")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Message")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("SignedOffBy")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Properties")), Is.EqualTo(BoolT.True));
        IDictionary expectMap = new Dictionary<string, string>();
        expectMap["prop-1"] = "value-1";
        expectMap["prop-2"] = "value-2";
        Assert.That(cmObj.Get(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(cmObj.Get(StringT.StringOf("CommitTime")), Is.EqualTo(TimestampT.TimestampOf(now)));
        Assert.That(cmObj.Get(StringT.StringOf("AuthorTime")), Is.EqualTo(TimestampT.TimestampOf(nowMinus5)));
        Assert.That(cmObj.Get(StringT.StringOf("Committer")),
            Is.EqualTo(StringT.StringOf("committer@projectnessie.org")));
        Assert.That(cmObj.Get(StringT.StringOf("Author")), Is.EqualTo(StringT.StringOf("author@projectnessie.org")));
        Assert.That(cmObj.Get(StringT.StringOf("Hash")),
            Is.EqualTo(StringT.StringOf("beeffeed123412341234123412341234")));
        Assert.That(cmObj.Get(StringT.StringOf("Message")), Is.EqualTo(StringT.StringOf("Feed of beef")));
        Assert.That(cmObj.Get(StringT.StringOf("SignedOffBy")),
            Is.EqualTo(StringT.StringOf("signed-off@projectnessie.org")));
        Assert.That(cmObj.Get(StringT.StringOf("Properties")),
            Is.EqualTo(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), expectMap)));
    }

    [Test]
    public virtual void NessieCommitMetaPart()
    {
        var reg = JsonRegistry.NewRegistry();

        var now = new Instant();

        var cm = new MetaTest
        {
            CommitTime = now, Committer = "committer@projectnessie.org", Hash = "beeffeed123412341234123412341234",
            Message = "Feed of beef"
        };
        var cmVal = reg.ToTypeAdapter()(cm);
        Assert.That(cmVal, Is.InstanceOf(typeof(ObjectT)));
        Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
        Assert.That(cmVal.Type().TypeName(), Is.EqualTo(cm.GetType().FullName));
        Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
        var cmObj = (ObjectT)cmVal;
        Assert.That(cmObj.IsSet(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(cmObj.IsSet(StringT.StringOf("CommitTime")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("AuthorTime")), Is.EqualTo(BoolT.False));
        Assert.That(cmObj.IsSet(StringT.StringOf("Committer")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Author")), Is.EqualTo(BoolT.False));
        Assert.That(cmObj.IsSet(StringT.StringOf("Hash")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("Message")), Is.EqualTo(BoolT.True));
        Assert.That(cmObj.IsSet(StringT.StringOf("SignedOffBy")), Is.EqualTo(BoolT.False));
        Assert.That(cmObj.IsSet(StringT.StringOf("Properties")), Is.EqualTo(BoolT.True)); // just empty
        Assert.That(cmObj.Get(StringT.StringOf("Foo")), Is.InstanceOf(typeof(Err)));
        Assert.That(cmObj.Get(StringT.StringOf("CommitTime")), Is.EqualTo(TimestampT.TimestampOf(now)));
        Assert.That(cmObj.Get(StringT.StringOf("AuthorTime")), Is.EqualTo(NullT.NullValue));
        Assert.That(cmObj.Get(StringT.StringOf("Committer")),
            Is.EqualTo(StringT.StringOf("committer@projectnessie.org")));
        Assert.That(cmObj.Get(StringT.StringOf("Author")), Is.EqualTo(NullT.NullValue));
        Assert.That(cmObj.Get(StringT.StringOf("Hash")),
            Is.EqualTo(StringT.StringOf("beeffeed123412341234123412341234")));
        Assert.That(cmObj.Get(StringT.StringOf("Message")), Is.EqualTo(StringT.StringOf("Feed of beef")));
        Assert.That(cmObj.Get(StringT.StringOf("SignedOffBy")), Is.EqualTo(NullT.NullValue));
        Assert.That(cmObj.Get(StringT.StringOf("Properties")),
            Is.EqualTo(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), new Dictionary<string, string>())));
    }

    [Test]
    public virtual void Copy()
    {
        var reg = JsonRegistry.NewRegistry();
        Assert.That(reg.ToTypeAdapter(), Is.EqualTo(reg.ToTypeAdapter()));
    }

    [Test]
    public virtual void RegisterType()
    {
        var reg = JsonRegistry.NewRegistry();
        Assert.That(() => reg.RegisterType(IntT.IntType), Throws.Exception.InstanceOf(typeof(NotSupportedException)));
    }
}