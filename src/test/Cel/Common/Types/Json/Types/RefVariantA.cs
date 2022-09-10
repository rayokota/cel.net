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
namespace Cel.Common.Types.Json.Types
{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Value.Immutable(prehash = true) @JsonSerialize(as = ImmutableRefVariantA.class) @JsonDeserialize(as = ImmutableRefVariantA.class) @JsonTypeName("A") public interface RefVariantA extends RefBase
	public class RefVariantA : RefBase
	{
		public virtual string Type { get; } = "A";
		
	  public virtual string Name {get; set; }

	  public virtual string Hash {get; set; }
	}

}