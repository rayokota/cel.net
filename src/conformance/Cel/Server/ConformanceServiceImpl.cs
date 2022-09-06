using Cel.Common;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

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
namespace Cel.Server
{
	using CheckRequest = Google.Api.Expr.Conformance.V1Alpha1.CheckRequest;
	using CheckResponse = Google.Api.Expr.Conformance.V1Alpha1.CheckResponse;
	using ConformanceServiceImplBase = Google.Api.Expr.Conformance.V1Alpha1.ConformanceService.ConformanceServiceBase;
	using ErrorSet = Google.Api.Expr.V1Alpha1.ErrorSet;
	using EvalRequest = Google.Api.Expr.Conformance.V1Alpha1.EvalRequest;
	using EvalResponse = Google.Api.Expr.Conformance.V1Alpha1.EvalResponse;
	using ExprValue = Google.Api.Expr.V1Alpha1.ExprValue;
	using IssueDetails = Google.Api.Expr.Conformance.V1Alpha1.IssueDetails;
	using ListValue = Google.Api.Expr.V1Alpha1.ListValue;
	using MapValue = Google.Api.Expr.V1Alpha1.MapValue;
	using Entry = Google.Api.Expr.V1Alpha1.MapValue.Types.Entry;
	using ParseRequest = Google.Api.Expr.Conformance.V1Alpha1.ParseRequest;
	using ParseResponse = Google.Api.Expr.Conformance.V1Alpha1.ParseResponse;
	using SourcePosition = Google.Api.Expr.V1Alpha1.SourcePosition;
	using UnknownSet = Google.Api.Expr.V1Alpha1.UnknownSet;
	using Value = Google.Api.Expr.V1Alpha1.Value;
	using Any = Google.Protobuf.WellKnownTypes.Any;
	using ByteString = Google.Protobuf.ByteString;
	using Duration = Google.Protobuf.WellKnownTypes.Duration;
	using Message = Google.Protobuf.IMessage;
	using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
	using Code = Google.Rpc.Code;
	using Status = Google.Rpc.Status;
	  using TestAllTypesPb2 = Google.Api.Expr.Test.V1.Proto2.TestAllTypes;
	  using TestAllTypesPb3 = Google.Api.Expr.Test.V1.Proto3.TestAllTypes;

	public class ConformanceServiceImpl : ConformanceServiceImplBase
	{

		private bool verboseEvalErrors;

	  public virtual bool VerboseEvalErrors
	  {
		  set
		  {
			this.verboseEvalErrors = value;
		  }
	  }

      public override Task<ParseResponse> Parse(ParseRequest request, ServerCallContext context)
	  {
		  string sourceText = request.CelSource;
		  if (sourceText.Trim().Length == 0)
		  {
			throw new System.ArgumentException("No source code.");
		  }

		  // NOTE: syntax_version isn't currently used
		  IList<EnvOption> parseOptions = new List<EnvOption>();
		  if (request.DisableMacros)
		  {
			parseOptions.Add(IEnvOption.ClearMacros());
		  }

		  Env env = Env.NewEnv(((List<EnvOption>)parseOptions).ToArray());
		  Env.AstIssuesTuple astIss = env.Parse(sourceText);

		  ParseResponse response = new ParseResponse();
		  if (!astIss.HasIssues())
		  {
			// Success
			response.ParsedExpr = Cel.AstToParsedExpr(astIss.Ast);
		  }
		  else
		  {
			// Failure
			AppendErrors(astIss.Issues.Errors, response.Issues);
		  }

		  return Task.FromResult(response);
	  }

      public override Task<CheckResponse> Check(CheckRequest request, ServerCallContext context)
	  {
		  // Build the environment.
		  IList<EnvOption> checkOptions = new List<EnvOption>();
		  if (!request.NoStdEnv)
		  {
			checkOptions.Add(global::Cel.Library.StdLib());
		  }

		  checkOptions.Add(IEnvOption.Container(request.Container));
		  checkOptions.Add(IEnvOption.Declarations(request.TypeEnv));
		  checkOptions.Add(IEnvOption.Types(new TestAllTypesPb2(), new TestAllTypesPb3()));
		  Env env = Env.NewCustomEnv(((List<EnvOption>)checkOptions).ToArray());

		  // Check the expression.
		  Env.AstIssuesTuple astIss = env.Check(Cel.ParsedExprToAst(request.ParsedExpr));
		  CheckResponse resp = new CheckResponse();

		  if (!astIss.HasIssues())
		  {
			// Success
			resp.CheckedExpr = Cel.AstToCheckedExpr(astIss.Ast);
		  }
		  else
		  {
			// Failure
			AppendErrors(astIss.Issues.Errors, resp.Issues);
		  }

		  return Task.FromResult(resp);
	  }

