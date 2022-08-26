using System.Collections.Generic;

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
namespace Cel.Parser
{
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using ByteString = Google.Protobuf.ByteString;
	using Location = Cel.Common.Location;

	public sealed class ExprHelperImpl : ExprHelper
	{

	  internal readonly Helper parserHelper;
	  internal readonly long id;

	  public ExprHelperImpl(Helper parserHelper, long id)
	  {
		this.parserHelper = parserHelper;
		this.id = id;
	  }

	  internal long NextMacroID()
	  {
		return parserHelper.Id(parserHelper.GetLocation(id));
	  }

	  // LiteralBool implements the ExprHelper interface method.
	  public Expr LiteralBool(bool value)
	  {
		return parserHelper.NewLiteralBool(NextMacroID(), value);
	  }

	  // LiteralBytes implements the ExprHelper interface method.
	  public Expr LiteralBytes(ByteString value)
	  {
		return parserHelper.NewLiteralBytes(NextMacroID(), value);
	  }

	  // LiteralDouble implements the ExprHelper interface method.
	  public Expr LiteralDouble(double value)
	  {
		return parserHelper.NewLiteralDouble(NextMacroID(), value);
	  }

	  // LiteralInt implements the ExprHelper interface method.
	  public Expr LiteralInt(long value)
	  {
		return parserHelper.NewLiteralInt(NextMacroID(), value);
	  }

	  // LiteralString implements the ExprHelper interface method.
	  public Expr LiteralString(string value)
	  {
		return parserHelper.NewLiteralString(NextMacroID(), value);
	  }

	  // LiteralUint implements the ExprHelper interface method.
	  public Expr LiteralUint(ulong value)
	  {
		return parserHelper.NewLiteralUint(NextMacroID(), value);
	  }

	  // NewList implements the ExprHelper interface method.
	  public Expr NewList(IList<Expr> elems)
	  {
		return parserHelper.NewList(NextMacroID(), elems);
	  }

	  public Expr NewList(params Expr[] elems)
	  {
		  return NewList(elems.ToArray());
	  }

	  // NewMap implements the ExprHelper interface method.
	  public Expr NewMap(IList<Expr.Types.CreateStruct.Types.Entry> entries)
	  {
		return parserHelper.NewMap(NextMacroID(), entries);
	  }

	  // NewMapEntry implements the ExprHelper interface method.
	  public Expr.Types.CreateStruct.Types.Entry NewMapEntry(Expr key, Expr val)
	  {
		return parserHelper.NewMapEntry(NextMacroID(), key, val);
	  }

	  // NewObject implements the ExprHelper interface method.
	  public Expr NewObject(string typeName, IList<Expr.Types.CreateStruct.Types.Entry> fieldInits)
	  {
		return parserHelper.NewObject(NextMacroID(), typeName, fieldInits);
	  }

	  // NewObjectFieldInit implements the ExprHelper interface method.
	  public Expr.Types.CreateStruct.Types.Entry NewObjectFieldInit(string field, Expr init)
	  {
		return parserHelper.NewObjectField(NextMacroID(), field, init);
	  }

	  // Fold implements the ExprHelper interface method.
	  public Expr Fold(string iterVar, Expr iterRange, string accuVar, Expr accuInit, Expr condition, Expr step, Expr result)
	  {
		return parserHelper.NewComprehension(NextMacroID(), iterVar, iterRange, accuVar, accuInit, condition, step, result);
	  }

	  // Ident implements the ExprHelper interface method.
	  public Expr Ident(string name)
	  {
		return parserHelper.NewIdent(NextMacroID(), name);
	  }

	  // GlobalCall implements the ExprHelper interface method.
	  public Expr GlobalCall(string function, IList<Expr> args)
	  {
		return parserHelper.NewGlobalCall(NextMacroID(), function, args);
	  }

	  public Expr GlobalCall(string function, params Expr[] args)
	  {
		  return GlobalCall(function, args.ToArray());
	  }

	  // ReceiverCall implements the ExprHelper interface method.
	  public Expr ReceiverCall(string function, Expr target, IList<Expr> args)
	  {
		return parserHelper.NewReceiverCall(NextMacroID(), function, target, args);
	  }

	  // PresenceTest implements the ExprHelper interface method.
	  public Expr PresenceTest(Expr operand, string field)
	  {
		return parserHelper.NewPresenceTest(NextMacroID(), operand, field);
	  }

	  // Select implements the ExprHelper interface method.
	  public Expr Select(Expr operand, string field)
	  {
		return parserHelper.NewSelect(NextMacroID(), operand, field);
	  }

	  // OffsetLocation implements the ExprHelper interface method.
	  public Location OffsetLocation(long exprID)
	  {
		return parserHelper.GetLocation(exprID);
	  }
	}

}