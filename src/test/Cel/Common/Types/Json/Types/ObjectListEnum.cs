﻿using System.Collections.Generic;

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
namespace Cel.Common.Types.Json.Types
{
	public class ObjectListEnum
	{
		private IList<ObjectListEnum_Entry> Entries {get; set; }
	}
	  public class ObjectListEnum_Entry
	  {
		  private ClassWithEnum.ClassEnum Type {get; set;  }

	  private 	ClassWithEnum Holder {get; set; } 
	  }

}