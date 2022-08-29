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
namespace Cel.Parser
{
    using Expr = Google.Api.Expr.V1Alpha1.Expr;
    using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
    using ByteString = Google.Protobuf.ByteString;
    using Location = global::Cel.Common.Location;

    /// <summary>
    /// ExprHelper assists with the manipulation of proto-based Expr values in a manner which is
    /// consistent with the source position and expression id generation code leveraged by both the
    /// parser and type-checker.
    /// </summary>
    public interface ExprHelper
    {
        /// <summary>
        /// LiteralBool creates an Expr value for a bool literal. </summary>
        Expr LiteralBool(bool value);

        /// <summary>
        /// LiteralBytes creates an Expr value for a byte literal. </summary>
        Expr LiteralBytes(ByteString value);

        /// <summary>
        /// LiteralDouble creates an Expr value for double literal. </summary>
        Expr LiteralDouble(double value);

        /// <summary>
        /// LiteralInt creates an Expr value for an int literal. </summary>
        Expr LiteralInt(long value);

        /// <summary>
        /// LiteralString creates am Expr value for a string literal. </summary>
        Expr LiteralString(string value);

        /// <summary>
        /// LiteralUint creates an Expr value for a uint literal. </summary>
        Expr LiteralUint(ulong value);

        /// <summary>
        /// NewList creates a CreateList instruction where the list is comprised of the optional set of
        /// elements provided as arguments.
        /// </summary>
        Expr NewList(IList<Expr> elems);

        /// <summary>
        /// NewList creates a CreateList instruction where the list is comprised of the optional set of
        /// elements provided as arguments.
        /// </summary>
        Expr NewList(params Expr[] elems);

        /// <summary>
        /// NewMap creates a CreateStruct instruction for a map where the map is comprised of the optional
        /// set of key, value entries.
        /// </summary>
        Expr NewMap(IList<Expr.Types.CreateStruct.Types.Entry> entries);

        /// <summary>
        /// NewMapEntry creates a Map Entry for the key, value pair. </summary>
        Expr.Types.CreateStruct.Types.Entry NewMapEntry(Expr key, Expr val);

        /// <summary>
        /// NewObject creates a CreateStruct instruction for an object with a given type name and optional
        /// set of field initializers.
        /// </summary>
        Expr NewObject(string typeName, IList<Expr.Types.CreateStruct.Types.Entry> fieldInits);

        /// <summary>
        /// NewObjectFieldInit creates a new Object field initializer from the field name and value. </summary>
        Expr.Types.CreateStruct.Types.Entry NewObjectFieldInit(string field, Expr init);

        /// <summary>
        /// Fold creates a fold comprehension instruction.
        /// 
        /// <para>- iterVar is the iteration variable name. - iterRange represents the expression that
        /// resolves to a list or map where the elements or keys (respectively) will be iterated over. -
        /// accuVar is the accumulation variable name, typically parser.AccumulatorName. - accuInit is the
        /// initial expression whose value will be set for the accuVar prior to folding. - condition is the
        /// expression to test to determine whether to continue folding. - step is the expression to
        /// evaluation at the conclusion of a single fold iteration. - result is the computation to
        /// evaluate at the conclusion of the fold.
        /// 
        /// </para>
        /// <para>The accuVar should not shadow variable names that you would like to reference within the
        /// environment in the step and condition expressions. Presently, the name __result__ is commonly
        /// used by built-in macros but this may change in the future.
        /// </para>
        /// </summary>
        Expr Fold(string iterVar, Expr iterRange, string accuVar, Expr accuInit, Expr condition, Expr step,
            Expr result);

        /// <summary>
        /// Ident creates an identifier Expr value. </summary>
        Expr Ident(string name);

        /// <summary>
        /// GlobalCall creates a function call Expr value for a global (free) function. </summary>
        Expr GlobalCall(string function, IList<Expr> args);

        /// <summary>
        /// GlobalCall creates a function call Expr value for a global (free) function. </summary>
        Expr GlobalCall(string function, params Expr[] args);

        /// <summary>
        /// ReceiverCall creates a function call Expr value for a receiver-style function. </summary>
        Expr ReceiverCall(string function, Expr target, IList<Expr> args);

        /// <summary>
        /// PresenceTest creates a Select TestOnly Expr value for modelling has() semantics. </summary>
        Expr PresenceTest(Expr operand, string field);

        /// <summary>
        /// Select create a field traversal Expr value. </summary>
        Expr Select(Expr operand, string field);

        /// <summary>
        /// OffsetLocation returns the Location of the expression identifier. </summary>
        Location OffsetLocation(long exprID);
    }
}