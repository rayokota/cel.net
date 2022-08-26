using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

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
	using Constant = Google.Api.Expr.V1Alpha1.Constant;
	using Expr = Google.Api.Expr.V1Alpha1.Expr;
	using Call = Google.Api.Expr.V1Alpha1.Expr.Types.Call;
	using Comprehension = Google.Api.Expr.V1Alpha1.Expr.Types.Comprehension;
	using CreateList = Google.Api.Expr.V1Alpha1.Expr.Types.CreateList;
	using CreateStruct = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct;
	using Entry = Google.Api.Expr.V1Alpha1.Expr.Types.CreateStruct.Types.Entry;
	using Ident = Google.Api.Expr.V1Alpha1.Expr.Types.Ident;
	using Select = Google.Api.Expr.V1Alpha1.Expr.Types.Select;
	using SourceInfo = Google.Api.Expr.V1Alpha1.SourceInfo;
	using ByteString = Google.Protobuf.ByteString;
	using Location = Cel.Common.Location;
	using Source = Cel.Common.Source;
	using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;
	using Token = Antlr4.Runtime.IToken;

	public sealed class Helper
	{
	  private readonly Source source;
	  private readonly IDictionary<long, int> positions;
	  private long nextID;

	  internal Helper(Source source)
	  {
		this.source = source;
		this.nextID = 1;
		this.positions = new Dictionary<long, int>();
	  }

	  internal SourceInfo SourceInfo
	  {
		  get
		  {
			  SourceInfo sourceInfo = new SourceInfo();
			  sourceInfo.Location = source.Description();
			  sourceInfo.Positions.Add(positions);
			  sourceInfo.LineOffsets.Add(source.LineOffsets());
			  return sourceInfo;
		  }
	  }

	  internal Expr NewLiteral(object ctx, Constant value)
	  {
		  Expr expr = NewExpr(ctx);
		  expr.ConstExpr = value;
		  return expr;
	  }

	  internal Expr NewLiteralBool(object ctx, bool value)
	  {
		  Constant constant = new Constant();
		  constant.BoolValue = value;
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewLiteralString(object ctx, string value)
	  {
		  Constant constant = new Constant();
		  constant.StringValue = value;
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewLiteralBytes(object ctx, ByteString value)
	  {
		  Constant constant = new Constant();
		  constant.BytesValue = value;
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewLiteralInt(object ctx, long value)
	  {
		  Constant constant = new Constant();
		  constant.Int64Value = value;
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewLiteralUint(object ctx, ulong value)
	  {
		  Constant constant = new Constant();
		  constant.Uint64Value = Convert.ToUInt64(value);
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewLiteralDouble(object ctx, double value)
	  {
		  Constant constant = new Constant();
		  constant.DoubleValue = value;
		  return NewLiteral(ctx, constant);
	  }

	  internal Expr NewIdent(object ctx, string name)
	  {
		  Ident ident = new Ident();
		  ident.Name = name;
		  Expr expr = NewExpr(ctx);
		  expr.IdentExpr = ident;
		  return expr;
	  }

	  internal Expr NewSelect(object ctx, Expr operand, string field)
	  {
		  Select selectExpr = new Select();
		  selectExpr.Operand = operand;
		  selectExpr.Field = field;
		  Expr expr = NewExpr(ctx);
		  expr.SelectExpr = selectExpr;
		  return expr;
	  }

	  internal Expr NewPresenceTest(object ctx, Expr operand, string field)
	  {
		  Select selectExpr = new Select();
		  selectExpr.Operand = operand;
		  selectExpr.Field = field;
		  selectExpr.TestOnly = true;
		  Expr expr = NewExpr(ctx);
		  expr.SelectExpr = selectExpr;
		  return expr;
	  }

	  internal Expr NewGlobalCall(object ctx, string function, params Expr[] args)
	  {
		  return NewGlobalCall(ctx, function, args.ToList());
	  }

	  internal Expr NewGlobalCall(object ctx, string function, IList<Expr> args)
	  {
		  Call call = new Call();
		  call.Function = function;
		  call.Args.Add(args);
		  Expr expr = NewExpr(ctx);
		  expr.CallExpr = call;
		  return expr;
	  }

	  internal Expr NewReceiverCall(object ctx, string function, Expr target, IList<Expr> args)
	  {
		  Call call = new Call();
		  call.Function = function;
		  call.Target = target;
		  call.Args.Add(args);
		  Expr expr = NewExpr(ctx);
		  expr.CallExpr = call;
		  return expr;
	  }

	  internal Expr NewList(object ctx, IList<Expr> elements)
	  {
		  CreateList createList = new CreateList();
		  createList.Elements.Add(elements);
		  Expr expr = NewExpr(ctx);
		  expr.ListExpr = createList;
		  return expr;
	  }

	  internal Expr NewMap(object ctx, IList<Expr.Types.CreateStruct.Types.Entry> entries)
	  {
		  CreateStruct createStruct = new CreateStruct();
		  createStruct.Entries.Add(entries);
		  Expr expr = NewExpr(ctx);
		  expr.StructExpr = createStruct;
		  return expr;
	  }

	  internal Expr.Types.CreateStruct.Types.Entry NewMapEntry(long entryID, Expr key, Expr value)
	  {
		  Entry entry = new Entry();
		  entry.Id = entryID;
		  entry.MapKey = key;
		  entry.Value = value;
		  return entry;
	  }

	  internal Expr NewObject(object ctx, string typeName, IList<Expr.Types.CreateStruct.Types.Entry> entries)
	  {
		  CreateStruct createStruct = new CreateStruct();
		  createStruct.MessageName = typeName;
		  createStruct.Entries.Add(entries);
		  Expr expr = NewExpr(ctx);
		  expr.StructExpr = createStruct;
		  return expr;
	  }

	  internal Expr.Types.CreateStruct.Types.Entry NewObjectField(long fieldID, string field, Expr value)
	  {
		  Entry entry = new Entry();
		  entry.Id = fieldID;
		  entry.FieldKey = field;
		  entry.Value = value;
		  return entry;
	  }

	  internal Expr NewComprehension(object ctx, string iterVar, Expr iterRange, string accuVar, Expr accuInit, Expr condition, Expr step, Expr result)
	  {
		  Comprehension comprehension = new Comprehension();
		  comprehension.AccuVar = accuVar;
		  comprehension.AccuInit = accuInit;
		  comprehension.IterVar = iterVar;
		  comprehension.IterRange = iterRange;
		  comprehension.LoopCondition = condition;
		  comprehension.LoopStep = step;
		  comprehension.Result = result;
		  Expr expr =  NewExpr(ctx);
		  expr.ComprehensionExpr = comprehension;
		  return expr;
	  }

	  internal Expr NewExpr(object ctx)
	  {
		long exprId = (ctx is long) ? ((long?) ctx).Value : Id(ctx);
		Expr expr = new Expr();
		expr.Id = exprId;
		return expr;
	  }

	  internal long Id(object ctx)
	  {
		Location location;
		if (ctx is ParserRuleContext)
		{
		  Token token = ((ParserRuleContext) ctx).Start;
		  location = source.NewLocation(token.Line, token.Column);
		}
		else if (ctx is Token)
		{
		  Token token = (Token) ctx;
		  location = source.NewLocation(token.Line, token.Column);
		}
		else if (ctx is Location)
		{
		  location = (Location) ctx;
		}
		else
		{
		  // This should only happen if the ctx is nil
		  return -1L;
		}
		long id = nextID;
		positions[id] = source.LocationOffset(location);
		nextID++;
		return id;
	  }

	  internal Location GetLocation(long id)
	  {
		int characterOffset = positions[id];
		return source.OffsetLocation(characterOffset);
	  }

	  // newBalancer creates a balancer instance bound to a specific function and its first term.
	  internal Balancer NewBalancer(string function, Expr term)
	  {
		return new Balancer(this, function, term);
	  }

	  internal sealed class Balancer
	  {
		  private readonly Helper outerInstance;

		internal readonly string function;
		internal readonly IList<Expr> terms;
		internal readonly IList<long> ops;

		public Balancer(Helper outerInstance, string function, Expr term)
		{
			this.outerInstance = outerInstance;
		  this.function = function;
		  this.terms = new List<Expr>();
		  this.terms.Add(term);
		  this.ops = new List<long>();
		}

		// addTerm adds an operation identifier and term to the set of terms to be balanced.
		internal void AddTerm(long op, Expr term)
		{
		  terms.Add(term);
		  ops.Add(op);
		}

		// balance creates a balanced tree from the sub-terms and returns the final Expr value.
		internal Expr balance()
		{
		  if (terms.Count == 1)
		  {
			return terms[0];
		  }
		  return BalancedTree(0, ops.Count - 1);
		}

		// balancedTree recursively balances the terms provided to a commutative operator.
		internal Expr BalancedTree(int lo, int hi)
		{
		  int mid = (lo + hi + 1) / 2;

		  Expr left;
		  if (mid == lo)
		  {
			left = terms[mid];
		  }
		  else
		  {
			left = BalancedTree(lo, mid - 1);
		  }

		  Expr right;
		  if (mid == hi)
		  {
			right = terms[mid + 1];
		  }
		  else
		  {
			right = BalancedTree(mid + 1, hi);
		  }
		  return outerInstance.NewGlobalCall(ops[mid], function, left, right);
		}
	  }
	}

}