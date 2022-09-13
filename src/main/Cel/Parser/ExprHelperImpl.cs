using Cel.Common;
using Google.Api.Expr.V1Alpha1;
using Google.Protobuf;

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
namespace Cel.Parser;

public sealed class ExprHelperImpl : IExprHelper
{
    private readonly long id;
    private readonly Helper parserHelper;

    public ExprHelperImpl(Helper parserHelper, long id)
    {
        this.parserHelper = parserHelper;
        this.id = id;
    }

    // LiteralBool implements the ExprHelper interface method.
    public Expr LiteralBool(bool value)
    {
        return parserHelper.NewLiteralBool(NextMacroId(), value);
    }

    // LiteralBytes implements the ExprHelper interface method.
    public Expr LiteralBytes(ByteString value)
    {
        return parserHelper.NewLiteralBytes(NextMacroId(), value);
    }

    // LiteralDouble implements the ExprHelper interface method.
    public Expr LiteralDouble(double value)
    {
        return parserHelper.NewLiteralDouble(NextMacroId(), value);
    }

    // LiteralInt implements the ExprHelper interface method.
    public Expr LiteralInt(long value)
    {
        return parserHelper.NewLiteralInt(NextMacroId(), value);
    }

    // LiteralString implements the ExprHelper interface method.
    public Expr LiteralString(string value)
    {
        return parserHelper.NewLiteralString(NextMacroId(), value);
    }

    // LiteralUint implements the ExprHelper interface method.
    public Expr LiteralUint(ulong value)
    {
        return parserHelper.NewLiteralUint(NextMacroId(), value);
    }

    // NewList implements the ExprHelper interface method.
    public Expr NewList(IList<Expr> elems)
    {
        return parserHelper.NewList(NextMacroId(), elems);
    }

    public Expr NewList(params Expr[] elems)
    {
        return NewList(new List<Expr>(elems));
    }

    // NewMap implements the ExprHelper interface method.
    public Expr NewMap(IList<Expr.Types.CreateStruct.Types.Entry> entries)
    {
        return parserHelper.NewMap(NextMacroId(), entries);
    }

    // NewMapEntry implements the ExprHelper interface method.
    public Expr.Types.CreateStruct.Types.Entry NewMapEntry(Expr key, Expr val)
    {
        return parserHelper.NewMapEntry(NextMacroId(), key, val);
    }

    // NewObject implements the ExprHelper interface method.
    public Expr NewObject(string typeName, IList<Expr.Types.CreateStruct.Types.Entry> fieldInits)
    {
        return parserHelper.NewObject(NextMacroId(), typeName, fieldInits);
    }

    // NewObjectFieldInit implements the ExprHelper interface method.
    public Expr.Types.CreateStruct.Types.Entry NewObjectFieldInit(string field, Expr init)
    {
        return parserHelper.NewObjectField(NextMacroId(), field, init);
    }

    // Fold implements the ExprHelper interface method.
    public Expr Fold(string iterVar, Expr? iterRange, string accuVar, Expr accuInit, Expr condition, Expr step,
        Expr result)
    {
        return parserHelper.NewComprehension(NextMacroId(), iterVar, iterRange, accuVar, accuInit, condition, step,
            result);
    }

    // Ident implements the ExprHelper interface method.
    public Expr Ident(string name)
    {
        return parserHelper.NewIdent(NextMacroId(), name);
    }

    // GlobalCall implements the ExprHelper interface method.
    public Expr GlobalCall(string function, IList<Expr> args)
    {
        return parserHelper.NewGlobalCall(NextMacroId(), function, args);
    }

    public Expr GlobalCall(string function, params Expr[] args)
    {
        return GlobalCall(function, new List<Expr>(args));
    }

    // ReceiverCall implements the ExprHelper interface method.
    public Expr ReceiverCall(string function, Expr target, IList<Expr> args)
    {
        return parserHelper.NewReceiverCall(NextMacroId(), function, target, args);
    }

    // PresenceTest implements the ExprHelper interface method.
    public Expr PresenceTest(Expr operand, string field)
    {
        return parserHelper.NewPresenceTest(NextMacroId(), operand, field);
    }

    // Select implements the ExprHelper interface method.
    public Expr Select(Expr operand, string field)
    {
        return parserHelper.NewSelect(NextMacroId(), operand, field);
    }

    // OffsetLocation implements the ExprHelper interface method.
    public ILocation OffsetLocation(long exprId)
    {
        return parserHelper.GetLocation(exprId);
    }

    internal long NextMacroId()
    {
        return parserHelper.Id(parserHelper.GetLocation(id));
    }
}