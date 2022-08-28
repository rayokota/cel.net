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
namespace Cel.Common.Types.Pb
{
	using Message = Google.Protobuf.IMessage;

	/// <summary>
	/// description is a private interface used to make it convenient to perform type unwrapping at the
	/// TypeDescription or FieldDescription level.
	/// </summary>
	public abstract class Description
	{
	  /// <summary>
	  /// Zero returns an empty immutable protobuf message when the description is a protobuf message
	  /// type.
	  /// </summary>
	  public abstract Message Zero();
	}

}