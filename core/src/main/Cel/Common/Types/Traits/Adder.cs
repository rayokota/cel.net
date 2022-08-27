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
namespace Cel.Common.Types.Traits
{
	using Val = Cel.Common.Types.Ref.Val;

	/// <summary>
	/// Adder interface to support '+' operator overloads. </summary>
	public interface Adder
	{
	  /// <summary>
	  /// Add returns a combination of the current value and other value.
	  /// 
	  /// <para>If the other value is an unsupported type, an error is returned.
	  /// </para>
	  /// </summary>
	  Val Add(Val other);
	}

}