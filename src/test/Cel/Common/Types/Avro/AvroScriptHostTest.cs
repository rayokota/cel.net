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