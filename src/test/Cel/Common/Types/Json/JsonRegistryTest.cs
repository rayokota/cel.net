using System.Collections;
using System.Collections.Generic;
using Cel.Common.Types;
using Cel.Common.Types.Json;
using Cel.Common.Types.Json.Types;
using Cel.Common.Types.Ref;
using Google.Protobuf.WellKnownTypes;
using NodaTime;
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
namespace Cel.Types.Json
{
	internal class JsonRegistryTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nessieBranch()
	  internal virtual void NessieBranch()
	  {
		TypeRegistry reg = JsonRegistry.NewRegistry();

		RefVariantB refVariantB = new RefVariantB{Name = "main", Hash = "cafebabe123412341234123412341234"};

		Val branchVal = reg.ToTypeAdapter()(refVariantB);
		Assert.That(branchVal, Is.InstanceOf(typeof(ObjectT)));
		Assert.That(branchVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		Assert.That(branchVal.Type().TypeName(), Is.EqualTo(refVariantB.GetType().FullName));

		ObjectT branchObj = (ObjectT) branchVal;
		Assert.That(branchObj.IsSet(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(branchObj.IsSet(StringT.StringOf("name")), Is.EqualTo(BoolT.True));
		Assert.That(branchObj.IsSet(StringT.StringOf("hash")), Is.EqualTo(BoolT.True));
		Assert.That(branchObj.Get(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(branchObj.Get(StringT.StringOf("name")), Is.EqualTo(StringT.StringOf("main")));
		Assert.That(branchObj.Get(StringT.StringOf("hash")), Is.EqualTo(StringT.StringOf("cafebabe123412341234123412341234")));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nessieCommitMetaFull()
	  internal virtual void NessieCommitMetaFull()
	  {
		TypeRegistry reg = JsonRegistry.NewRegistry();

		Instant now = new Instant();
		Instant nowMinus5 = now.Minus(Period.FromMinutes(5).ToDuration());

		IDictionary<string, string> props = new Dictionary<string, string>();
		props["prop-1"] = "value-1";
		props["prop-2"] = "value-2";
		MetaTest cm = new MetaTest()
		{
			CommitTime = now, AuthorTime = nowMinus5, Committer = "committer@projectnessie.org",
			Author = "author@projectnessie.org", Hash = "beeffeed123412341234123412341234", Message = "Feed of beef",
			SignedOffBy = "signed-off@projectnessie.org", Properties = props
		};
		Val cmVal = reg.ToTypeAdapter()(cm);
		Assert.That(cmVal, Is.InstanceOf(typeof(ObjectT)));
		Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		Assert.That(cmVal.Type().TypeName(), Is.EqualTo(cm.GetType().FullName));
		Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
		ObjectT cmObj = (ObjectT) cmVal;
		Assert.That(cmObj.IsSet(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(cmObj.IsSet(StringT.StringOf("commitTime")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("authorTime")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("committer")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("author")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("hash")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("message")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("signedOffBy")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("properties")), Is.EqualTo(BoolT.True));
		IDictionary expectMap = new Dictionary<string, string>();
		expectMap["prop-1"] = "value-1";
		expectMap["prop-2"] = "value-2";
		Assert.That(cmObj.Get(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(cmObj.Get(StringT.StringOf("commitTime")), Is.EqualTo(TimestampT.TimestampOf(now)));
		Assert.That(cmObj.Get(StringT.StringOf("authorTime")), Is.EqualTo(TimestampT.TimestampOf(nowMinus5)));
		Assert.That(cmObj.Get(StringT.StringOf("committer")), Is.EqualTo(StringT.StringOf("committer@projectnessie.org")));
		Assert.That(cmObj.Get(StringT.StringOf("author")), Is.EqualTo(StringT.StringOf("author@projectnessie.org")));
		Assert.That(cmObj.Get(StringT.StringOf("hash")), Is.EqualTo(StringT.StringOf("beeffeed123412341234123412341234")));
		Assert.That(cmObj.Get(StringT.StringOf("message")), Is.EqualTo(StringT.StringOf("Feed of beef")));
		Assert.That(cmObj.Get(StringT.StringOf("signedOffBy")), Is.EqualTo(StringT.StringOf("signed-off@projectnessie.org")));
		Assert.That(cmObj.Get(StringT.StringOf("properties")), Is.EqualTo(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), expectMap)));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nessieCommitMetaPart()
	  internal virtual void NessieCommitMetaPart()
	  {
		TypeRegistry reg = JsonRegistry.NewRegistry();

		Instant now = new Instant();

		MetaTest cm = new MetaTest()
		{
			CommitTime = now, Committer = "committer@projectnessie.org", Hash = "beeffeed123412341234123412341234",
			Message = "Feed of beef"
		};
		Val cmVal = reg.ToTypeAdapter()(cm);
		Assert.That(cmVal, Is.InstanceOf(typeof(ObjectT)));
		Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		Assert.That(cmVal.Type().TypeName(), Is.EqualTo(cm.GetType().FullName));
		Assert.That(cmVal.Type().TypeEnum(), Is.SameAs(TypeEnum.Object));
		ObjectT cmObj = (ObjectT) cmVal;
		Assert.That(cmObj.IsSet(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(cmObj.IsSet(StringT.StringOf("commitTime")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("authorTime")), Is.EqualTo(BoolT.False));
		Assert.That(cmObj.IsSet(StringT.StringOf("committer")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("author")), Is.EqualTo(BoolT.False));
		Assert.That(cmObj.IsSet(StringT.StringOf("hash")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("message")), Is.EqualTo(BoolT.True));
		Assert.That(cmObj.IsSet(StringT.StringOf("signedOffBy")), Is.EqualTo(BoolT.False));
		Assert.That(cmObj.IsSet(StringT.StringOf("properties")), Is.EqualTo(BoolT.True)); // just empty
		Assert.That(cmObj.Get(StringT.StringOf("foo")), Is.InstanceOf(typeof(Err)));
		Assert.That(cmObj.Get(StringT.StringOf("commitTime")), Is.EqualTo(TimestampT.TimestampOf(now)));
		Assert.That(cmObj.Get(StringT.StringOf("authorTime")), Is.EqualTo(NullT.NullValue));
		Assert.That(cmObj.Get(StringT.StringOf("committer")), Is.EqualTo(StringT.StringOf("committer@projectnessie.org")));
		Assert.That(cmObj.Get(StringT.StringOf("author")), Is.EqualTo(NullT.NullValue));
		Assert.That(cmObj.Get(StringT.StringOf("hash")), Is.EqualTo(StringT.StringOf("beeffeed123412341234123412341234")));
		Assert.That(cmObj.Get(StringT.StringOf("message")), Is.EqualTo(StringT.StringOf("Feed of beef")));
		Assert.That(cmObj.Get(StringT.StringOf("signedOffBy")), Is.EqualTo(NullT.NullValue));
		Assert.That(cmObj.Get(StringT.StringOf("properties")), Is.EqualTo(MapT.NewMaybeWrappedMap(reg.ToTypeAdapter(), new Hashtable())));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void copy()
	  internal virtual void Copy()
	  {
		TypeRegistry reg = JsonRegistry.NewRegistry();
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		Assert.That(reg.ToTypeAdapter(), Is.SameAs(reg.ToTypeAdapter()));
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void registerType()
	  internal virtual void RegisterType()
	  {
		TypeRegistry reg = JsonRegistry.NewRegistry();
		Assert.That(() => reg.RegisterType(IntT.IntType), Throws.Exception.InstanceOf(typeof(System.NotSupportedException)));
	  }
	}

}