      public override Task<EvalResponse> Eval(EvalRequest request, ServerCallContext context)
	  {
		
		  Env env = Env.NewEnv(IEnvOption.Container(request.Container), IEnvOption.Types(new TestAllTypesPb2(), new TestAllTypesPb3()));

		  global::Cel.Program prg;
		  Ast ast;

		  switch (request.ExprKindCase)
		  {
			case EvalRequest.ExprKindOneofCase.ParsedExpr:
			  ast = Cel.ParsedExprToAst(request.ParsedExpr);
			  break;
			case EvalRequest.ExprKindOneofCase.CheckedExpr:
			  ast = Cel.CheckedExprToAst(request.CheckedExpr);
			  break;
			default:
			  throw new System.ArgumentException("No expression.");
		  }

		  prg = env.Program(ast);

		  IDictionary<string, object> args = new Dictionary<string, object>();
		  foreach (var entry in request.Bindings)
		  {
			  var name = entry.Key;
			  var exprValue = entry.Value;
		  Val refVal = ExprValueToRefValue(env.TypeAdapter, exprValue);
		  args[name] = refVal;
		  }

		  // NOTE: the EvalState is currently discarded
		  EvalResult res = prg.Eval(args);
		  ExprValue resultExprVal;
		  if (!Err.IsError(res.Val))
		  {
			resultExprVal = RefValueToExprValue(res.Val);
		  }
		  else
		  {
			Err err = (Err) res.Val;

			if (verboseEvalErrors)
			{
				Console.Error.Write("\n" + "Eval error (not necessarily a bug!!!):\n" + "  error: {0}\n" + "{1}", err, err.HasCause() ? (Stacktrace(err.ToRuntimeException()) + "\n") : "");
			}

			Status status = new Status();
			status.Message =  err.ToString();
			ErrorSet errorSet = new ErrorSet();
			errorSet.Errors.Add(status);
			resultExprVal = new ExprValue();
			resultExprVal.Error = errorSet;
		  }

		  EvalResponse resp = new EvalResponse();
		  resp.Result = resultExprVal;

		  return Task.FromResult(resp);

	  }

	  internal static string? Stacktrace(Exception t)
	  {
		  return t.StackTrace;
	  }

	  /// <summary>
	  /// appendErrors converts the errors from errs to Status messages and appends them to the list of
	  /// issues.
	  /// </summary>
	  internal static void AppendErrors(IList<CelError> errs, RepeatedField<Status> issues)
	  {
		  foreach (var e in errs)
		  {
			  issues.Add(ErrToStatus(e, IssueDetails.Types.Severity.Error));
		  }
	  }

	  /// <summary>
	  /// ErrToStatus converts an Error to a Status message with the given severity. </summary>
	  internal static Status ErrToStatus(CelError e, IssueDetails.Types.Severity severity)
	  {
		  IssueDetails detail = new IssueDetails();
		  detail.Severity = severity;
		  SourcePosition pos = new SourcePosition();
		  pos.Line = e.Location.Line();
		  pos.Column = e.Location.Column();
		  detail.Position = pos;

		  Status status = new Status();
		  status.Code = (int)Code.InvalidArgument;
		  status.Message = e.Message;
		  status.Details.Add(Any.Pack(detail));
		  return status;
	  }

	  /// <summary>
	  /// RefValueToExprValue converts between ref.Val and exprpb.ExprValue. </summary>
	  internal static ExprValue RefValueToExprValue(Val res)
	  {
		if (UnknownT.IsUnknown(res))
		{
			UnknownSet unknownSet = new UnknownSet();
			unknownSet.Exprs.Add(res.IntValue());
			ExprValue exprValue = new ExprValue();
			exprValue.Unknown = unknownSet;
			return exprValue;
		}
		Value v = RefValueToValue(res);
			ExprValue result = new ExprValue();
			result.Value = v;
			return result;
	  }

	  // TODO(jimlarson): The following conversion code should be moved to
	  //  common/types/provider.go and consolidated/refactored as appropriate.
	  //  In particular, make judicious use of types.NativeToValue().

