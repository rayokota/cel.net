/*
 * Copyright (C) 2026 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
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
using NUnit.Framework;

namespace Cel.Types.Avro;

/// <summary>
///     An opaque value must fail *as CEL* when a rule treats it as a container.
///
///     <see cref="Cel.Common.Types.TypeT.NewObjectTypeValue" /> advertises the indexer and
///     field-tester traits, and an opaque carrier implements neither - so on paper a trait-gated
///     dispatch that casts straight to the interface would throw <c>InvalidCastException</c> out
///     of the evaluator instead of producing a CEL error. It does not: indexing and field
///     selection are resolved by the attribute/qualifier layer, which tests for the interface and
///     returns "no such overload" or "invalid type for field selection".
///
///     Pinned here because the reasoning is not local - the trait advertisement and the code that
///     honours it are in different layers, and the safety comes from the layer that is *not*
///     advertising anything. A rule author must get a rule error, never an evaluator crash.
/// </summary>
internal class OpaqueValueTraitTest
{
    private static readonly RecordSchema DecimalRecord = (RecordSchema)Schema.Parse(
        "{\"type\":\"record\",\"name\":\"DecimalTraitRecord\",\"fields\":[" +
        "{\"name\":\"amount\",\"type\":{\"type\":\"bytes\",\"logicalType\":\"decimal\"," +
        "\"precision\":8,\"scale\":2}}]}");

    private static Exception Evaluate(string expr)
    {
        var record = new GenericRecord(DecimalRecord);
        record.Add("amount", new AvroDecimal(12.34m));

        ScriptHost host = ScriptHost.NewBuilder().Registry(AvroRegistry.NewRegistry()).Build();
        Script script = host.BuildScript(expr)
            .WithDeclarations(Decls.NewVar("user", Decls.NewObjectType(DecimalRecord.Fullname)))
            .WithTypes(DecimalRecord)
            .Build();

        return Assert.Catch(() =>
            script.Execute<object>(new Dictionary<string, object> { ["user"] = record }))!;
    }

    // Every shape that reaches the value as a container, the dynamic ones included: the field is
    // declared `dyn` (a logical type has no nameable check-time type), so all of these compile.
    [TestCase("user.amount[0]")]
    [TestCase("dyn(user.amount)[0]")]
    [TestCase("dyn(user.amount)['k']")]
    [TestCase("[user.amount][0][0]")]
    [TestCase("(user.amount)[0] == 1")]
    [TestCase("dyn(user.amount).foo")]
    [TestCase("has(user.amount.x)")]
    [TestCase("size(user.amount)")]
    public virtual void TreatingAnOpaqueValueAsAContainerIsARuleError(string expr)
    {
        var e = Evaluate(expr);

        Assert.That(e, Is.TypeOf<ScriptExecutionException>());
        // The point of the test: not an InvalidCastException, and not any other evaluator crash.
        for (var inner = e; inner != null; inner = inner.InnerException)
        {
            Assert.That(inner, Is.Not.TypeOf<InvalidCastException>());
        }
        Assert.That(e.Message, Does.Contain("no such overload")
            .Or.Contain("invalid type for field selection"));
    }
}
