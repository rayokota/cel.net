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
namespace Cel.Interpreter
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.False;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.BoolT.True;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.indexOutOfBoundsException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.isError;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.maybeNoSuchOverloadErr;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchAttributeException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchKey;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchKeyException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.noSuchOverload;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Err.throwErrorAsIllegalStateException;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.IntT.intOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.StringT.stringOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.Types.boolOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UintT.uintOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Common.Types.UnknownT.isUnknown;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Cel.Interpreter.Coster.costOf;

	using Type = Google.Api.Expr.V1Alpha1.Type;
	using Container = Cel.Common.Containers.Container;
	using Err = Cel.Common.Types.Err;
	using NullT = Cel.Common.Types.NullT;
	using FieldType = Cel.Common.Types.Ref.FieldType;
	using TypeAdapter = Cel.Common.Types.Ref.TypeAdapter;
	using TypeProvider = Cel.Common.Types.Ref.TypeProvider;
	using Val = Cel.Common.Types.Ref.Val;
	using Indexer = Cel.Common.Types.Traits.Indexer;
	using Mapper = Cel.Common.Types.Traits.Mapper;
	using QualifierValueEquator = Cel.Interpreter.AttributePattern.QualifierValueEquator;

	/// <summary>
	/// AttributeFactory provides methods creating Attribute and Qualifier values. </summary>
	public interface AttributeFactory
	{
	  /// <summary>
	  /// AbsoluteAttribute creates an attribute that refers to a top-level variable name.
	  /// 
	  /// <para>Checked expressions generate absolute attribute with a single name. Parse-only expressions
	  /// may have more than one possible absolute identifier when the expression is created within a
	  /// container, e.g. package or namespace.
	  /// 
	  /// </para>
	  /// <para>When there is more than one name supplied to the AbsoluteAttribute call, the names must be
	  /// in CEL's namespace resolution order. The name arguments provided here are returned in the same
	  /// order as they were provided by the NamespacedAttribute CandidateVariableNames method.
	  /// </para>
	  /// </summary>
	  AttributeFactory_NamespacedAttribute AbsoluteAttribute(long id, params string[] names);

	  /// <summary>
	  /// ConditionalAttribute creates an attribute with two Attribute branches, where the Attribute that
	  /// is resolved depends on the boolean evaluation of the input 'expr'.
	  /// </summary>
	  AttributeFactory_Attribute ConditionalAttribute(long id, Interpretable expr, AttributeFactory_Attribute t, AttributeFactory_Attribute f);

	  /// <summary>
	  /// MaybeAttribute creates an attribute that refers to either a field selection or a namespaced
	  /// variable name.
	  /// 
	  /// <para>Only expressions which have not been type-checked may generate oneof attributes.
	  /// </para>
	  /// </summary>
	  AttributeFactory_Attribute MaybeAttribute(long id, string name);

	  /// <summary>
	  /// RelativeAttribute creates an attribute whose value is a qualification of a dynamic computation
	  /// rather than a static variable reference.
	  /// </summary>
	  AttributeFactory_Attribute RelativeAttribute(long id, Interpretable operand);

	  /// <summary>
	  /// NewQualifier creates a qualifier on the target object with a given value.
	  /// 
	  /// <para>The 'val' may be an Attribute or any proto-supported map key type: bool, int, string, uint.
	  /// 
	  /// </para>
	  /// <para>The qualifier may consider the object type being qualified, if present. If absent, the
	  /// qualification should be considered dynamic and the qualification should still work, though it
	  /// may be sub-optimal.
	  /// </para>
	  /// </summary>
	  AttributeFactory_Qualifier NewQualifier(Type objType, long qualID, object val);

	  /// <summary>
	  /// Qualifier marker interface for designating different qualifier values and where they appear
	  /// within field selections and index call expressions (`_[_]`).
	  /// </summary>

	  /// <summary>
	  /// ConstantQualifier interface embeds the Qualifier interface and provides an option to inspect
	  /// the qualifier's constant value.
	  /// 
	  /// <para>Non-constant qualifiers are of Attribute type.
	  /// </para>
	  /// </summary>

	  /// <summary>
	  /// Attribute values are a variable or value with an optional set of qualifiers, such as field,
	  /// key, or index accesses.
	  /// </summary>

	  /// <summary>
	  /// NamespacedAttribute values are a variable within a namespace, and an optional set of qualifiers
	  /// such as field, key, or index accesses.
	  /// </summary>

	  /// <summary>
	  /// NewAttributeFactory returns a default AttributeFactory which is produces Attribute values
	  /// capable of resolving types by simple names and qualify the values using the supported qualifier
	  /// types: bool, int, string, and uint.
	  /// </summary>
	  static AttributeFactory NewAttributeFactory(Container cont, TypeAdapter a, TypeProvider p)
	  {
		return new AttributeFactory_AttrFactory(cont, a, p);
	  }

	  static AttributeFactory_Qualifier NewQualifierStatic(TypeAdapter adapter, long id, object v)
	  {
		if (v is Attribute)
		{
		  return new AttributeFactory_AttrQualifier(id, (AttributeFactory_Attribute) v);
		}

		System.Type c = v.GetType();

		if (v is Val)
		{
		  Val val = (Val) v;
		  switch (val.Type().TypeEnum())
		  {
			case string:
			  return new AttributeFactory_StringQualifier(id, (string) val.value(), val, adapter);
			case Int:
			  return new AttributeFactory_IntQualifier(id, val.intValue(), val, adapter);
			case Uint:
			  return new AttributeFactory_UintQualifier(id, val.intValue(), val, adapter);
			case Bool:
			  return new AttributeFactory_BoolQualifier(id, val.booleanValue(), val, adapter);
		  }
		}

		if (c == String.class)
		{
		  return new AttributeFactory_StringQualifier(id, (string) v, stringOf((string) v), adapter);
		}
		if (c == ULong.class)
		{
		  long l = ((ULong) v).longValue();
		  return new AttributeFactory_UintQualifier(id, l, uintOf(l), adapter);
		}
		if ((c == Byte.class) || (c == Short.class) || (c == Integer.class) || (c == Long.class))
		{
		  long i = ((Number) v).longValue();
		  return new AttributeFactory_IntQualifier(id, i, intOf(i), adapter);
		}
		if (c == Boolean.class)
		{
		  bool b = (bool?) v;
		  return new AttributeFactory_BoolQualifier(id, b, boolOf(b), adapter);
		}

		throw new System.InvalidOperationException(String.format("invalid qualifier type: %s", v.getClass().getName()));
	  }

	  /// <summary>
	  /// fieldQualifier indicates that the qualification is a well-defined field with a known field
	  /// type. When the field type is known this can be used to improve the speed and efficiency of
	  /// field resolution.
	  /// </summary>

	  /// <summary>
	  /// refResolve attempts to convert the value to a CEL value and then uses reflection methods to try
	  /// and resolve the qualifier.
	  /// </summary>
	  static Val RefResolve(TypeAdapter adapter, Val idx, object obj)
	  {
		Val celVal = adapter.nativeToValue(obj);
		if (celVal instanceof Mapper)
		{
		  Mapper mapper = (Mapper) celVal;
		  Val elem = mapper.find(idx);
		  if (elem == null)
		  {
			return noSuchKey(idx);
		  }
		  return elem;
		}
		if (celVal instanceof Indexer)
		{
		  Indexer indexer = (Indexer) celVal;
		  return indexer.get(idx);
		}
		if (isUnknown(celVal))
		{
		  return celVal;
		}
		// TODO: If the types.Err value contains more than just an error message at some point in the
		//  future, then it would be reasonable to return error values as ref.Val types rather than
		//  simple go error types.
		throwErrorAsIllegalStateException(celVal);
		return noSuchOverload(celVal, "ref-resolve", null);
	  }
	}

	  public interface AttributeFactory_Qualifier
	  {
	/// <summary>
	/// ID where the qualifier appears within an expression. </summary>
	long Id();

	/// <summary>
	/// Qualify performs a qualification, e.g. field selection, on the input object and returns the
	/// value or error that results.
	/// </summary>
	object Qualify(Activation vars, object obj);
	  }

	  public interface AttributeFactory_ConstantQualifier : AttributeFactory_Qualifier
	  {
	Val Value();
	  }

	  public interface AttributeFactory_ConstantQualifierEquator : QualifierValueEquator, AttributeFactory_ConstantQualifier
	  {
	  }

	  public interface AttributeFactory_Attribute : AttributeFactory_Qualifier
	  {
	/// <summary>
	/// AddQualifier adds a qualifier on the Attribute or error if the qualification is not a valid
	/// qualifier type.
	/// </summary>
	AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier q);

	/// <summary>
	/// Resolve returns the value of the Attribute given the current Activation. </summary>
	object Resolve(Activation a);
	  }

	  public interface AttributeFactory_NamespacedAttribute : AttributeFactory_Attribute
	  {
	/// <summary>
	/// CandidateVariableNames returns the possible namespaced variable names for this Attribute in
	/// the CEL namespace resolution order.
	/// </summary>
	string[] CandidateVariableNames();

	/// <summary>
	/// Qualifiers returns the list of qualifiers associated with the Attribute.s </summary>
	IList<AttributeFactory_Qualifier> Qualifiers();

	/// <summary>
	/// TryResolve attempts to return the value of the attribute given the current Activation. If an
	/// error is encountered during attribute resolution, it will be returned immediately. If the
	/// attribute cannot be resolved within the Activation, the result must be: `nil`, `false`,
	/// `nil`.
	/// </summary>
	object TryResolve(Activation a);
	  }

	  public sealed class AttributeFactory_AttrFactory : AttributeFactory
	  {
	internal readonly Container container;
	internal readonly TypeAdapter adapter;
	internal readonly TypeProvider provider;

	internal AttributeFactory_AttrFactory(Container container, TypeAdapter adapter, TypeProvider provider)
	{
	  this.container = container;
	  this.adapter = adapter;
	  this.provider = provider;
	}

	/// <summary>
	/// AbsoluteAttribute refers to a variable value and an optional qualifier path.
	/// 
	/// <para>The namespaceNames represent the names the variable could have based on namespace
	/// resolution rules.
	/// </para>
	/// </summary>
	public AttributeFactory_NamespacedAttribute AbsoluteAttribute(long id, params string[] names)
	{
	  return new AttributeFactory_AbsoluteAttribute(id, names, new List<AttributeFactory_Qualifier>(), adapter, provider, this);
	}

	/// <summary>
	/// ConditionalAttribute supports the case where an attribute selection may occur on a
	/// conditional expression, e.g. (cond ? a : b).c
	/// </summary>
	public AttributeFactory_Attribute ConditionalAttribute(long id, Interpretable expr, AttributeFactory_Attribute t, AttributeFactory_Attribute f)
	{
	  return new AttributeFactory_ConditionalAttribute(id, expr, t, f, adapter, this);
	}

	/// <summary>
	/// MaybeAttribute collects variants of unchecked AbsoluteAttribute values which could either be
	/// direct variable accesses or some combination of variable access with qualification.
	/// </summary>
	public AttributeFactory_Attribute MaybeAttribute(long id, string name)
	{
	  IList<AttributeFactory_NamespacedAttribute> attrs = new List<AttributeFactory_NamespacedAttribute>();
	  attrs.Add(AbsoluteAttribute(id, container.ResolveCandidateNames(name)));
	  return new AttributeFactory_MaybeAttribute(id, attrs, adapter, provider, this);
	}

	/// <summary>
	/// RelativeAttribute refers to an expression and an optional qualifier path. </summary>
	public AttributeFactory_Attribute RelativeAttribute(long id, Interpretable operand)
	{
	  return new AttributeFactory_RelativeAttribute(id, operand, new List<AttributeFactory_Qualifier>(), adapter, this);
	}

	/// <summary>
	/// NewQualifier is an implementation of the AttributeFactory interface. </summary>
	public AttributeFactory_Qualifier NewQualifier(Type objType, long qualID, object val)
	{
	  // Before creating a new qualifier check to see if this is a protobuf message field access.
	  // If so, use the precomputed GetFrom qualification method rather than the standard
	  // stringQualifier.
	  if (val is string)
	  {
		string str = (string) val;
		if (objType != null && !objType.getMessageType().isEmpty())
		{
		  FieldType ft = provider.FindFieldType(objType.getMessageType(), str);
		  if (ft != null && ft.isSet != null && ft.getFrom != null)
		  {
			return new AttributeFactory_FieldQualifier(qualID, str, ft, adapter);
		  }
		}
	  }
	  return NewQualifierStatic(adapter, qualID, val);
	}

	public override string ToString()
	{
	  return "AttrFactory{" + "container=" + container + ", adapter=" + adapter + ", provider=" + provider + '}';
	}
	  }

	  public sealed class AttributeFactory_AbsoluteAttribute : AttributeFactory.Qualifier, AttributeFactory.NamespacedAttribute, Coster
	  {
	internal readonly long id;
	/// <summary>
	/// namespaceNames represent the names the variable could have based on declared container
	/// (package) of the expression.
	/// </summary>
	internal readonly string[] namespaceNames;

	internal readonly IList<AttributeFactory_Qualifier> qualifiers;
	internal readonly TypeAdapter adapter;
	internal readonly TypeProvider provider;
	internal readonly AttributeFactory fac;

	internal AttributeFactory_AbsoluteAttribute(long id, string[] namespaceNames, IList<AttributeFactory_Qualifier> qualifiers, TypeAdapter adapter, TypeProvider provider, AttributeFactory fac)
	{
	  this.id = id;
	  this.namespaceNames = Objects.requireNonNull(namespaceNames);
	  this.qualifiers = Objects.requireNonNull(qualifiers);
	  this.adapter = Objects.requireNonNull(adapter);
	  this.provider = Objects.requireNonNull(provider);
	  this.fac = Objects.requireNonNull(fac);
	}

	/// <summary>
	/// ID implements the Attribute interface method. </summary>
	public override long Id()
	{
	  return id;
	}

	/// <summary>
	/// Cost implements the Coster interface method. </summary>
	public Coster_Cost Cost()
	{
	  long min = 0L;
	  long max = 0L;
	  foreach (AttributeFactory_Qualifier q in qualifiers)
	  {
		Coster_Cost qc = Coster_Cost.EstimateCost(q);
		min += qc.min;
		max += qc.max;
	  }
	  min++; // For object retrieval.
	  max++;
	  return CostOf(min, max);
	}

	/// <summary>
	/// AddQualifier implements the Attribute interface method. </summary>
	public override AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier q)
	{
	  qualifiers.Add(q);
	  return this;
	}

	/// <summary>
	/// CandidateVariableNames implements the NamespaceAttribute interface method. </summary>
	public override string[] CandidateVariableNames()
	{
	  return namespaceNames;
	}

	/// <summary>
	/// Qualifiers returns the list of Qualifier instances associated with the namespaced attribute.
	/// </summary>
	public override IList<AttributeFactory_Qualifier> Qualifiers()
	{
	  return qualifiers;
	}

	/// <summary>
	/// Qualify is an implementation of the Qualifier interface method. </summary>
	public override object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  object val = Resolve(vars);
	  if (isUnknown(val))
	  {
		return val;
	  }
	  AttributeFactory_Qualifier qual = fac.NewQualifier(null, id, val);
	  return qual.Qualify(vars, obj);
	}

	/// <summary>
	/// Resolve returns the resolved Attribute value given the Activation, or error if the Attribute
	/// variable is not found, or if its Qualifiers cannot be applied successfully.
	/// </summary>
	public override object Resolve(Cel.Interpreter.Activation vars)
	{
	  object obj = TryResolve(vars);
	  if (obj == null)
	  {
		throw noSuchAttributeException(this);
	  }
	  return obj;
	}

	/// <summary>
	/// TryResolve iterates through the namespaced variable names until one is found within the
	/// Activation or TypeProvider.
	/// 
	/// <para>If the variable name cannot be found as an Activation variable or in the TypeProvider as a
	/// type, then the result is `nil`, `false`, `nil` per the interface requirement.
	/// </para>
	/// </summary>
	public override object TryResolve(Cel.Interpreter.Activation vars)
	{
	  foreach (string nm in namespaceNames)
	  {
		// If the variable is found, process it. Otherwise, wait until the checks to
		// determine whether the type is unknown before returning.
		object op = vars.ResolveName(nm);
		if (op != null)
		{
		  foreach (AttributeFactory_Qualifier qual in qualifiers)
		  {
			object op2 = qual.Qualify(vars, op);
			if (op2 is Err)
			{
			  return op2;
			}
			if (op2 == null)
			{
			  break;
			}
			op = op2;
		  }
		  return op;
		}
		// Attempt to resolve the qualified type name if the name is not a variable identifier.
		Val typ = provider.FindIdent(nm);
		if (typ != null)
		{
		  if (qualifiers.Count == 0)
		  {
			return typ;
		  }
		  throw noSuchAttributeException(this);
		}
	  }
	  return null;
	}

	/// <summary>
	/// String implements the Stringer interface method. </summary>
	public override string ToString()
	{
	  return "id: " + id + ", names: " + "[" + string.Join(", ", namespaceNames) + "]";
	}
	  }

	  public sealed class AttributeFactory_ConditionalAttribute : AttributeFactory.Qualifier, AttributeFactory.Attribute, Coster
	  {
	internal readonly long id;
	internal readonly Interpretable expr;
	internal readonly AttributeFactory_Attribute truthy;
	internal readonly AttributeFactory_Attribute falsy;
	internal readonly TypeAdapter adapter;
	internal readonly AttributeFactory fac;

	internal AttributeFactory_ConditionalAttribute(long id, Interpretable expr, AttributeFactory_Attribute truthy, AttributeFactory_Attribute falsy, TypeAdapter adapter, AttributeFactory fac)
	{
	  this.id = id;
	  this.expr = expr;
	  this.truthy = truthy;
	  this.falsy = falsy;
	  this.adapter = adapter;
	  this.fac = fac;
	}

	/// <summary>
	/// ID is an implementation of the Attribute interface method. </summary>
	public override long Id()
	{
	  return id;
	}

	/// <summary>
	/// Cost provides the heuristic cost of a ternary operation {@code &lt;expr&gt; ? &lt;t&gt; :
	/// &lt;f&gt;}. The cost is computed as {@code cost(expr)} plus the min/max costs of evaluating
	/// either `t` or `f`.
	/// </summary>
	public Coster_Cost Cost()
	{
	  Coster_Cost t = Coster_Cost.EstimateCost(truthy);
	  Coster_Cost f = Coster_Cost.EstimateCost(falsy);
	  Coster_Cost e = Coster_Cost.EstimateCost(expr);
	  return CostOf(e.min + Math.Min(t.min, f.min), e.max + Math.Max(t.max, f.max));
	}

	/// <summary>
	/// AddQualifier appends the same qualifier to both sides of the conditional, in effect managing
	/// the qualification of alternate attributes.
	/// </summary>
	public override AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier qual)
	{
	  truthy.AddQualifier(qual); // just do
	  falsy.AddQualifier(qual); // just do
	  return this;
	}

	/// <summary>
	/// Qualify is an implementation of the Qualifier interface method. </summary>
	public override object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  object val = Resolve(vars);
	  if (isUnknown(val))
	  {
		return val;
	  }
	  AttributeFactory_Qualifier qual = fac.NewQualifier(null, id, val);
	  return qual.Qualify(vars, obj);
	}

	/// <summary>
	/// Resolve evaluates the condition, and then resolves the truthy or falsy branch accordingly.
	/// </summary>
	public override object Resolve(Cel.Interpreter.Activation vars)
	{
	  Val val = expr.Eval(vars);
	  if (val == null)
	  {
		throw noSuchAttributeException(this);
	  }
	  if (isError(val))
	  {
		return null;
	  }
	  if (val == True)
	  {
		return truthy.Resolve(vars);
	  }
	  if (val == False)
	  {
		return falsy.Resolve(vars);
	  }
	  if (isUnknown(val))
	  {
		return val;
	  }
	  return maybeNoSuchOverloadErr(val);
	}

	/// <summary>
	/// String is an implementation of the Stringer interface method. </summary>
	public override string ToString()
	{
	  return string.Format("id: {0:D}, truthy attribute: {1}, falsy attribute: {2}", id, truthy, falsy);
	}
	  }

	  public sealed class AttributeFactory_MaybeAttribute : Coster, AttributeFactory_Attribute, AttributeFactory_Qualifier
	  {
	internal readonly long id;
	internal readonly IList<AttributeFactory_NamespacedAttribute> attrs;
	internal readonly TypeAdapter adapter;
	internal readonly TypeProvider provider;
	internal readonly AttributeFactory fac;

	internal AttributeFactory_MaybeAttribute(long id, IList<AttributeFactory_NamespacedAttribute> attrs, TypeAdapter adapter, TypeProvider provider, AttributeFactory fac)
	{
	  this.id = id;
	  this.attrs = attrs;
	  this.adapter = adapter;
	  this.provider = provider;
	  this.fac = fac;
	}

	/// <summary>
	/// ID is an implementation of the Attribute interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Cost implements the Coster interface method. The min cost is computed as the minimal cost
	/// among all the possible attributes, the max cost ditto.
	/// </summary>
	public Coster_Cost Cost()
	{
	  long min = long.MaxValue;
	  long max = 0L;
	  foreach (AttributeFactory_NamespacedAttribute a in attrs)
	  {
		Coster_Cost ac = Coster_Cost.EstimateCost(a);
		min = Long.min(min, ac.min);
		max = Long.max(max, ac.max);
	  }
	  return CostOf(min, max);
	}

	/// <summary>
	/// AddQualifier adds a qualifier to each possible attribute variant, and also creates a new
	/// namespaced variable from the qualified value.
	/// 
	/// <para>The algorithm for building the maybe attribute is as follows:
	/// 
	/// <ol>
	///   <li>Create a maybe attribute from a simple identifier when it occurs in a parsed-only
	///       expression <br>
	///       <br>
	///       {@code mb = MaybeAttribute(&lt;id&gt;, "a")} <br>
	///       <br>
	///       Initializing the maybe attribute creates an absolute attribute internally which
	///       includes the possible namespaced names of the attribute. In this example, let's assume
	///       we are in namespace 'ns', then the maybe is either one of the following variable names:
	///       <br>
	///       <br>
	///       possible variables names -- ns.a, a
	///   <li>Adding a qualifier to the maybe means that the variable name could be a longer
	///       qualified name, or a field selection on one of the possible variable names produced
	///       earlier: <br>
	///       <br>
	///       {@code mb.AddQualifier("b")} <br>
	///       <br>
	///       possible variables names -- ns.a.b, a.b<br>
	///       possible field selection -- ns.a['b'], a['b']
	/// </ol>
	/// 
	/// If none of the attributes within the maybe resolves a value, the result is an error.
	/// </para>
	/// </summary>
	public override AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier qual)
	{
	  string str = "";
	  bool isStr = false;
	  if (qual is AttributeFactory.ConstantQualifier)
	  {
		AttributeFactory_ConstantQualifier cq = (AttributeFactory_ConstantQualifier) qual;
		object cqv = cq.Value().Value();
		if (cqv is string)
		{
		  str = (string) cqv;
		  isStr = true;
		}
	  }
	  string[] augmentedNames = new string[0];
	  // First add the qualifier to all existing attributes in the oneof.
	  foreach (AttributeFactory_NamespacedAttribute attr in attrs)
	  {
		if (isStr && attr.Qualifiers().Count == 0)
		{
		  string[] candidateVars = attr.CandidateVariableNames();
		  augmentedNames = new string[candidateVars.Length];
		  for (int i = 0; i < candidateVars.Length; i++)
		  {
			string name = candidateVars[i];
			augmentedNames[i] = string.Format("{0}.{1}", name, str);
		  }
		}
		attr.AddQualifier(qual);
	  }
	  // Next, ensure the most specific variable / type reference is searched first.
	  if (attrs.Count == 0)
	  {
		attrs.Add(fac.AbsoluteAttribute(qual.Id(), augmentedNames));
	  }
	  else
	  {
		attrs.Insert(0, fac.AbsoluteAttribute(qual.Id(), augmentedNames));
	  }
	  return this;
	}

	/// <summary>
	/// Qualify is an implementation of the Qualifier interface method. </summary>
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  object val = Resolve(vars);
	  if (isUnknown(val))
	  {
		return val;
	  }
	  AttributeFactory_Qualifier qual = fac.NewQualifier(null, id, val);
	  return qual.Qualify(vars, obj);
	}

	/// <summary>
	/// Resolve follows the variable resolution rules to determine whether the attribute is a
	/// variable or a field selection.
	/// </summary>
	public override object Resolve(Cel.Interpreter.Activation vars)
	{
	  foreach (AttributeFactory_NamespacedAttribute attr in attrs)
	  {
		object obj = attr.TryResolve(vars);
		// If the object was found, return it.
		if (obj != null)
		{
		  return obj;
		}
	  }
	  // Else, produce a no such attribute error.
	  throw noSuchAttributeException(this);
	}

	/// <summary>
	/// String is an implementation of the Stringer interface method. </summary>
	public override string ToString()
	{
	  return string.Format("id: {0}, attributes: {1}", id, attrs);
	}
	  }

	  public sealed class AttributeFactory_RelativeAttribute : Coster, AttributeFactory.Qualifier, AttributeFactory_Attribute
	  {
	internal readonly long id;
	internal readonly Interpretable operand;
	internal readonly IList<AttributeFactory_Qualifier> qualifiers;
	internal readonly TypeAdapter adapter;
	internal readonly AttributeFactory fac;

	internal AttributeFactory_RelativeAttribute(long id, Interpretable operand, IList<AttributeFactory_Qualifier> qualifiers, TypeAdapter adapter, AttributeFactory fac)
	{
	  this.id = id;
	  this.operand = operand;
	  this.qualifiers = qualifiers;
	  this.adapter = adapter;
	  this.fac = fac;
	}

	/// <summary>
	/// ID is an implementation of the Attribute interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Cost implements the Coster interface method. </summary>
	public Coster_Cost Cost()
	{
	  Coster_Cost c = Coster_Cost.EstimateCost(operand);
	  long min = c.min;
	  long max = c.max;
	  foreach (AttributeFactory_Qualifier qual in qualifiers)
	  {
		Coster_Cost q = Coster_Cost.EstimateCost(qual);
		min += q.min;
		max += q.max;
	  }
	  return CostOf(min, max);
	}

	/// <summary>
	/// AddQualifier implements the Attribute interface method. </summary>
	public AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier qual)
	{
	  qualifiers.Add(qual);
	  return this;
	}

	/// <summary>
	/// Qualify is an implementation of the Qualifier interface method. </summary>
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  object val = Resolve(vars);
	  if (isUnknown(val))
	  {
		return val;
	  }
	  AttributeFactory_Qualifier qual = fac.NewQualifier(null, id, val);
	  return qual.Qualify(vars, obj);
	}

	/// <summary>
	/// Resolve expression value and qualifier relative to the expression result. </summary>
	public object Resolve(Cel.Interpreter.Activation vars)
	{
	  // First, evaluate the operand.
	  Val v = operand.Eval(vars);
	  if (isError(v))
	  {
		return null;
	  }
	  if (isUnknown(v))
	  {
		return v;
	  }
	  // Next, qualify it. Qualification handles unkonwns as well, so there's no need to recheck.
	  object obj = v;
	  foreach (AttributeFactory_Qualifier qual in qualifiers)
	  {
		if (obj == null)
		{
		  throw noSuchAttributeException(this);
		}
		obj = qual.Qualify(vars, obj);
		if (obj is Err)
		{
		  return obj;
		}
	  }
	  if (obj == null)
	  {
		throw noSuchAttributeException(this);
	  }
	  return obj;
	}

	/// <summary>
	/// String is an implementation of the Stringer interface method. </summary>
	public override string ToString()
	{
	  return string.Format("id: {0:D}, operand: {1}", id, operand);
	}
	  }

	  public sealed class AttributeFactory_AttrQualifier : Coster, AttributeFactory_Attribute
	  {
	internal readonly long id;
	internal readonly AttributeFactory_Attribute attribute;

	internal AttributeFactory_AttrQualifier(long id, AttributeFactory_Attribute attribute)
	{
	  this.id = id;
	  this.attribute = attribute;
	}

	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.EstimateCost(attribute);
	}

	public AttributeFactory_Attribute AddQualifier(AttributeFactory_Qualifier q)
	{
	  return attribute.AddQualifier(q);
	}

	public object Resolve(Activation a)
	{
	  return attribute.Resolve(a);
	}

	public object Qualify(Activation vars, object obj)
	{
	  return attribute.Qualify(vars, obj);
	}

	public override string ToString()
	{
	  return "AttrQualifier{" + "id=" + id + ", attribute=" + attribute + '}';
	}
	  }

	  public sealed class AttributeFactory_StringQualifier : Coster, AttributeFactory.ConstantQualifierEquator, QualifierValueEquator
	  {
	internal readonly long id;
	internal readonly string value;
	internal readonly Val celValue;
	internal readonly TypeAdapter adapter;

	internal AttributeFactory_StringQualifier(long id, string value, Val celValue, TypeAdapter adapter)
	{
	  this.id = id;
	  this.value = value;
	  this.celValue = celValue;
	  this.adapter = adapter;
	}

	/// <summary>
	/// ID is an implementation of the Qualifier interface method. </summary>
	public override long Id()
	{
	  return id;
	}

	/// <summary>
	/// Qualify implements the Qualifier interface method. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") @Override public Object qualify(Cel.Interpreter.Activation vars, Object obj)
	public override object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  string s = value;
	  if (obj is System.Collections.IDictionary)
	  {
		System.Collections.IDictionary m = (System.Collections.IDictionary) obj;
		obj = m[s];
		if (obj == null)
		{
		  if (m.Contains(s))
		  {
			return NullT.NullValue;
		  }
		  throw noSuchKeyException(s);
		}
	  }
	  else if (isUnknown(obj))
	  {
		return obj;
	  }
	  else
	  {
		return refResolve(adapter, celValue, obj);
	  }
	  return obj;
	}

	/// <summary>
	/// Value implements the ConstantQualifier interface </summary>
	public override Val Value()
	{
	  return celValue;
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.None;
	}

	public bool QualifierValueEquals(object value)
	{
	  if (value is string)
	  {
		return this.value.Equals(value);
	  }
	  return false;
	}

	public override string ToString()
	{
	  return "StringQualifier{" + "id=" + id + ", value='" + value + '\'' + ", celValue=" + celValue + ", adapter=" + adapter + '}';
	}
	  }

	  public sealed class AttributeFactory_IntQualifier : Coster, AttributeFactory_ConstantQualifierEquator
	  {
	internal readonly long id;
	internal readonly long value;
	internal readonly Val celValue;
	internal readonly TypeAdapter adapter;

	internal AttributeFactory_IntQualifier(long id, long value, Val celValue, TypeAdapter adapter)
	{
	  this.id = id;
	  this.value = value;
	  this.celValue = celValue;
	  this.adapter = adapter;
	}

	/// <summary>
	/// ID is an implementation of the Qualifier interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Qualify implements the Qualifier interface method. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") @Override public Object qualify(Cel.Interpreter.Activation vars, Object obj)
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  long i = value;
	  if (obj is System.Collections.IDictionary)
	  {
		System.Collections.IDictionary m = (System.Collections.IDictionary) obj;
		obj = m[i];
		if (obj == null)
		{
		  obj = m[(int) i];
		}
		if (obj == null)
		{
		  if (m.Contains(i) || m.Contains((int) i))
		  {
			return null;
		  }
		  throw noSuchKeyException(i);
		}
		return obj;
	  }
	  if (obj.GetType().IsArray)
	  {
		int l = Array.getLength(obj);
		if (i < 0 || i >= l)
		{
		  throw indexOutOfBoundsException(i);
		}
		obj = Array.get(obj, (int) i);
		return obj;
	  }
	  if (obj is System.Collections.IList)
	  {
		System.Collections.IList list = (System.Collections.IList) obj;
		int l = list.Count;
		if (i < 0 || i >= l)
		{
		  throw indexOutOfBoundsException(i);
		}
		obj = list[(int) i];
		return obj;
	  }
	  if (isUnknown(obj))
	  {
		return obj;
	  }
	  return refResolve(adapter, celValue, obj);
	}

	/// <summary>
	/// Value implements the ConstantQualifier interface </summary>
	public Val Value()
	{
	  return celValue;
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.None;
	}

	public bool QualifierValueEquals(object value)
	{
	  if (value is ULong)
	  {
		return false;
	  }
	  if (value is Number)
	  {
		return this.value == ((Number) value).longValue();
	  }
	  return false;
	}

	public override string ToString()
	{
	  return "IntQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" + adapter + '}';
	}
	  }

	  public sealed class AttributeFactory_UintQualifier : Coster, AttributeFactory_ConstantQualifierEquator
	  {
	internal readonly long id;
	internal readonly long value;
	internal readonly Val celValue;
	internal readonly TypeAdapter adapter;

	internal AttributeFactory_UintQualifier(long id, long value, Val celValue, TypeAdapter adapter)
	{
	  this.id = id;
	  this.value = value;
	  this.celValue = celValue;
	  this.adapter = adapter;
	}

	/// <summary>
	/// ID is an implementation of the Qualifier interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Qualify implements the Qualifier interface method. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") @Override public Object qualify(Cel.Interpreter.Activation vars, Object obj)
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  long i = value;
	  if (obj is System.Collections.IDictionary)
	  {
		System.Collections.IDictionary m = (System.Collections.IDictionary) obj;
		obj = m[ULong.ValueOf(i)];
		if (obj == null)
		{
		  throw noSuchKeyException(i);
		}
		return obj;
	  }
	  if (obj.GetType().IsArray)
	  {
		int l = Array.getLength(obj);
		if (i < 0 && i >= l)
		{
		  throw indexOutOfBoundsException(i);
		}
		obj = Array.get(obj, (int) i);
		return obj;
	  }
	  if (isUnknown(obj))
	  {
		return obj;
	  }
	  return refResolve(adapter, celValue, obj);
	}

	/// <summary>
	/// Value implements the ConstantQualifier interface </summary>
	public Val Value()
	{
	  return celValue;
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.None;
	}

	public bool QualifierValueEquals(object value)
	{
	  if (value is ULong)
	  {
		return this.value == ((ULong) value).LongValue();
	  }
	  return false;
	}

	public override string ToString()
	{
	  return "UintQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" + adapter + '}';
	}
	  }

	  public sealed class AttributeFactory_BoolQualifier : Coster, AttributeFactory_ConstantQualifierEquator
	  {
	internal readonly long id;
	internal readonly bool value;
	internal readonly Val celValue;
	internal readonly TypeAdapter adapter;

	internal AttributeFactory_BoolQualifier(long id, bool value, Val celValue, TypeAdapter adapter)
	{
	  this.id = id;
	  this.value = value;
	  this.celValue = celValue;
	  this.adapter = adapter;
	}

	/// <summary>
	/// ID is an implementation of the Qualifier interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Qualify implements the Qualifier interface method. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") @Override public Object qualify(Cel.Interpreter.Activation vars, Object obj)
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  bool b = value;
	  if (obj is System.Collections.IDictionary)
	  {
		System.Collections.IDictionary m = (System.Collections.IDictionary) obj;
		obj = m[b];
		if (obj == null)
		{
		  if (m.Contains(b))
		  {
			return null;
		  }
		  throw noSuchKeyException(b);
		}
	  }
	  else if (isUnknown(obj))
	  {
		return obj;
	  }
	  else
	  {
		return refResolve(adapter, celValue, obj);
	  }
	  return obj;
	}

	/// <summary>
	/// Value implements the ConstantQualifier interface </summary>
	public Val Value()
	{
	  return celValue;
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.None;
	}

	public bool QualifierValueEquals(object value)
	{
	  if (value is Boolean)
	  {
		return this.value == ((bool?) value).Value;
	  }
	  return false;
	}

	public override string ToString()
	{
	  return "BoolQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" + adapter + '}';
	}
	  }

	  public sealed class AttributeFactory_FieldQualifier : Coster, AttributeFactory_ConstantQualifierEquator
	  {
	internal readonly long id;
	internal readonly string name;
	internal readonly FieldType fieldType;
	internal readonly TypeAdapter adapter;

	internal AttributeFactory_FieldQualifier(long id, string name, FieldType fieldType, TypeAdapter adapter)
	{
	  this.id = id;
	  this.name = name;
	  this.fieldType = fieldType;
	  this.adapter = adapter;
	}

	/// <summary>
	/// ID is an implementation of the Qualifier interface method. </summary>
	public long Id()
	{
	  return id;
	}

	/// <summary>
	/// Qualify implements the Qualifier interface method. </summary>
	public object Qualify(Cel.Interpreter.Activation vars, object obj)
	{
	  if (obj is Val)
	  {
		obj = ((Val) obj).Value();
	  }
	  return fieldType.getFrom(obj);
	}

	/// <summary>
	/// Value implements the ConstantQualifier interface </summary>
	public Val Value()
	{
	  return stringOf(name);
	}

	/// <summary>
	/// Cost returns zero for constant field qualifiers </summary>
	public Coster_Cost Cost()
	{
	  return Coster_Cost.None;
	}

	public bool QualifierValueEquals(object value)
	{
	  if (value is string)
	  {
		return this.name.Equals(value);
	  }
	  return false;
	}

	public override string ToString()
	{
	  return "FieldQualifier{" + "id=" + id + ", name='" + name + '\'' + ", fieldType=" + fieldType + ", adapter=" + adapter + '}';
	}
	  }

}