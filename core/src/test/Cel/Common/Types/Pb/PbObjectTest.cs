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

using Cel.Common.Types;
using Cel.Common.Types.Pb;
using Cel.Common.Types.Traits;
using NUnit.Framework;

namespace Cel.Common.Types.Pb
{
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using ParsedExpr = Google.Api.Expr.V1Alpha1.ParsedExpr;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
	using Any = Google.Protobuf.WellKnownTypes.Any;
	using Message = Google.Protobuf.IMessage;

	public class PbObjectTest
	{

[Test]
	  public virtual void NewProtoObject()
	  {
		ProtoTypeRegistry reg = ProtoTypeRegistry.NewRegistry();
		SourceInfo info = new SourceInfo();
		info.LineOffsets.Add(new List<int>{1, 2, 3});
		ParsedExpr parsedExpr = new ParsedExpr();
		parsedExpr.SourceInfo = info;
		// NOTE: had to add expr
		Expr e = new Expr();
		e.CallExpr = new Expr.Types.Call();
		parsedExpr.Expr = e;
		reg.RegisterMessage(parsedExpr);
		Indexer obj = (Indexer) reg.NativeToValue(parsedExpr);
		Indexer si = (Indexer) obj.Get(StringT.StringOf("source_info"));
		Indexer lo = (Indexer) si.Get(StringT.StringOf("line_offsets"));
		Assert.That(lo.Get(IntT.IntOf(2)).Equal(IntT.IntOf(3)), Is.SameAs(BoolT.True));

		Indexer expr = (Indexer) obj.Get(StringT.StringOf("expr"));
		Indexer call = (Indexer) expr.Get(StringT.StringOf("call_expr"));
		Assert.That(call.Get(StringT.StringOf("function")).Equal(StringT.StringOf("")), Is.SameAs(BoolT.True));
	  }

[Test]
	  public virtual void ProtoObjectConvertToNative()
	  {
		  Ref.TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
		SourceInfo info = new SourceInfo();
		info.LineOffsets.Add(new List<int>{1, 2, 3});
		ParsedExpr msg = new ParsedExpr();
			msg.SourceInfo = info;
		Ref.Val objVal = reg.ToTypeAdapter()(msg);

		// Proto Message
		ParsedExpr val = (ParsedExpr) objVal.ConvertToNative(typeof(ParsedExpr));
		Assert.That(val, Is.EqualTo(msg));

		// Dynamic protobuf
		Ref.Val dynPB = reg.NewValue(ParsedExpr.Descriptor.FullName, new Dictionary<string, Ref.Val>{{"source_info", reg.ToTypeAdapter()(msg.SourceInfo)}});
		Ref.Val dynVal = reg.ToTypeAdapter()(dynPB);
		val = (ParsedExpr) dynVal.ConvertToNative(msg.GetType());
		Assert.That(val, Is.EqualTo(msg));

		// google.protobuf.Any
		Any anyVal = (Any) objVal.ConvertToNative(typeof(Any));
		Message unpackedAny = anyVal.Unpack<ParsedExpr>();
		Assert.That(unpackedAny, Is.EqualTo(objVal.Value()));
	  }

	  public virtual void ProtoObjectConvertToNativeJSON()
	  {
		// TODO this is the rest of the above test, the missing JSON part
		//    // JSON
		//    Value jsonVal = objVal.convertToNative(Value.class);
		//    jsonBytes = protojson.Marshal(jsonVal.(proto.Message))
		//    jsonTxt = string(jsonBytes)
		//    outMap := map[string]interface{}{}
		//    err = json.Unmarshal(jsonBytes, &outMap)
		//    want := map[string]interface{}{
		//      "sourceInfo": map[string]interface{}{
		//        "lineOffsets": []interface{}{1.0, 2.0, 3.0},
		//      },
		//    }
		//    if !reflect.DeepEqual(outMap, want) {
		//      t.Errorf("got json '%v', expected %v", outMap, want)
		//    }
	  }

[Test]
	  public virtual void ProtoObjectIsSet()
	  {
		Ref.TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
		SourceInfo info = new SourceInfo();
		info.LineOffsets.Add(new List<int>{1, 2, 3});
		ParsedExpr msg = new ParsedExpr();
		msg.SourceInfo = info;

		Ref.Val obj = reg.ToTypeAdapter()(msg);
		Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
		ObjectT objVal = (ObjectT) obj;

		Assert.That(objVal.IsSet(StringT.StringOf("source_info")), Is.SameAs(BoolT.True));
		Assert.That(objVal.IsSet(StringT.StringOf("expr")), Is.SameAs(BoolT.False));
		Assert.That(Err.IsError(objVal.IsSet(StringT.StringOf("bad_field"))), Is.True);
		Assert.That(Err.IsError(objVal.IsSet(IntT.IntZero)), Is.True);
	  }

[Test]
	  public virtual void ProtoObjectGet()
	  {
		  Ref.TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
		SourceInfo info = new SourceInfo();
		info.LineOffsets.Add(new List<int>{1, 2, 3});
		ParsedExpr msg = new ParsedExpr();
		msg.SourceInfo = info;
		// NOTE: had to add expr
		msg.Expr = new Expr();

		Ref.Val obj = reg.ToTypeAdapter()(msg);
		Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
		ObjectT objVal = (ObjectT) obj;

		Assert.That(objVal.Get(StringT.StringOf("source_info")).Equal(reg.ToTypeAdapter()(msg.SourceInfo)), Is.SameAs(BoolT.True));
		Assert.That(objVal.Get(StringT.StringOf("expr")).Equal(reg.ToTypeAdapter()(new Expr())), Is.SameAs(BoolT.True));
		Assert.That(Err.IsError(objVal.Get(StringT.StringOf("bad_field"))), Is.True);
		Assert.That(Err.IsError(objVal.Get(IntT.IntZero)), Is.True);
	  }

[Test]
	  public virtual void ProtoObjectConvertToType()
	  {
		Ref.TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
		SourceInfo info = new SourceInfo();
		info.LineOffsets.Add(new List<int>{1, 2, 3});
		ParsedExpr msg = new ParsedExpr();
		msg.SourceInfo = info;

		Ref.Val obj = reg.ToTypeAdapter()(msg);
		Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
		ObjectT objVal = (ObjectT) obj;

		Ref.Type tv = objVal.Type();
		Assert.That(objVal.ConvertToType(TypeT.TypeType).Equal(tv), Is.SameAs(BoolT.True));
		Assert.That(objVal.ConvertToType(objVal.Type()), Is.SameAs(objVal));
	  }
	}

}