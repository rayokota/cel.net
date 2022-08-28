﻿using System.Collections.Generic;

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
	using Overload = global::Cel.Interpreter.Functions.Overload;

	/// <summary>
	/// Dispatcher resolves function calls to their appropriate overload. </summary>
	public interface Dispatcher
	{
	  /// <summary>
	  /// Add one or more overloads, returning an error if any Overload has the same Overload#Name. </summary>
	  void Add(params Overload[] overloads);

	  /// <summary>
	  /// FindOverload returns an Overload definition matching the provided name. </summary>
	  Overload FindOverload(string overload);

	  /// <summary>
	  /// OverloadIds returns the set of all overload identifiers configured for dispatch. </summary>
	  string[] OverloadIds();

	  /// <summary>
	  /// NewDispatcher returns an empty Dispatcher instance. </summary>
	  static Dispatcher NewDispatcher()
	  {
		return new Dispatcher_DefaultDispatcher(null, new Dictionary<string, Overload>());
	  }

	  /// <summary>
	  /// ExtendDispatcher returns a Dispatcher which inherits the overloads of its parent, and provides
	  /// an isolation layer between built-ins and extension functions which is useful for forward
	  /// compatibility.
	  /// </summary>
	  static Dispatcher ExtendDispatcher(Dispatcher parent)
	  {
		return new Dispatcher_DefaultDispatcher(parent, new Dictionary<string, Overload>());
	  }

	  /// <summary>
	  /// defaultDispatcher struct which contains an overload map. </summary>
	}

	  public sealed class Dispatcher_DefaultDispatcher : Dispatcher
	  {
	internal readonly Dispatcher parent;
	internal readonly IDictionary<string, Overload> overloads;

	internal Dispatcher_DefaultDispatcher(Dispatcher parent, IDictionary<string, Overload> overloads)
	{
	  this.parent = parent;
	  this.overloads = overloads;
	}

	/// <summary>
	/// Add implements the Dispatcher.Add interface method. </summary>
	public void Add(params Overload[] overloads)
	{
	  foreach (Overload o in overloads)
	  {

		// add the overload unless an overload of the same name has already been provided.
		if (this.overloads.ContainsKey(o.@operator))
		{
		  throw new System.ArgumentException(string.Format("overload already exists '{0}'", o.@operator));
		}
		// index the overload by function name.
		this.overloads[o.@operator] = o;
	  }
	}

	/// <summary>
	/// FindOverload implements the Dispatcher.FindOverload interface method. </summary>
	public Overload FindOverload(string overload)
	{
	  Overload o = overloads[overload];
	  if (o != null)
	  {
		return o;
	  }
	  return parent != null ? parent.FindOverload(overload) : null;
	}

	/// <summary>
	/// OverloadIds implements the Dispatcher interface method. </summary>
	public string[] OverloadIds()
	{
	  List<string> r = new List<string>(overloads.Keys);
	  if (parent != null)
	  {
		  r.AddRange(parent.OverloadIds());
	  }
	  return r.ToArray();
	}
	  }

}