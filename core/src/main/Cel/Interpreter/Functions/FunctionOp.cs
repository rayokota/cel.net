﻿/*
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
namespace Cel.Interpreter.Functions
{
	using Val = global::Cel.Common.Types.Ref.Val;

	/// <summary>
	/// FunctionOp is a function with accepts zero or more arguments and produces an value (as
	/// interface{}) or error as a result.
	/// </summary>
	public delegate Val FunctionOp(params Val[] values);

}