	  /// <summary>
	  /// RefValueToValue converts between ref.Val and Value. The ref.Val must not be error or unknown.
	  /// </summary>
	  internal static Value RefValueToValue(Val res)
	  {
		Value val = new Value();
		switch (res.Type().TypeEnum().InnerEnumValue)
		{
		  case TypeEnum.InnerEnum.Bool:
			  val.BoolValue = res.BooleanValue();
			  return val;
		  case TypeEnum.InnerEnum.Bytes:
			  val.BytesValue = (ByteString) res.ConvertToNative(typeof(ByteString));
			  return val;
		  case TypeEnum.InnerEnum.Double:
			  val.DoubleValue = (double)res.ConvertToNative(typeof(double));
			  return val;
		  case TypeEnum.InnerEnum.Int:
			  val.Int64Value = res.IntValue();
			  return val;
		  case TypeEnum.InnerEnum.Null:
			  val.NullValue = NullValue.NullValue;
			  return val;
		  case TypeEnum.InnerEnum.String:
			  val.StringValue = res.Value().ToString();
			  return val;
		  case TypeEnum.InnerEnum.Type:
			  val.TypeValue = ((TypeT)res).TypeName();
			  return val;
		  case TypeEnum.InnerEnum.Uint:
			  val.Uint64Value = (ulong) res.IntValue();
			  return val;
		  case TypeEnum.InnerEnum.Duration:
			Duration d = (Duration) res.ConvertToNative(typeof(Duration));
			val.ObjectValue = Any.Pack(d);
			return val;
		  case TypeEnum.InnerEnum.Timestamp:
			Timestamp t = (Timestamp) res.ConvertToNative(typeof(Timestamp));
			val.ObjectValue = Any.Pack(t);
			return val;
		  case TypeEnum.InnerEnum.List:
			Lister l = (Lister) res;
			ListValue elts = new ListValue();
			for (IteratorT i = l.Iterator(); i.HasNext() == BoolT.True;)
			{
			  Val v = i.Next();
			  elts.Values.Add(RefValueToValue(v));
			}

			val.ListValue = elts;
			return val;
		  case TypeEnum.InnerEnum.Map:
			Mapper m = (Mapper) res;
			MapValue elems = new MapValue();
			for (IteratorT i = m.Iterator(); i.HasNext() == BoolT.True;)
			{
			  Val k = i.Next();
			  Val v = m.Get(k);
			  Value kv = RefValueToValue(k);
			  Value vv = RefValueToValue(v);
			  Entry entry = new Entry();
			  entry.Key = kv;
			  entry.Value = vv;
			  elems.Entries.Add(entry);
			}
			val.MapValue = elems;
			return val;
		  case TypeEnum.InnerEnum.Object:
			// Object type
			Message pb = (Message) res.Value();
			// Somehow the conformance tests
			if (pb is ListValue)
			{
			  val.ListValue = (ListValue) pb;
			}
			else if (pb is MapValue)
			{
			  val.MapValue = (MapValue) pb;
			}
			else
			{
			  val.ObjectValue = Any.Pack(pb);
			}

			return val;
		  default:
			throw new System.InvalidOperationException(string.Format("Unknown {0}", res.Type().TypeEnum()));
		}
	  }

	  /// <summary>
	  /// ExprValueToRefValue converts between exprpb.ExprValue and ref.Val. </summary>
	  internal static Val ExprValueToRefValue(TypeAdapter adapter, ExprValue ev)
	  {
		switch (ev.KindCase)
		{
		  case ExprValue.KindOneofCase.Value:
			return ValueToRefValue(adapter, ev.Value);
		  case ExprValue.KindOneofCase.Error:
			// An error ExprValue is a repeated set of rpcpb.Status
			// messages, with no convention for the status details.
			// To convert this to a types.Err, we need to convert
			// these Status messages to a single string, and be
			// able to decompose that string on output so we can
			// round-trip arbitrary ExprValue messages.
			// TODO(jimlarson) make a convention for this.
			return Err.NewErr("XXX add details later");
		  case ExprValue.KindOneofCase.Unknown:
			return UnknownT.UnknownOf(ev.Unknown.Exprs[0]);
		}
		throw new System.ArgumentException("unknown ExprValue kind " + ev.KindCase);
	  }

	  /// <summary>
	  /// ValueToRefValue converts between exprpb.Value and ref.Val. </summary>
	  internal static Val ValueToRefValue(TypeAdapter adapter, Value v)
	  {
		switch (v.KindCase)
		{
		  case Value.KindOneofCase.NullValue:
			return NullT.NullValue;
		  case Value.KindOneofCase.BoolValue:
			return Types.BoolOf(v.BoolValue);
		  case Value.KindOneofCase.Int64Value:
			return IntT.IntOf(v.Int64Value);
		  case Value.KindOneofCase.Uint64Value:
			return UintT.UintOf(v.Uint64Value);
		  case Value.KindOneofCase.DoubleValue:
			return DoubleT.DoubleOf(v.DoubleValue);
		  case Value.KindOneofCase.StringValue:
			return StringT.StringOf(v.StringValue);
		  case Value.KindOneofCase.BytesValue:
			return BytesT.BytesOf(v.BytesValue.ToByteArray());
		  case Value.KindOneofCase.ObjectValue:
			Any any = v.ObjectValue;
			return adapter(any);
		  case Value.KindOneofCase.MapValue:
			MapValue m = v.MapValue;
			IDictionary<Val, Val> entries = new Dictionary<Val, Val>();
			foreach (MapValue.Types.Entry entry in m.Entries)
			{
			  Val key = ValueToRefValue(adapter, entry.Key);
			  Val pb = ValueToRefValue(adapter, entry.Value);
			  entries[key] = pb;
			}
			return adapter(entries);
		  case Value.KindOneofCase.ListValue:
			ListValue l = v.ListValue;
			IList<Val> elts = l.Values.Select(el => ValueToRefValue(adapter, el)).ToList();
			return adapter(elts);
		  case Value.KindOneofCase.TypeValue:
			string typeName = v.TypeValue;
			Common.Types.Ref.Type tv = Types.GetTypeByName(typeName);
			if (tv != null)
			{
			  return tv;
			}
			return TypeT.NewObjectTypeValue(typeName);
		  default:
			throw new System.ArgumentException("unknown value " + v.KindCase);
		}
	  }
	}

}