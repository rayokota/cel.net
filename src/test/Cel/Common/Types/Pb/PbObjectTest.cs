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

using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;

namespace Cel.Common.Types.Pb;

using Message = IMessage;

public class PbObjectTest
{
    [Test]
    public virtual void NewProtoObject()
    {
        var reg = ProtoTypeRegistry.NewRegistry();
        var info = new SourceInfo();
        info.LineOffsets.Add(new List<int> { 1, 2, 3 });
        var parsedExpr = new ParsedExpr();
        parsedExpr.SourceInfo = info;
        // NOTE: had to add expr
        var e = new Expr();
        e.CallExpr = new Expr.Types.Call();
        parsedExpr.Expr = e;
        reg.RegisterMessage(parsedExpr);
        var obj = (Indexer)reg.NativeToValue(parsedExpr);
        var si = (Indexer)obj.Get(StringT.StringOf("source_info"));
        var lo = (Indexer)si.Get(StringT.StringOf("line_offsets"));
        Assert.That(lo.Get(IntT.IntOf(2)).Equal(IntT.IntOf(3)), Is.SameAs(BoolT.True));

        var expr = (Indexer)obj.Get(StringT.StringOf("expr"));
        var call = (Indexer)expr.Get(StringT.StringOf("call_expr"));
        Assert.That(call.Get(StringT.StringOf("function")).Equal(StringT.StringOf("")), Is.SameAs(BoolT.True));
    }

    [Test]
    public virtual void ProtoObjectConvertToNative()
    {
        TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
        var info = new SourceInfo();
        info.LineOffsets.Add(new List<int> { 1, 2, 3 });
        var msg = new ParsedExpr();
        msg.SourceInfo = info;
        var objVal = reg.ToTypeAdapter()(msg);

        // Proto Message
        var val = (ParsedExpr)objVal.ConvertToNative(typeof(ParsedExpr));
        Assert.That(val, Is.EqualTo(msg));

        // Dynamic protobuf
        var dynPB = reg.NewValue(ParsedExpr.Descriptor.FullName,
            new Dictionary<string, Val> { { "source_info", reg.ToTypeAdapter()(msg.SourceInfo) } });
        var dynVal = reg.ToTypeAdapter()(dynPB);
        val = (ParsedExpr)dynVal.ConvertToNative(msg.GetType());
        Assert.That(val, Is.EqualTo(msg));

        // google.protobuf.Any
        var anyVal = (Any)objVal.ConvertToNative(typeof(Any));
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
        TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
        var info = new SourceInfo();
        info.LineOffsets.Add(new List<int> { 1, 2, 3 });
        var msg = new ParsedExpr();
        msg.SourceInfo = info;

        var obj = reg.ToTypeAdapter()(msg);
        Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
        var objVal = (ObjectT)obj;

        Assert.That(objVal.IsSet(StringT.StringOf("source_info")), Is.SameAs(BoolT.True));
        Assert.That(objVal.IsSet(StringT.StringOf("expr")), Is.SameAs(BoolT.False));
        Assert.That(Err.IsError(objVal.IsSet(StringT.StringOf("bad_field"))), Is.True);
        Assert.That(Err.IsError(objVal.IsSet(IntT.IntZero)), Is.True);
    }

    [Test]
    public virtual void ProtoObjectGet()
    {
        TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
        var info = new SourceInfo();
        info.LineOffsets.Add(new List<int> { 1, 2, 3 });
        var msg = new ParsedExpr();
        msg.SourceInfo = info;
        // NOTE: had to add expr
        msg.Expr = new Expr();

        var obj = reg.ToTypeAdapter()(msg);
        Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
        var objVal = (ObjectT)obj;

        Assert.That(objVal.Get(StringT.StringOf("source_info")).Equal(reg.ToTypeAdapter()(msg.SourceInfo)),
            Is.SameAs(BoolT.True));
        Assert.That(objVal.Get(StringT.StringOf("expr")).Equal(reg.ToTypeAdapter()(new Expr())), Is.SameAs(BoolT.True));
        Assert.That(Err.IsError(objVal.Get(StringT.StringOf("bad_field"))), Is.True);
        Assert.That(Err.IsError(objVal.Get(IntT.IntZero)), Is.True);
    }

    [Test]
    public virtual void ProtoObjectConvertToType()
    {
        TypeRegistry reg = ProtoTypeRegistry.NewRegistry(new Expr());
        var info = new SourceInfo();
        info.LineOffsets.Add(new List<int> { 1, 2, 3 });
        var msg = new ParsedExpr();
        msg.SourceInfo = info;

        var obj = reg.ToTypeAdapter()(msg);
        Assert.That(obj, Is.InstanceOf(typeof(ObjectT)));
        var objVal = (ObjectT)obj;

        var tv = objVal.Type();
        Assert.That(objVal.ConvertToType(TypeT.TypeType).Equal(tv), Is.SameAs(BoolT.True));
        Assert.That(objVal.ConvertToType(objVal.Type()), Is.SameAs(objVal));
    }
}