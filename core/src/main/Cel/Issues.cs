using System;
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
namespace Cel
{
	using CELError = global::Cel.Common.CelError;
	using Errors = global::Cel.Common.Errors;
	using Source = global::Cel.Common.Source;

	/// <summary>
	/// Issues defines methods for inspecting the error details of parse and check calls.
	/// 
	/// <para>Note: in the future, non-fatal warnings and notices may be inspectable via the Issues struct.
	/// </para>
	/// </summary>
	public sealed class Issues
	{

	  private readonly Errors errs;

	  private Issues(Errors errs)
	  {
		this.errs = errs;
	  }

	  /// <summary>
	  /// NewIssues returns an Issues struct from a common.Errors object. </summary>
	  public static Issues NewIssues(Errors errs)
	  {
		return new Issues(errs);
	  }

	  /// <summary>
	  /// NewIssues returns an Issues struct from a common.Errors object. </summary>
	  public static Issues NoIssues(Source source)
	  {
		return new Issues(new Errors(source));
	  }

	  /// <summary>
	  /// Err returns an error value if the issues list contains one or more errors. </summary>
	  public Exception Err()
	  {
		if (!errs.HasErrors())
		{
		  return null;
		}
		return new Exception(ToString());
	  }

	  public bool HasIssues()
	  {
		return errs.HasErrors();
	  }

	  /// <summary>
	  /// Errors returns the collection of errors encountered in more granular detail. </summary>
	  public IList<CELError> Errors
	  {
		  get
		  {
			  return errs.GetErrors;
		  }
	  }

	  /// <summary>
	  /// Append collects the issues from another Issues struct into a new Issues object. </summary>
	  public Issues Append(Issues other)
	  {
		return NewIssues(errs.Append(other.Errors));
	  }

	  /// <summary>
	  /// String converts the issues to a suitable display string. </summary>
	  public override string ToString()
	  {
		return errs.ToDisplayString();
	  }
	}

}