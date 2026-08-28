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

using Avro;
using Avro.Generic;
using Cel.Checker;
using Cel.Common.Types.Avro;
using Cel.Tools;
using Example.Avro;
using NUnit.Framework;

namespace Cel.Types.Avro;

internal class AvroScriptHostTest
{
    [Test]
    public virtual void Simple()
    {
        ScriptHost scriptHost = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();

        Script script =
            scriptHost
                .BuildScript("user.name == 'foobar' && user.kind == \"TWO\"")
                .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(User._SCHEMA.Fullname)))
                .WithTypes(User._SCHEMA)
                .Build();

        User userMatch = new User { name = "foobar", friends = new List<User>(), kind = Kind.TWO };
        User userNoMatch = new User { name = "foobaz", friends = new List<User>(), kind = Kind.THREE };

        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = userMatch }), Is.True);
        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = userNoMatch }), Is.False);

        RecordSchema recordSchema = (RecordSchema)User._SCHEMA;
        recordSchema.TryGetField("kind", out var field);
        EnumSchema enumSchema = (EnumSchema)field.Schema;

        GenericRecord userMatch2 = new GenericRecord(recordSchema);
        userMatch2.Add("name", "foobar");
        userMatch2.Add("kind", new GenericEnum(enumSchema, "TWO"));

        GenericRecord userNoMatch2 = new GenericRecord(recordSchema);
        userNoMatch2.Add("name", "foobaz");
        userNoMatch2.Add("kind", new GenericEnum(enumSchema, "THREE"));

        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = userMatch2 }), Is.True);
        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = userNoMatch2 }), Is.False);
    }

    // An Avro `decimal` logical-type field decodes to AvroDecimal. Reading it through field
    // access must not throw (it used to fail with "Cannot get schema for Avro.AvroDecimal");
    // the value is carried unchanged so a caller can reconstruct the exact decimal.
    [Test]
    public virtual void DecimalLogicalFieldIsCarried()
    {
        RecordSchema recordSchema = (RecordSchema)Schema.Parse(
            "{\"type\":\"record\",\"name\":\"DecimalRecord\",\"fields\":[" +
            "{\"name\":\"amount\",\"type\":{\"type\":\"bytes\",\"logicalType\":\"decimal\"," +
            "\"precision\":8,\"scale\":2}}]}");

        ScriptHost scriptHost = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();
        Script script =
            scriptHost
                .BuildScript("user.amount")
                .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(recordSchema.Fullname)))
                .WithTypes(recordSchema)
                .Build();

        AvroDecimal amount = new AvroDecimal(12.34m);
        GenericRecord record = new GenericRecord(recordSchema);
        record.Add("amount", amount);

        object result = script.Execute<object>(new Dictionary<string, object> { ["user"] = record });
        Assert.That(result, Is.EqualTo(amount));
    }

    // An Avro `fixed` field is a fixed-width byte string, so it must reach CEL as bytes - the same
    // as `bytes`, and what the Java reference does (GenericFixed -> CelByteString). It used to fall
    // through NativeToValue's arms to the record fallback and become an opaque object, so both
    // size() and a comparison against a bytes literal failed to find an overload.
    [Test]
    public virtual void FixedFieldIsBytes()
    {
        RecordSchema recordSchema = (RecordSchema)Schema.Parse(
            "{\"type\":\"record\",\"name\":\"FixedRecord\",\"fields\":[" +
            "{\"name\":\"fx\",\"type\":{\"type\":\"fixed\",\"name\":\"F4\",\"size\":4}}]}");

        ScriptHost scriptHost = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();
        recordSchema.TryGetField("fx", out var field);
        GenericRecord record = new GenericRecord(recordSchema);
        record.Add("fx", new GenericFixed((FixedSchema)field.Schema, new byte[] { 1, 2, 3, 4 }));

        foreach (string expr in new[]
                 {
                     "user.fx == b'\\x01\\x02\\x03\\x04'",
                     "size(user.fx) == 4",
                     "type(user.fx) == bytes"
                 })
        {
            Script script =
                scriptHost
                    .BuildScript(expr)
                    .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(recordSchema.Fullname)))
                    .WithTypes(recordSchema)
                    .Build();
            Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = record }),
                Is.True, expr);
        }
    }

    // A nullable union (["null", X]) is declared `dyn`, not X, so a rule can compare the field to
    // null - the natural way to test an Avro optional. Declaring X made the checker reject
    // `user.opt == null` with no matching overload for `_==_` applied to `(string, null)`, and it
    // rejected the comparison even when the field was *set*, because the declaration is decided at
    // check time. The member type still resolves at runtime, so the member's own operators work.
    [Test]
    public virtual void NullableUnionFieldIsComparableToNull()
    {
        RecordSchema recordSchema = (RecordSchema)Schema.Parse(
            "{\"type\":\"record\",\"name\":\"OptRecord\",\"fields\":[" +
            "{\"name\":\"opt\",\"type\":[\"null\",\"string\"]}]}");

        ScriptHost scriptHost = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();

        GenericRecord unset = new GenericRecord(recordSchema);
        unset.Add("opt", null);
        GenericRecord set = new GenericRecord(recordSchema);
        set.Add("opt", "hi");

        foreach ((string expr, GenericRecord record, bool expected) in new[]
                 {
                     ("user.opt == null", unset, true),
                     ("user.opt != null", unset, false),
                     ("user.opt == null", set, false),
                     ("user.opt != null", set, true),
                     // The member type still works through the dyn declaration.
                     ("user.opt == 'hi'", set, true),
                     ("type(user.opt) == string", set, true)
                 })
        {
            Script script =
                scriptHost
                    .BuildScript(expr)
                    .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(recordSchema.Fullname)))
                    .WithTypes(recordSchema)
                    .Build();
            Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = record }),
                Is.EqualTo(expected), expr);
        }
    }

    [Test]
    public virtual void ComplexInput()
    {
        ScriptHost scriptHost = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();

        Script script =
            scriptHost
                .BuildScript("user.friends[0].kind == \"TWO\"")
                .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(User._SCHEMA.Fullname)))
                .WithTypes(User._SCHEMA)
                .Build();

        User friend = new User { name = "friend", friends = new List<User>(), kind = Kind.TWO };
        User user = new User { name = "foobar", friends = new List<User> { friend }, kind = Kind.ONE };

        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = user }), Is.True);

        RecordSchema recordSchema = (RecordSchema)User._SCHEMA;
        recordSchema.TryGetField("kind", out var field);
        EnumSchema enumSchema = (EnumSchema)field.Schema;

        GenericRecord friend2 = new GenericRecord(recordSchema);
        friend2.Add("name", "friend");
        friend2.Add("kind", new GenericEnum(enumSchema, "TWO"));

        GenericRecord user2 = new GenericRecord(recordSchema);
        user2.Add("name", "foobar");
        user2.Add("kind", new GenericEnum(enumSchema, "ONE"));
        user2.Add("friends", new List<GenericRecord> { friend2 });

        Assert.That(script.Execute<bool>(new Dictionary<string, object> { ["user"] = user2 }), Is.True);

        // return the enum

        script =
            scriptHost
                .BuildScript("user.friends[0].kind")
                .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(User._SCHEMA.Fullname)))
                .WithTypes(User._SCHEMA)
                .Build();

        Assert.That(script.Execute<string>(new Dictionary<string, object> { ["user"] = user }), Is.EqualTo("TWO"));

        Assert.That(script.Execute<string>(new Dictionary<string, object> { ["user"] = user2 }), Is.EqualTo("TWO"));
    }
}