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
namespace Cel.Interpreter
{
	using Val = global::Cel.Common.Types.Ref.Val;

	/// <summary>
	/// EvalState tracks the values associated with expression ids during execution. </summary>
	public interface EvalState
	{
	  /// <summary>
	  /// IDs returns the list of ids with recorded values. </summary>
	  long[] Ids();

	  /// <summary>
	  /// Value returns the observed value of the given expression id if found, and a nil false result if
	  /// not.
	  /// </summary>
	  Val Value(long id);

	  /// <summary>
	  /// SetValue sets the observed value of the expression id. </summary>
	  void SetValue(long id, Val v);

	  /// <summary>
	  /// Reset clears the previously recorded expression values. </summary>
	  void Reset();

	  /// <summary>
	  /// NewEvalState returns an EvalState instanced used to observe the intermediate evaluations of an
	  /// expression.
	  /// </summary>
	  static EvalState NewEvalState()
	  {
		return new EvalState_EvalStateImpl();
	  }

	  /// <summary>
	  /// evalState permits the mutation of evaluation state for a given expression id. </summary>
	}

	  public sealed class EvalState_EvalStateImpl : EvalState
	  {
		  internal readonly IDictionary<long, Val> values = new Dictionary<long, Val>();

	/// <summary>
	/// IDs implements the EvalState interface method. </summary>
	public long[] Ids()
	{
	  return values.Keys.Select(l => l).ToArray();
	}

	/// <summary>
	/// Value is an implementation of the EvalState interface method. </summary>
	public Val Value(long id)
	{
	  return values[id];
	}

	/// <summary>
	/// SetValue is an implementation of the EvalState interface method. </summary>
	public void SetValue(long id, Val v)
	{
	  values.Add(id, v);
	}

	/// <summary>
	/// Reset implements the EvalState interface method. </summary>
	public void Reset()
	{
	  values.Clear();
	}
	  }

}