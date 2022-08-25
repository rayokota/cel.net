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


	public sealed class Options
	{
	  private readonly int maxRecursionDepth;
	  private readonly int errorRecoveryLimit;
	  private readonly int expressionSizeCodePointLimit;
	  private readonly IDictionary<string, Macro> macros;

	  private Options(int maxRecursionDepth, int errorRecoveryLimit, int expressionSizeCodePointLimit, IDictionary<string, Macro> macros)
	  {
		this.maxRecursionDepth = maxRecursionDepth;
		this.errorRecoveryLimit = errorRecoveryLimit;
		this.expressionSizeCodePointLimit = expressionSizeCodePointLimit;
		this.macros = macros;
	  }

	  public int MaxRecursionDepth
	  {
		  get
		  {
			return maxRecursionDepth;
		  }
	  }

	  public int ErrorRecoveryLimit
	  {
		  get
		  {
			return errorRecoveryLimit;
		  }
	  }

	  public int ExpressionSizeCodePointLimit
	  {
		  get
		  {
			return expressionSizeCodePointLimit;
		  }
	  }

	  public Macro getMacro(string name)
	  {
		return macros[name];
	  }

	  public static Builder builder()
	  {
		return new Builder();
	  }

	  public sealed class Builder
	  {
//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
		internal readonly IDictionary<string, Macro> macros_Conflict = new Dictionary<string, Macro>();
//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
		internal int maxRecursionDepth_Conflict = 250;
//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
		internal int errorRecoveryLimit_Conflict = 30;
//JAVA TO C# CONVERTER NOTE: Field name conflicts with a method name of the current type:
		internal int expressionSizeCodePointLimit_Conflict = 100_000;

		internal Builder()
		{
		}

		public Builder maxRecursionDepth(int maxRecursionDepth)
		{
		  if (maxRecursionDepth < -1)
		  {
			throw new System.ArgumentException(string.Format("max recursion depth must be greater than or equal to -1: {0:D}", maxRecursionDepth));
		  }
		  else if (maxRecursionDepth == -1)
		  {
			maxRecursionDepth = int.MaxValue;
		  }
		  this.maxRecursionDepth_Conflict = maxRecursionDepth;
		  return this;
		}

		public Builder errorRecoveryLimit(int errorRecoveryLimit)
		{
		  if (errorRecoveryLimit < -1)
		  {
			throw new System.ArgumentException(string.Format("error recovery limit must be greater than or equal to -1: {0:D}", errorRecoveryLimit));
		  }
		  else if (errorRecoveryLimit == -1)
		  {
			errorRecoveryLimit = int.MaxValue;
		  }
		  this.errorRecoveryLimit_Conflict = errorRecoveryLimit;
		  return this;
		}

		public Builder expressionSizeCodePointLimit(int expressionSizeCodePointLimit)
		{
		  if (expressionSizeCodePointLimit < -1)
		  {
			throw new System.ArgumentException(string.Format("expression size code point limit must be greater than or equal to -1: {0:D}", expressionSizeCodePointLimit));
		  }
		  else if (expressionSizeCodePointLimit == -1)
		  {
			expressionSizeCodePointLimit = int.MaxValue;
		  }
		  this.expressionSizeCodePointLimit_Conflict = expressionSizeCodePointLimit;
		  return this;
		}

		public Builder macros(params Macro[] macros)
		{
		  return this.macros(macros.ToList());
		}

		public Builder macros(IList<Macro> macros)
		{
		  foreach (Macro macro in macros)
		  {
			this.macros_Conflict[macro.macroKey()] = macro;
		  }
		  return this;
		}

		public Options build()
		{
		  return new Options(maxRecursionDepth_Conflict, errorRecoveryLimit_Conflict, expressionSizeCodePointLimit_Conflict, new Dictionary<string, Macro>(macros_Conflict));
		}
	  }
	}

}