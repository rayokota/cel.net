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
namespace Cel.Checker
{
	using Decl = Google.Api.Expr.V1Alpha1.Decl;

	/// <summary>
	/// Scopes represents nested Decl sets where the Scopes value contains a Groups containing all
	/// identifiers in scope and an optional parent representing outer scopes. Each Groups value is a
	/// mapping of names to Decls in the ident and function namespaces. Lookups are performed such that
	/// bindings in inner scopes shadow those in outer scopes.
	/// </summary>
	public sealed class Scopes
	{
	  private readonly Scopes parent;
	  private readonly Group scopes;

	  private Scopes(Scopes parent, Group scopes)
	  {
		this.parent = parent;
		this.scopes = scopes;
	  }

	  /// <summary>
	  /// NewScopes creates a new, empty Scopes. Some operations can't be safely performed until a Group
	  /// is added with Push.
	  /// </summary>
	  public static Scopes NewScopes()
	  {
		return new Scopes(null, NewGroup());
	  }

	  /// <summary>
	  /// Push creates a new Scopes value which references the current Scope as its parent. </summary>
	  public Scopes Push()
	  {
		return new Scopes(this, NewGroup());
	  }

	  /// <summary>
	  /// Pop returns the parent Scopes value for the current scope, or the current scope if the parent
	  /// is nil.
	  /// </summary>
	  public Scopes Pop()
	  {
		if (parent != null)
		{
		  return parent;
		}
		// TODO: Consider whether this should be an error / panic.
		return this;
	  }

	  /// <summary>
	  /// AddIdent adds the ident Decl in the current scope. Note: If the name collides with an existing
	  /// identifier in the scope, the Decl is overwritten.
	  /// </summary>
	  public void AddIdent(Decl decl)
	  {
		scopes.idents[decl.Name] = decl;
	  }

	  /// <summary>
	  /// FindIdent finds the first ident Decl with a matching name in Scopes, or nil if one cannot be
	  /// found. Note: The search is performed from innermost to outermost.
	  /// </summary>
	  public Decl FindIdent(string name)
	  {
		Decl ident = scopes.idents[name];
		if (ident != null)
		{
		  return ident;
		}
		if (parent != null)
		{
		  return parent.FindIdent(name);
		}
		return null;
	  }

	  /// <summary>
	  /// FindIdentInScope finds the first ident Decl with a matching name in the current Scopes value,
	  /// or nil if one does not exist. Note: The search is only performed on the current scope and does
	  /// not search outer scopes.
	  /// </summary>
	  public Decl FindIdentInScope(string name)
	  {
		return scopes.idents[name];
	  }

	  /// <summary>
	  /// AddFunction adds the function Decl to the current scope. Note: Any previous entry for a
	  /// function in the current scope with the same name is overwritten.
	  /// </summary>
	  public void AddFunction(Decl fn)
	  {
		scopes.functions[fn.Name] = fn;
	  }

	  /// <summary>
	  /// FindFunction finds the first function Decl with a matching name in Scopes. The search is
	  /// performed from innermost to outermost. Returns nil if no such function in Scopes.
	  /// </summary>
	  public Decl FindFunction(string name)
	  {
		Decl ident = scopes.functions[name];
		if (ident != null)
		{
		  return ident;
		}
		if (parent != null)
		{
		  return parent.FindFunction(name);
		}
		return null;
	  }

	  public Decl UpdateFunction(string name, Decl ident)
	  {
		if (scopes.functions.ContainsKey(name))
		{
		  scopes.functions[name] = ident;
		}
		else
		{
		  if (parent != null)
		  {
			return parent.UpdateFunction(name, ident);
		  }
		}
		return null;
	  }

	  /// <summary>
	  /// Group is a set of Decls that is pushed on or popped off a Scopes as a unit. Contains separate
	  /// namespaces for idenifier and function Decls. (Should be named "Scope" perhaps?)
	  /// </summary>
	  public sealed class Group
	  {
		internal readonly IDictionary<string, Decl> idents;
		internal readonly IDictionary<string, Decl> functions;

		internal Group(IDictionary<string, Decl> idents, IDictionary<string, Decl> functions)
		{
		  this.idents = idents;
		  this.functions = functions;
		}
	  }

	  internal static Group NewGroup()
	  {
		return new Group(new Dictionary<string, Decl>(), new Dictionary<string, Decl>());
	  }
	}

}