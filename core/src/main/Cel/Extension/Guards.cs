using System;

/*
 * Copyright (C) 2022 The Authors of CEL-Java
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
namespace Cel.extension
{
	using Err = global::Cel.common.types.Err;
	using IntT = global::Cel.common.types.IntT;
	using ListT = global::Cel.common.types.ListT;
	using StringT = global::Cel.common.types.StringT;
	using BinaryOp = global::Cel.interpreter.functions.BinaryOp;
	using FunctionOp = global::Cel.interpreter.functions.FunctionOp;
	using UnaryOp = global::Cel.interpreter.functions.UnaryOp;

	/// <summary>
	/// function invocation guards for common call signatures within extension functions. </summary>
	public sealed class Guards
	{

	  private Guards()
	  {
	  }

	  public static BinaryOp CallInStrIntOutStr(System.Func<string, int, string> func)
	  {
		return (lhs, rhs) =>
		{
	  try
	  {
		return StringT.StringOf(func(((string) lhs.value()), GetIntValue((IntT) rhs)));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static FunctionOp CallInStrIntIntOutStr(TriFunction<string, int, int, string> func)
	  {
		return values =>
		{
	  try
	  {
		return StringT.StringOf(func.Apply(((string) values[0].value()), (GetIntValue((IntT) values[1])), (GetIntValue((IntT) values[2]))));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static BinaryOp CallInStrStrOutInt(System.Func<string, string, int> func)
	  {
		return (lhs, rhs) =>
		{
	  try
	  {
		return IntT.IntOf(func(((string) lhs.value()), ((string) rhs.value())));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static FunctionOp CallInStrStrIntOutInt(TriFunction<string, string, int, int> func)
	  {
		return values =>
		{
	  try
	  {
		return IntT.IntOf(func.Apply(((string) values[0].value()), ((string) values[1].value()), (GetIntValue((IntT) values[2]))));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static BinaryOp CallInStrStrOutStrArr(System.Func<string, string, string[]> func)
	  {
		return (lhs, rhs) =>
		{
	  try
	  {
		return ListT.NewStringArrayList(func(((string) lhs.value()), ((string) rhs.value())));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static FunctionOp CallInStrStrIntOutStrArr(TriFunction<string, string, int, string[]> func)
	  {
		return values =>
		{
	  try
	  {
		return ListT.NewStringArrayList(func.Apply(((string) values[0].value()), ((string) values[1].value()), GetIntValue((IntT) values[2])));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static FunctionOp CallInStrStrStrOutStr(TriFunction<string, string, string, string> func)
	  {
		return values =>
		{
	  try
	  {
		return StringT.StringOf(func.Apply(((string) values[0].value()), ((string) values[1].value()), ((string) values[2].value())));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static FunctionOp CallInStrStrStrIntOutStr(QuadFunction<string, string, string, int, string> func)
	  {
		return values =>
		{
	  try
	  {
		return StringT.StringOf(func(((string) values[0].value()), ((string) values[1].value()), ((string) values[2].value()), GetIntValue((IntT) values[3])));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static UnaryOp CallInStrOutStr(System.Func<string, string> func)
	  {
		return val =>
		{
	  try
	  {
		return StringT.StringOf(func(((string) val.value())));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static UnaryOp CallInStrArrayOutStr(System.Func<string[], string> func)
	  {
		return val =>
		{
	  try
	  {
		object[] objects = (object[]) val.value();
		return StringT.StringOf(func(Arrays.CopyOf(objects, objects.Length, typeof(string[]))));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  public static BinaryOp CallInStrArrayStrOutStr(System.Func<string[], string, string> func)
	  {
		return (lhs, rhs) =>
		{
	  try
	  {
		object[] objects = (object[]) lhs.value();
		return StringT.StringOf(func(Arrays.CopyOf(objects, objects.Length, typeof(string[])), ((string) rhs.value())));
	  }
	  catch (Exception e)
	  {
		return Err.NewErr(e, "%s", e.Message);
	  }
		};
	  }

	  private static int GetIntValue(IntT value)
	  {
		long? longValue = (long?) value.Value();
		if (longValue > int.MaxValue || (longValue < int.MinValue))
		{
		  throw new Exception(string.Format("Integer {0:D} value overflow", longValue));
		}
		return longValue.Value;
	  }
	}

}