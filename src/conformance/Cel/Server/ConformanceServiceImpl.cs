using System;
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
namespace Cel.Server
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.astToCheckedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.astToParsedExpr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.checkedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.CEL.parsedExprToAst;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.newCustomEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Env.newEnv;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.clearMacros;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.container;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.declarations;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.EnvOption.types;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.Library.StdLib;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.BytesT.bytesOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.DoubleT.doubleOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.isError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Err.newErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.TypeT.newObjectTypeValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UintT.uintOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UnknownT.isUnknown;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.projectnessie.cel.common.types.UnknownT.unknownOf;

	using CheckRequest = Google.Api.Expr.V1Alpha1.CheckRequest;
	using CheckResponse = Google.Api.Expr.V1Alpha1.CheckResponse;
	using ConformanceServiceImplBase = Google.Api.Expr.V1Alpha1.ConformanceServiceGrpc.ConformanceServiceImplBase;
	using ErrorSet = Google.Api.Expr.V1Alpha1.ErrorSet;
	using EvalRequest = Google.Api.Expr.V1Alpha1.EvalRequest;
	using EvalResponse = Google.Api.Expr.V1Alpha1.EvalResponse;
	using ExprValue = Google.Api.Expr.V1Alpha1.ExprValue;
	using IssueDetails = Google.Api.Expr.V1Alpha1.IssueDetails;
	using ListValue = Google.Api.Expr.V1Alpha1.ListValue;
	using MapValue = Google.Api.Expr.V1Alpha1.MapValue;
	using Entry = Google.Api.Expr.V1Alpha1.MapValue.Entry;
	using ParseRequest = Google.Api.Expr.V1Alpha1.ParseRequest;
	using ParseResponse = Google.Api.Expr.V1Alpha1.ParseResponse;
	using SourcePosition = Google.Api.Expr.V1Alpha1.SourcePosition;
	using UnknownSet = Google.Api.Expr.V1Alpha1.UnknownSet;
	using Value = Google.Api.Expr.V1Alpha1.Value;
	using Any = com.google.protobuf.Any;
	using ByteString = com.google.protobuf.ByteString;
	using Duration = com.google.protobuf.Duration;
	using Message = com.google.protobuf.Message;
	using Timestamp = com.google.protobuf.Timestamp;
	using Code = com.google.rpc.Code;
	using Status = com.google.rpc.Status;
	using Ast = org.projectnessie.cel.Ast;
	using Env = org.projectnessie.cel.Env;
	using AstIssuesTuple = org.projectnessie.cel.Env.AstIssuesTuple;
	using EnvOption = org.projectnessie.cel.EnvOption;
	using Program = org.projectnessie.cel.Program;
	using Program_EvalResult = org.projectnessie.cel.Program_EvalResult;
	using CELError = org.projectnessie.cel.common.CELError;
	using Err = org.projectnessie.cel.common.types.Err;
	using IteratorT = org.projectnessie.cel.common.types.IteratorT;
	using NullT = org.projectnessie.cel.common.types.NullT;
	using TypeT = org.projectnessie.cel.common.types.TypeT;
	using Types = org.projectnessie.cel.common.types.Types;
	using Type = org.projectnessie.cel.common.types.@ref.Type;
	using TypeAdapter = org.projectnessie.cel.common.types.@ref.TypeAdapter;
	using Val = org.projectnessie.cel.common.types.@ref.Val;
	using Lister = org.projectnessie.cel.common.types.traits.Lister;
	using Mapper = org.projectnessie.cel.common.types.traits.Mapper;

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

	  public override void Parse(ParseRequest request, io.grpc.stub.StreamObserver<ParseResponse> responseObserver)
	  {
		try
		{
		  string sourceText = request.getCelSource();
		  if (sourceText.Trim().Length == 0)
		  {
			throw new System.ArgumentException("No source code.");
		  }

		  // NOTE: syntax_version isn't currently used
		  IList<EnvOption> parseOptions = new List<EnvOption>();
		  if (request.getDisableMacros())
		  {
			parseOptions.Add(clearMacros());
		  }

		  Env env = newEnv(((List<EnvOption>)parseOptions).ToArray());
		  Env.AstIssuesTuple astIss = env.Parse(sourceText);

		  ParseResponse.Builder response = ParseResponse.newBuilder();
		  if (!astIss.HasIssues())
		  {
			// Success
			response.setParsedExpr(astToParsedExpr(astIss.Ast));
		  }
		  else
		  {
			// Failure
			AppendErrors(astIss.Issues.Errors, response.addIssuesBuilder);
		  }

		  responseObserver.onNext(response.build());
		  responseObserver.onCompleted();
		}
		catch (Exception e)
		{
		  responseObserver.onError(io.grpc.Status.fromCode(io.grpc.Status.Code.UNKNOWN).withDescription(Stacktrace(e)).asException());
		}
	  }

	  public override void Check(CheckRequest request, io.grpc.stub.StreamObserver<CheckResponse> responseObserver)
	  {
		try
		{
		  // Build the environment.
		  IList<EnvOption> checkOptions = new List<EnvOption>();
		  if (!request.getNoStdEnv())
		  {
			checkOptions.Add(StdLib());
		  }

		  checkOptions.Add(container(request.getContainer()));
		  checkOptions.Add(declarations(request.getTypeEnvList()));
		  checkOptions.Add(types(com.google.api.expr.test.v1.proto2.TestAllTypesProto.TestAllTypes.getDefaultInstance(), com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()));
		  Env env = newCustomEnv(((List<EnvOption>)checkOptions).ToArray());

		  // Check the expression.
		  Env.AstIssuesTuple astIss = env.Check(parsedExprToAst(request.getParsedExpr()));
		  CheckResponse.Builder resp = CheckResponse.newBuilder();

		  if (!astIss.HasIssues())
		  {
			// Success
			resp.setCheckedExpr(astToCheckedExpr(astIss.Ast));
		  }
		  else
		  {
			// Failure
			AppendErrors(astIss.Issues.Errors, resp.addIssuesBuilder);
		  }

		  responseObserver.onNext(resp.build());
		  responseObserver.onCompleted();
		}
		catch (Exception e)
		{
		  responseObserver.onError(io.grpc.Status.fromCode(io.grpc.Status.Code.UNKNOWN).withDescription(Stacktrace(e)).asException());
		}
	  }

	  public override void Eval(EvalRequest request, io.grpc.stub.StreamObserver<EvalResponse> responseObserver)
	  {
		try
		{
		  Env env = newEnv(container(request.getContainer()), types(com.google.api.expr.test.v1.proto2.TestAllTypesProto.TestAllTypes.getDefaultInstance(), com.google.api.expr.test.v1.proto3.TestAllTypesProto.TestAllTypes.getDefaultInstance()));

		  Program prg;
		  Ast ast;

		  switch (request.getExprKindCase())
		  {
			case PARSED_EXPR:
			  ast = parsedExprToAst(request.getParsedExpr());
			  break;
			case CHECKED_EXPR:
			  ast = checkedExprToAst(request.getCheckedExpr());
			  break;
			default:
			  throw new System.ArgumentException("No expression.");
		  }

		  prg = env.Program(ast);

		  IDictionary<string, object> args = new Dictionary<string, object>();
		  request.getBindingsMap().forEach((name, exprValue) =>
		  {
		  Val refVal = ExprValueToRefValue(env.TypeAdapter, exprValue);
		  args[name] = refVal;
		  });

		  // NOTE: the EvalState is currently discarded
		  Program_EvalResult res = prg.Eval(args);
		  ExprValue resultExprVal;
		  if (!isError(res.Val))
		  {
			resultExprVal = RefValueToExprValue(res.Val);
		  }
		  else
		  {
			Err err = (Err) res.Val;

			if (verboseEvalErrors)
			{
			  System.err.printf("%n" + "Eval error (not necessarily a bug!!!):%n" + "  error: %s%n" + "%s", err, err.HasCause() ? (Stacktrace(err.ToRuntimeException()) + "\n") : "");
			}

			resultExprVal = ExprValue.newBuilder().setError(ErrorSet.newBuilder().addErrors(Status.newBuilder().setMessage(err.ToString()))).build();
		  }

		  EvalResponse.Builder resp = EvalResponse.newBuilder().setResult(resultExprVal);

		  responseObserver.onNext(resp.build());
		  responseObserver.onCompleted();
		}
		catch (Exception e)
		{
		  responseObserver.onError(io.grpc.Status.fromCode(io.grpc.Status.Code.UNKNOWN).withDescription(Stacktrace(e)).asException());
		}
	  }

	  internal static string Stacktrace(Exception t)
	  {
		StringWriter sw = new StringWriter();
		PrintWriter pw = new PrintWriter(sw);
		t.printStackTrace(pw);
		pw.flush();
		return sw.ToString();
	  }

	  /// <summary>
	  /// appendErrors converts the errors from errs to Status messages and appends them to the list of
	  /// issues.
	  /// </summary>
	  internal static void AppendErrors(IList<CELError> errs, System.Func<Status.Builder> builderSupplier)
	  {
		errs.ForEach(e => ErrToStatus(e, IssueDetails.Severity.ERROR, builderSupplier()));
	  }

	  /// <summary>
	  /// ErrToStatus converts an Error to a Status message with the given severity. </summary>
	  internal static void ErrToStatus(CELError e, IssueDetails.Severity severity, Status.Builder status)
	  {
		IssueDetails.Builder detail = IssueDetails.newBuilder().setSeverity(severity).setPosition(SourcePosition.newBuilder().setLine(e.Location.Line()).setColumn(e.Location.Column()).build());

		status.setCode(Code.INVALID_ARGUMENT_VALUE).setMessage(e.Message).addDetails(Any.pack(detail.build()));
	  }

	  /// <summary>
	  /// RefValueToExprValue converts between ref.Val and exprpb.ExprValue. </summary>
	  internal static ExprValue RefValueToExprValue(Val res)
	  {
		if (isUnknown(res))
		{
		  return ExprValue.newBuilder().setUnknown(UnknownSet.newBuilder().addExprs(res.IntValue())).build();
		}
		Value v = RefValueToValue(res);
		return ExprValue.newBuilder().setValue(v).build();
	  }

	  // TODO(jimlarson): The following conversion code should be moved to
	  //  common/types/provider.go and consolidated/refactored as appropriate.
	  //  In particular, make judicious use of types.NativeToValue().

	  /// <summary>
	  /// RefValueToValue converts between ref.Val and Value. The ref.Val must not be error or unknown.
	  /// </summary>
	  internal static Value RefValueToValue(Val res)
	  {
		switch (res.Type().TypeEnum().innerEnumValue)
		{
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Bool:
			return Value.newBuilder().setBoolValue(res.BooleanValue()).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Bytes:
			return Value.newBuilder().setBytesValue(res.ConvertToNative(typeof(ByteString))).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.double:
			return Value.newBuilder().setDoubleValue(res.ConvertToNative(typeof(Double))).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Int:
			return Value.newBuilder().setInt64Value(res.IntValue()).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Null:
			return Value.newBuilder().setNullValueValue(0).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.String:
			return Value.newBuilder().setStringValue(res.Value().ToString()).build();
		  case Type:
			return Value.newBuilder().setTypeValue(((TypeT) res).TypeName()).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Uint:
			return Value.newBuilder().setUint64Value(res.IntValue()).build();
		  case Duration:
			Duration d = res.ConvertToNative(typeof(Duration));
			return Value.newBuilder().setObjectValue(Any.pack(d)).build();
		  case Timestamp:
			Timestamp t = res.ConvertToNative(typeof(Timestamp));
			return Value.newBuilder().setObjectValue(Any.pack(t)).build();
		  case System.Collections.IList:
			Lister l = (Lister) res;
			ListValue.Builder elts = ListValue.newBuilder();
			for (IteratorT i = l.Iterator(); i.HasNext() == True;)
			{
			  Val v = i.Next();
			  elts.addValues(RefValueToValue(v));
			}
			return Value.newBuilder().setListValue(elts).build();
		  case System.Collections.IDictionary:
			Mapper m = (Mapper) res;
			MapValue.Builder elems = MapValue.newBuilder();
			for (IteratorT i = m.Iterator(); i.HasNext() == True;)
			{
			  Val k = i.Next();
			  Val v = m.Get(k);
			  Value kv = RefValueToValue(k);
			  Value vv = RefValueToValue(v);
			  elems.addEntriesBuilder().setKey(kv).setValue(vv);
			}
			return Value.newBuilder().setMapValue(elems).build();
		  case org.projectnessie.cel.common.types.@ref.TypeEnum.InnerEnum.Object:
			// Object type
			Message pb = (Message) res.Value();
			Value.Builder v = Value.newBuilder();
			// Somehow the conformance tests
			if (pb is ListValue)
			{
			  v.setListValue((ListValue) pb);
			}
			else if (pb is MapValue)
			{
			  v.setMapValue((MapValue) pb);
			}
			else
			{
			  v.setObjectValue(Any.pack(pb));
			}
			return v.build();
		  default:
			throw new System.InvalidOperationException(string.Format("Unknown {0}", res.Type().TypeEnum()));
		}
	  }

	  /// <summary>
	  /// ExprValueToRefValue converts between exprpb.ExprValue and ref.Val. </summary>
	  internal static Val ExprValueToRefValue(TypeAdapter adapter, ExprValue ev)
	  {
		switch (ev.getKindCase())
		{
		  case VALUE:
			return ValueToRefValue(adapter, ev.getValue());
		  case ERROR:
			// An error ExprValue is a repeated set of rpcpb.Status
			// messages, with no convention for the status details.
			// To convert this to a types.Err, we need to convert
			// these Status messages to a single string, and be
			// able to decompose that string on output so we can
			// round-trip arbitrary ExprValue messages.
			// TODO(jimlarson) make a convention for this.
			return newErr("XXX add details later");
		  case UNKNOWN:
			return unknownOf(ev.getUnknown().getExprs(0));
		}
		throw new System.ArgumentException("unknown ExprValue kind " + ev.getKindCase());
	  }

	  /// <summary>
	  /// ValueToRefValue converts between exprpb.Value and ref.Val. </summary>
	  internal static Val ValueToRefValue(TypeAdapter adapter, Value v)
	  {
		switch (v.getKindCase())
		{
		  case NULL_VALUE:
			return NullT.NullValue;
		  case BOOL_VALUE:
			return boolOf(v.getBoolValue());
		  case INT64_VALUE:
			return intOf(v.getInt64Value());
		  case UINT64_VALUE:
			return uintOf(v.getUint64Value());
		  case DOUBLE_VALUE:
			return doubleOf(v.getDoubleValue());
		  case STRING_VALUE:
			return stringOf(v.getStringValue());
		  case BYTES_VALUE:
			return bytesOf(v.getBytesValue().toByteArray());
		  case OBJECT_VALUE:
			Any any = v.getObjectValue();
			return adapter.NativeToValue(any);
		  case MAP_VALUE:
			MapValue m = v.getMapValue();
			IDictionary<Val, Val> entries = new Dictionary<Val, Val>();
			foreach (MapValue.Entry entry in m.getEntriesList())
			{
			  Val key = ValueToRefValue(adapter, entry.getKey());
			  Val pb = ValueToRefValue(adapter, entry.getValue());
			  entries[key] = pb;
			}
			return adapter.NativeToValue(entries);
		  case LIST_VALUE:
			ListValue l = v.getListValue();
			IList<Val> elts = l.getValuesList().Select(el => ValueToRefValue(adapter, el)).ToList();
			return adapter.NativeToValue(elts);
		  case TYPE_VALUE:
			string typeName = v.getTypeValue();
			Type tv = Types.GetTypeByName(typeName);
			if (tv != null)
			{
			  return tv;
			}
			return newObjectTypeValue(typeName);
		  default:
			throw new System.ArgumentException("unknown value " + v.getKindCase());
		}
	  }
	}

}