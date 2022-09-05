using System.Collections;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
using Cel.Common.Types.Traits;
using Container = Cel.Common.Containers.Container;
using Type = Google.Api.Expr.V1Alpha1.Type;

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
namespace Cel.Interpreter;

/// <summary>
///     AttributeFactory provides methods creating Attribute and Qualifier values.
/// </summary>
public interface AttributeFactory
{
    /// <summary>
    ///     AbsoluteAttribute creates an attribute that refers to a top-level variable name.
    ///     <para>
    ///         Checked expressions generate absolute attribute with a single name. Parse-only expressions
    ///         may have more than one possible absolute identifier when the expression is created within a
    ///         container, e.g. package or namespace.
    ///     </para>
    ///     <para>
    ///         When there is more than one name supplied to the AbsoluteAttribute call, the names must be
    ///         in CEL's namespace resolution order. The name arguments provided here are returned in the same
    ///         order as they were provided by the NamespacedAttribute CandidateVariableNames method.
    ///     </para>
    /// </summary>
    NamespacedAttribute AbsoluteAttribute(long id, params string[] names);

    /// <summary>
    ///     ConditionalAttribute creates an attribute with two Attribute branches, where the Attribute that
    ///     is resolved depends on the boolean evaluation of the input 'expr'.
    /// </summary>
    Attribute ConditionalAttribute(long id, Interpretable expr, Attribute t,
        Attribute f);

    /// <summary>
    ///     MaybeAttribute creates an attribute that refers to either a field selection or a namespaced
    ///     variable name.
    ///     <para>
    ///         Only expressions which have not been type-checked may generate oneof attributes.
    ///     </para>
    /// </summary>
    Attribute MaybeAttribute(long id, string name);

    /// <summary>
    ///     RelativeAttribute creates an attribute whose value is a qualification of a dynamic computation
    ///     rather than a static variable reference.
    /// </summary>
    Attribute RelativeAttribute(long id, Interpretable operand);

    /// <summary>
    ///     NewQualifier creates a qualifier on the target object with a given value.
    ///     <para>
    ///         The 'val' may be an Attribute or any proto-supported map key type: bool, int, string, uint.
    ///     </para>
    ///     <para>
    ///         The qualifier may consider the object type being qualified, if present. If absent, the
    ///         qualification should be considered dynamic and the qualification should still work, though it
    ///         may be sub-optimal.
    ///     </para>
    /// </summary>
    Qualifier NewQualifier(Type? objType, long qualID, object val);

    /// <summary>
    ///     Qualifier marker interface for designating different qualifier values and where they appear
    ///     within field selections and index call expressions (`_[_]`).
    /// </summary>
    /// <summary>
    ///     ConstantQualifier interface embeds the Qualifier interface and provides an option to inspect
    ///     the qualifier's constant value.
    ///     <para>
    ///         Non-constant qualifiers are of Attribute type.
    ///     </para>
    /// </summary>
    /// <summary>
    ///     Attribute values are a variable or value with an optional set of qualifiers, such as field,
    ///     key, or index accesses.
    /// </summary>
    /// <summary>
    ///     NamespacedAttribute values are a variable within a namespace, and an optional set of qualifiers
    ///     such as field, key, or index accesses.
    /// </summary>
    /// <summary>
    ///     NewAttributeFactory returns a default AttributeFactory which is produces Attribute values
    ///     capable of resolving types by simple names and qualify the values using the supported qualifier
    ///     types: bool, int, string, and uint.
    /// </summary>
    static AttributeFactory NewAttributeFactory(Container cont, TypeAdapter a, TypeProvider p)
    {
        return new AttrFactory(cont, a, p);
    }

    static Qualifier NewQualifierStatic(TypeAdapter adapter, long id, object v)
    {
        if (v is Attribute)
            return new AttrQualifier(id, (Attribute)v);

        var c = v.GetType();

        if (v is Val)
        {
            var val = (Val)v;
            switch (val.Type().TypeEnum().InnerEnumValue)
            {
                case TypeEnum.InnerEnum.String:
                    return new StringQualifier(id, (string)val.Value(), val, adapter);
                case TypeEnum.InnerEnum.Int:
                    return new IntQualifier(id, val.IntValue(), val, adapter);
                case TypeEnum.InnerEnum.Uint:
                    return new UintQualifier(id, (ulong)val.IntValue(), val, adapter);
                case TypeEnum.InnerEnum.Bool:
                    return new BoolQualifier(id, val.BooleanValue(), val, adapter);
            }
        }

        if (c == typeof(string))
            return new StringQualifier(id, (string)v, StringT.StringOf((string)v), adapter);

        if (c == typeof(uint))
        {
            var l = (uint)v;
            return new UintQualifier(id, l, UintT.UintOf(l), adapter);
        }

        if (c == typeof(ulong))
        {
            var l = (ulong)v;
            return new UintQualifier(id, l, UintT.UintOf(l), adapter);
        }

        if (c == typeof(byte))
        {
            var i = (byte)v;
            return new IntQualifier(id, i, IntT.IntOf(i), adapter);
        }

        if (c == typeof(short))
        {
            var i = (short)v;
            return new IntQualifier(id, i, IntT.IntOf(i), adapter);
        }

        if (c == typeof(int))
        {
            var i = (int)v;
            return new IntQualifier(id, i, IntT.IntOf(i), adapter);
        }

        if (c == typeof(long))
        {
            var i = (long)v;
            return new IntQualifier(id, i, IntT.IntOf(i), adapter);
        }

        if (c == typeof(bool))
        {
            var b = (bool)v;
            return new BoolQualifier(id, b, Types.BoolOf(b), adapter);
        }

        throw new InvalidOperationException(string.Format("invalid qualifier type: {0}",
            v.GetType()));
    }

    /// <summary>
    ///     fieldQualifier indicates that the qualification is a well-defined field with a known field
    ///     type. When the field type is known this can be used to improve the speed and efficiency of
    ///     field resolution.
    /// </summary>
    /// <summary>
    ///     RefResolve attempts to convert the value to a CEL value and then uses reflection methods to try
    ///     and resolve the qualifier.
    /// </summary>
    static Val RefResolve(TypeAdapter adapter, Val idx, object obj)
    {
        var celVal = adapter(obj);
        if (celVal is Mapper)
        {
            var mapper = (Mapper)celVal;
            var elem = mapper.Find(idx);
            if (elem == null) return Err.NoSuchKey(idx);

            return elem;
        }

        if (celVal is Indexer)
        {
            var indexer = (Indexer)celVal;
            return indexer.Get(idx);
        }

        if (UnknownT.IsUnknown(celVal)) return celVal;

        // TODO: If the types.Err value contains more than just an error message at some point in the
        //  future, then it would be reasonable to return error values as ref.Val types rather than
        //  simple go error types.
        Err.ThrowErrorAsIllegalStateException(celVal);
        return Err.NoSuchOverload(celVal, "ref-resolve", null);
    }
}

public interface Qualifier
{
    /// <summary>
    ///     ID where the qualifier appears within an expression.
    /// </summary>
    long Id();

    /// <summary>
    ///     Qualify performs a qualification, e.g. field selection, on the input object and returns the
    ///     value or error that results.
    /// </summary>
    object Qualify(Activation vars, object obj);
}

public interface ConstantQualifier : Qualifier
{
    Val Value();
}

public interface ConstantQualifierEquator : AttributePattern.QualifierValueEquator,
    ConstantQualifier
{
}

public interface Attribute : Qualifier
{
    /// <summary>
    ///     AddQualifier adds a qualifier on the Attribute or error if the qualification is not a valid
    ///     qualifier type.
    /// </summary>
    Attribute AddQualifier(Qualifier q);

    /// <summary>
    ///     Resolve returns the value of the Attribute given the current Activation.
    /// </summary>
    object Resolve(Activation a);
}

public interface NamespacedAttribute : Attribute
{
    /// <summary>
    ///     CandidateVariableNames returns the possible namespaced variable names for this Attribute in
    ///     the CEL namespace resolution order.
    /// </summary>
    string[] CandidateVariableNames();

    /// <summary>
    ///     Qualifiers returns the list of qualifiers associated with the Attribute.s
    /// </summary>
    IList<Qualifier> Qualifiers();

    /// <summary>
    ///     TryResolve attempts to return the value of the attribute given the current Activation. If an
    ///     error is encountered during attribute resolution, it will be returned immediately. If the
    ///     attribute cannot be resolved within the Activation, the result must be: `nil`, `false`,
    ///     `nil`.
    /// </summary>
    object TryResolve(Activation a);
}

public sealed class AttrFactory : AttributeFactory
{
    internal readonly TypeAdapter adapter;
    internal readonly Container container;
    internal readonly TypeProvider provider;

    internal AttrFactory(Container container, TypeAdapter adapter, TypeProvider provider)
    {
        this.container = container;
        this.adapter = adapter;
        this.provider = provider;
    }

    /// <summary>
    ///     AbsoluteAttribute refers to a variable value and an optional qualifier path.
    ///     <para>
    ///         The namespaceNames represent the names the variable could have based on namespace
    ///         resolution rules.
    ///     </para>
    /// </summary>
    public NamespacedAttribute AbsoluteAttribute(long id, params string[] names)
    {
        return new AbsoluteAttribute(id, names, new List<Qualifier>(), adapter,
            provider, this);
    }

    /// <summary>
    ///     ConditionalAttribute supports the case where an attribute selection may occur on a
    ///     conditional expression, e.g. (cond ? a : b).c
    /// </summary>
    public Attribute ConditionalAttribute(long id, Interpretable expr,
        Attribute t, Attribute f)
    {
        return new ConditionalAttribute(id, expr, t, f, adapter, this);
    }

    /// <summary>
    ///     MaybeAttribute collects variants of unchecked AbsoluteAttribute values which could either be
    ///     direct variable accesses or some combination of variable access with qualification.
    /// </summary>
    public Attribute MaybeAttribute(long id, string name)
    {
        IList<NamespacedAttribute> attrs = new List<NamespacedAttribute>();
        attrs.Add(AbsoluteAttribute(id, container.ResolveCandidateNames(name)));
        return new MaybeAttribute(id, attrs, adapter, provider, this);
    }

    /// <summary>
    ///     RelativeAttribute refers to an expression and an optional qualifier path.
    /// </summary>
    public Attribute RelativeAttribute(long id, Interpretable operand)
    {
        return new RelativeAttribute(id, operand, new List<Qualifier>(), adapter,
            this);
    }

    /// <summary>
    ///     NewQualifier is an implementation of the AttributeFactory interface.
    /// </summary>
    public Qualifier NewQualifier(Type? objType, long qualID, object val)
    {
        // Before creating a new qualifier check to see if this is a protobuf message field access.
        // If so, use the precomputed GetFrom qualification method rather than the standard
        // stringQualifier.
        if (val is string)
        {
            var str = (string)val;
            if (objType != null && objType.MessageType.Length > 0)
            {
                var ft = provider.FindFieldType(objType.MessageType, str);
                if (ft != null && ft.isSet != null && ft.getFrom != null)
                    return new FieldQualifier(qualID, str, ft, adapter);
            }
        }

        return AttributeFactory.NewQualifierStatic(adapter, qualID, val);
    }

    public override string ToString()
    {
        return "AttrFactory{" + "container=" + container + ", adapter=" + adapter + ", provider=" + provider + '}';
    }
}

public sealed class AbsoluteAttribute : Qualifier,
    NamespacedAttribute, Coster
{
    internal readonly TypeAdapter adapter;
    internal readonly AttributeFactory fac;
    internal readonly long id;

    /// <summary>
    ///     namespaceNames represent the names the variable could have based on declared container
    ///     (package) of the expression.
    /// </summary>
    internal readonly string[] namespaceNames;

    internal readonly TypeProvider provider;

    internal readonly IList<Qualifier> qualifiers;

    internal AbsoluteAttribute(long id, string[] namespaceNames,
        IList<Qualifier> qualifiers, TypeAdapter adapter, TypeProvider provider,
        AttributeFactory fac)
    {
        this.id = id;
        this.namespaceNames = namespaceNames;
        this.qualifiers = qualifiers;
        this.adapter = adapter;
        this.provider = provider;
        this.fac = fac;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        var min = 0L;
        var max = 0L;
        foreach (var q in qualifiers)
        {
            var qc = global::Cel.Interpreter.Cost.EstimateCost(q);
            min += qc.min;
            max += qc.max;
        }

        min++; // For object retrieval.
        max++;
        return Coster.CostOf(min, max);
    }

    /// <summary>
    ///     AddQualifier implements the Attribute interface method.
    /// </summary>
    public Attribute AddQualifier(Qualifier q)
    {
        qualifiers.Add(q);
        return this;
    }

    /// <summary>
    ///     CandidateVariableNames implements the NamespaceAttribute interface method.
    /// </summary>
    public string[] CandidateVariableNames()
    {
        return namespaceNames;
    }

    /// <summary>
    ///     Qualifiers returns the list of Qualifier instances associated with the namespaced attribute.
    /// </summary>
    public IList<Qualifier> Qualifiers()
    {
        return qualifiers;
    }

    /// <summary>
    ///     Resolve returns the resolved Attribute value given the Activation, or error if the Attribute
    ///     variable is not found, or if its Qualifiers cannot be applied successfully.
    /// </summary>
    public object Resolve(Activation vars)
    {
        var obj = TryResolve(vars);
        if (obj == null) throw Err.NoSuchAttributeException(this);

        return obj;
    }

    /// <summary>
    ///     TryResolve iterates through the namespaced variable names until one is found within the
    ///     Activation or TypeProvider.
    ///     <para>
    ///         If the variable name cannot be found as an Activation variable or in the TypeProvider as a
    ///         type, then the result is `nil`, `false`, `nil` per the interface requirement.
    ///     </para>
    /// </summary>
    public object TryResolve(Activation vars)
    {
        foreach (var nm in namespaceNames)
        {
            // If the variable is found, process it. Otherwise, wait until the checks to
            // determine whether the type is unknown before returning.
            var op = vars.ResolveName(nm);
            if (op != null)
            {
                foreach (var qual in qualifiers)
                {
                    var op2 = qual.Qualify(vars, op);
                    if (op2 is Err) return op2;

                    if (op2 == null) break;

                    op = op2;
                }

                return op;
            }

            // Attempt to resolve the qualified type name if the name is not a variable identifier.
            var typ = provider.FindIdent(nm);
            if (typ != null)
            {
                if (qualifiers.Count == 0) return typ;

                throw Err.NoSuchAttributeException(this);
            }
        }

        return null;
    }

    /// <summary>
    ///     ID implements the Attribute interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify is an implementation of the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var val = Resolve(vars);
        if (UnknownT.IsUnknown(val)) return val;

        var qual = fac.NewQualifier(null, id, val);
        return qual.Qualify(vars, obj);
    }

    /// <summary>
    ///     String implements the Stringer interface method.
    /// </summary>
    public override string ToString()
    {
        return "id: " + id + ", names: " + "[" + string.Join(", ", namespaceNames) + "]";
    }
}

public sealed class ConditionalAttribute : Qualifier, Attribute,
    Coster
{
    internal readonly TypeAdapter adapter;
    internal readonly Interpretable expr;
    internal readonly AttributeFactory fac;
    internal readonly Attribute falsy;
    internal readonly long id;
    internal readonly Attribute truthy;

    internal ConditionalAttribute(long id, Interpretable expr, Attribute truthy,
        Attribute falsy, TypeAdapter adapter, AttributeFactory fac)
    {
        this.id = id;
        this.expr = expr;
        this.truthy = truthy;
        this.falsy = falsy;
        this.adapter = adapter;
        this.fac = fac;
    }

    /// <summary>
    ///     AddQualifier appends the same qualifier to both sides of the conditional, in effect managing
    ///     the qualification of alternate attributes.
    /// </summary>
    public Attribute AddQualifier(Qualifier qual)
    {
        truthy.AddQualifier(qual); // just do
        falsy.AddQualifier(qual); // just do
        return this;
    }

    /// <summary>
    ///     Resolve evaluates the condition, and then resolves the truthy or falsy branch accordingly.
    /// </summary>
    public object Resolve(Activation vars)
    {
        var val = expr.Eval(vars);
        if (val == null) throw Err.NoSuchAttributeException(this);

        if (Err.IsError(val)) return null;

        if (val == BoolT.True) return truthy.Resolve(vars);

        if (val == BoolT.False) return falsy.Resolve(vars);

        if (UnknownT.IsUnknown(val)) return val;

        return Err.MaybeNoSuchOverloadErr(val);
    }

    /// <summary>
    ///     Cost provides the heuristic cost of a ternary operation {@code &lt;expr&gt; ? &lt;t&gt; :
    ///     &lt;f&gt;}. The cost is computed as {@code cost(expr)} plus the min/max costs of evaluating
    ///     either `t` or `f`.
    /// </summary>
    public Cost Cost()
    {
        var t = global::Cel.Interpreter.Cost.EstimateCost(truthy);
        var f = global::Cel.Interpreter.Cost.EstimateCost(falsy);
        var e = global::Cel.Interpreter.Cost.EstimateCost(expr);
        return Coster.CostOf(e.min + Math.Min(t.min, f.min), e.max + Math.Max(t.max, f.max));
    }

    /// <summary>
    ///     ID is an implementation of the Attribute interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify is an implementation of the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var val = Resolve(vars);
        if (UnknownT.IsUnknown(val)) return val;

        var qual = fac.NewQualifier(null, id, val);
        return qual.Qualify(vars, obj);
    }

    /// <summary>
    ///     String is an implementation of the Stringer interface method.
    /// </summary>
    public override string ToString()
    {
        return string.Format("id: {0:D}, truthy attribute: {1}, falsy attribute: {2}", id, truthy, falsy);
    }
}

public sealed class MaybeAttribute : Coster, Attribute, Qualifier
{
    internal readonly TypeAdapter adapter;
    internal readonly IList<NamespacedAttribute> attrs;
    internal readonly AttributeFactory fac;
    internal readonly long id;
    internal readonly TypeProvider provider;

    internal MaybeAttribute(long id, IList<NamespacedAttribute> attrs,
        TypeAdapter adapter, TypeProvider provider, AttributeFactory fac)
    {
        this.id = id;
        this.attrs = attrs;
        this.adapter = adapter;
        this.provider = provider;
        this.fac = fac;
    }

    /// <summary>
    ///     ID is an implementation of the Attribute interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     AddQualifier adds a qualifier to each possible attribute variant, and also creates a new
    ///     namespaced variable from the qualified value.
    ///     <para>
    ///         The algorithm for building the maybe attribute is as follows:
    ///         <ol>
    ///             <li>
    ///                 Create a maybe attribute from a simple identifier when it occurs in a parsed-only
    ///                 expression
    ///                 <br>
    ///                     <br>
    ///                         {@code mb = MaybeAttribute(&lt;id&gt;, "a")}
    ///                         <br>
    ///                             <br>
    ///                                 Initializing the maybe attribute creates an absolute attribute internally which
    ///                                 includes the possible namespaced names of the attribute. In this example, let's assume
    ///                                 we are in namespace 'ns', then the maybe is either one of the following variable names:
    ///                                 <br>
    ///                                     <br>
    ///                                         possible variables names -- ns.a, a
    ///                                         <li>
    ///                                             Adding a qualifier to the maybe means that the variable name could be a
    ///                                             longer
    ///                                             qualified name, or a field selection on one of the possible variable names
    ///                                             produced
    ///                                             earlier:
    ///                                             <br>
    ///                                                 <br>
    ///                                                     {@code mb.AddQualifier("b")}
    ///                                                     <br>
    ///                                                         <br>
    ///                                                             possible variables names -- ns.a.b, a.b
    ///                                                             <br>
    ///                                                                 possible field selection -- ns.a['b'], a['b']
    ///         </ol>
    ///         If none of the attributes within the maybe resolves a value, the result is an error.
    ///     </para>
    /// </summary>
    public Attribute AddQualifier(Qualifier qual)
    {
        var str = "";
        var isStr = false;
        if (qual is ConstantQualifier)
        {
            var cq = (ConstantQualifier)qual;
            var cqv = cq.Value().Value();
            if (cqv is string)
            {
                str = (string)cqv;
                isStr = true;
            }
        }

        var augmentedNames = new string[0];
        // First add the qualifier to all existing attributes in the oneof.
        foreach (var attr in attrs)
        {
            if (isStr && attr.Qualifiers().Count == 0)
            {
                var candidateVars = attr.CandidateVariableNames();
                augmentedNames = new string[candidateVars.Length];
                for (var i = 0; i < candidateVars.Length; i++)
                {
                    var name = candidateVars[i];
                    augmentedNames[i] = string.Format("{0}.{1}", name, str);
                }
            }

            attr.AddQualifier(qual);
        }

        // Next, ensure the most specific variable / type reference is searched first.
        if (attrs.Count == 0)
            attrs.Add(fac.AbsoluteAttribute(qual.Id(), augmentedNames));
        else
            attrs.Insert(0, fac.AbsoluteAttribute(qual.Id(), augmentedNames));

        return this;
    }

    /// <summary>
    ///     Qualify is an implementation of the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var val = Resolve(vars);
        if (UnknownT.IsUnknown(val)) return val;

        var qual = fac.NewQualifier(null, id, val);
        return qual.Qualify(vars, obj);
    }

    /// <summary>
    ///     Resolve follows the variable resolution rules to determine whether the attribute is a
    ///     variable or a field selection.
    /// </summary>
    public object Resolve(Activation vars)
    {
        foreach (var attr in attrs)
        {
            var obj = attr.TryResolve(vars);
            // If the object was found, return it.
            if (obj != null) return obj;
        }

        // Else, produce a no such attribute error.
        throw Err.NoSuchAttributeException(this);
    }

    /// <summary>
    ///     Cost implements the Coster interface method. The min cost is computed as the minimal cost
    ///     among all the possible attributes, the max cost ditto.
    /// </summary>
    public Cost Cost()
    {
        var min = long.MaxValue;
        var max = 0L;
        foreach (var a in attrs)
        {
            var ac = global::Cel.Interpreter.Cost.EstimateCost(a);
            min = Math.Min(min, ac.min);
            max = Math.Max(max, ac.max);
        }

        return Coster.CostOf(min, max);
    }

    /// <summary>
    ///     String is an implementation of the Stringer interface method.
    /// </summary>
    public override string ToString()
    {
        return string.Format("id: {0}, attributes: {1}", id, attrs);
    }
}

public sealed class RelativeAttribute : Coster, Qualifier,
    Attribute
{
    internal readonly TypeAdapter adapter;
    internal readonly AttributeFactory fac;
    internal readonly long id;
    internal readonly Interpretable operand;
    internal readonly IList<Qualifier> qualifiers;

    internal RelativeAttribute(long id, Interpretable operand,
        IList<Qualifier> qualifiers, TypeAdapter adapter, AttributeFactory fac)
    {
        this.id = id;
        this.operand = operand;
        this.qualifiers = qualifiers;
        this.adapter = adapter;
        this.fac = fac;
    }

    /// <summary>
    ///     AddQualifier implements the Attribute interface method.
    /// </summary>
    public Attribute AddQualifier(Qualifier qual)
    {
        qualifiers.Add(qual);
        return this;
    }

    /// <summary>
    ///     Resolve expression value and qualifier relative to the expression result.
    /// </summary>
    public object Resolve(Activation vars)
    {
        // First, evaluate the operand.
        var v = operand.Eval(vars);
        if (Err.IsError(v)) return null;

        if (UnknownT.IsUnknown(v)) return v;

        // Next, qualify it. Qualification handles unknowns as well, so there's no need to recheck.
        object obj = v;
        foreach (var qual in qualifiers)
        {
            if (obj == null) throw Err.NoSuchAttributeException(this);

            obj = qual.Qualify(vars, obj);
            if (obj is Err) return obj;
        }

        if (obj == null) throw Err.NoSuchAttributeException(this);

        return obj;
    }

    /// <summary>
    ///     Cost implements the Coster interface method.
    /// </summary>
    public Cost Cost()
    {
        var c = global::Cel.Interpreter.Cost.EstimateCost(operand);
        var min = c.min;
        var max = c.max;
        foreach (var qual in qualifiers)
        {
            var q = global::Cel.Interpreter.Cost.EstimateCost(qual);
            min += q.min;
            max += q.max;
        }

        return Coster.CostOf(min, max);
    }

    /// <summary>
    ///     ID is an implementation of the Attribute interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify is an implementation of the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var val = Resolve(vars);
        if (UnknownT.IsUnknown(val)) return val;

        var qual = fac.NewQualifier(null, id, val);
        return qual.Qualify(vars, obj);
    }

    /// <summary>
    ///     String is an implementation of the Stringer interface method.
    /// </summary>
    public override string ToString()
    {
        return string.Format("id: {0:D}, operand: {1}", id, operand);
    }
}

public sealed class AttrQualifier : Coster, Attribute
{
    internal readonly Attribute attribute;
    internal readonly long id;

    internal AttrQualifier(long id, Attribute attribute)
    {
        this.id = id;
        this.attribute = attribute;
    }

    public long Id()
    {
        return id;
    }

    public Attribute AddQualifier(Qualifier q)
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

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.EstimateCost(attribute);
    }

    public override string ToString()
    {
        return "AttrQualifier{" + "id=" + id + ", attribute=" + attribute + '}';
    }
}

public sealed class StringQualifier : Coster, ConstantQualifierEquator,
    AttributePattern.QualifierValueEquator
{
    internal readonly TypeAdapter adapter;
    internal readonly Val celValue;
    internal readonly long id;
    internal readonly string value;

    internal StringQualifier(long id, string value, Val celValue, TypeAdapter adapter)
    {
        this.id = id;
        this.value = value;
        this.celValue = celValue;
        this.adapter = adapter;
    }

    /// <summary>
    ///     ID is an implementation of the Qualifier interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify implements the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var s = value;
        if (obj is IDictionary)
        {
            var m = (IDictionary)obj;
            if (m.Contains(s))
            {
                obj = m[s];
                if (obj == null) return NullT.NullValue;
            }
            else
            {
                throw Err.NoSuchKeyException(s);
            }
        }
        else if (UnknownT.IsUnknown(obj))
        {
            return obj;
        }
        else
        {
            return AttributeFactory.RefResolve(adapter, celValue, obj);
        }

        return obj;
    }

    /// <summary>
    ///     Value implements the ConstantQualifier interface
    /// </summary>
    public Val Value()
    {
        return celValue;
    }

    public bool QualifierValueEquals(object value)
    {
        if (value is string) return this.value.Equals(value);

        return false;
    }

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.None;
    }

    public override string ToString()
    {
        return "StringQualifier{" + "id=" + id + ", value='" + value + '\'' + ", celValue=" + celValue +
               ", adapter=" + adapter + '}';
    }
}

public sealed class IntQualifier : Coster, ConstantQualifierEquator
{
    internal readonly TypeAdapter adapter;
    internal readonly Val celValue;
    internal readonly long id;
    internal readonly long value;

    internal IntQualifier(long id, long value, Val celValue, TypeAdapter adapter)
    {
        this.id = id;
        this.value = value;
        this.celValue = celValue;
        this.adapter = adapter;
    }

    /// <summary>
    ///     ID is an implementation of the Qualifier interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify implements the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var i = value;
        if (obj is IDictionary)
        {
            var m = (IDictionary)obj;
            if (m.Contains(i))
            {
                obj = m[i];
                if (obj == null) throw Err.NoSuchKeyException(i);
            }
            else
            {
                if (m.Contains((int)i))
                {
                    obj = m[(int)i];
                    if (obj == null) throw Err.NoSuchKeyException(i);
                }
            }

            return obj;
        }

        if (obj.GetType().IsArray)
        {
            var array = (Array)obj;
            var l = array.Length;
            if (i < 0 || i >= l) throw Err.IndexOutOfBoundsException(i);

            obj = array.GetValue(i);
            return obj;
        }

        if (obj is IList)
        {
            var list = (IList)obj;
            var l = list.Count;
            if (i < 0 || i >= l) throw Err.IndexOutOfBoundsException(i);

            obj = list[(int)i];
            return obj;
        }

        if (UnknownT.IsUnknown(obj)) return obj;

        return AttributeFactory.RefResolve(adapter, celValue, obj);
    }

    /// <summary>
    ///     Value implements the ConstantQualifier interface
    /// </summary>
    public Val Value()
    {
        return celValue;
    }

    public bool QualifierValueEquals(object value)
    {
        if (value is ulong) return false;

        if (value is byte) return this.value == (byte)value;
        if (value is short) return this.value == (short)value;
        if (value is int) return this.value == (int)value;
        if (value is long) return this.value == (long)value;

        return false;
    }

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.None;
    }

    public override string ToString()
    {
        return "IntQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" +
               adapter + '}';
    }
}

public sealed class UintQualifier : Coster, ConstantQualifierEquator
{
    internal readonly TypeAdapter adapter;
    internal readonly Val celValue;
    internal readonly long id;
    internal readonly ulong value;

    internal UintQualifier(long id, ulong value, Val celValue, TypeAdapter adapter)
    {
        this.id = id;
        this.value = value;
        this.celValue = celValue;
        this.adapter = adapter;
    }

    /// <summary>
    ///     ID is an implementation of the Qualifier interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify implements the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var i = value;
        if (obj is IDictionary)
        {
            var m = (IDictionary)obj;
            if (m.Contains(i))
            {
                obj = m[i];
                if (obj == null) throw Err.NoSuchKeyException(i);
            }

            return obj;
        }

        if (obj.GetType().IsArray)
        {
            var array = (Array)obj;
            var l = array.Length;
            if (i < 0 || i >= (ulong)l) throw Err.IndexOutOfBoundsException(i);

            obj = array.GetValue((int)i);
            return obj;
        }

        if (UnknownT.IsUnknown(obj)) return obj;

        return AttributeFactory.RefResolve(adapter, celValue, obj);
    }

    /// <summary>
    ///     Value implements the ConstantQualifier interface
    /// </summary>
    public Val Value()
    {
        return celValue;
    }

    public bool QualifierValueEquals(object value)
    {
        if (value is ulong) return this.value == (ulong)value;

        return false;
    }

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.None;
    }

    public override string ToString()
    {
        return "UintQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" +
               adapter + '}';
    }
}

public sealed class BoolQualifier : Coster, ConstantQualifierEquator
{
    internal readonly TypeAdapter adapter;
    internal readonly Val celValue;
    internal readonly long id;
    internal readonly bool value;

    internal BoolQualifier(long id, bool value, Val celValue, TypeAdapter adapter)
    {
        this.id = id;
        this.value = value;
        this.celValue = celValue;
        this.adapter = adapter;
    }

    /// <summary>
    ///     ID is an implementation of the Qualifier interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify implements the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        var b = value;
        if (obj is IDictionary)
        {
            var m = (IDictionary)obj;
            if (m.Contains(b))
            {
                obj = m[b];
                if (obj == null) throw Err.NoSuchKeyException(b);
            }
        }
        else if (UnknownT.IsUnknown(obj))
        {
            return obj;
        }
        else
        {
            return AttributeFactory.RefResolve(adapter, celValue, obj);
        }

        return obj;
    }

    /// <summary>
    ///     Value implements the ConstantQualifier interface
    /// </summary>
    public Val Value()
    {
        return celValue;
    }

    public bool QualifierValueEquals(object value)
    {
        if (value is bool) return this.value == ((bool?)value).Value;

        return false;
    }

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.None;
    }

    public override string ToString()
    {
        return "BoolQualifier{" + "id=" + id + ", value=" + value + ", celValue=" + celValue + ", adapter=" +
               adapter + '}';
    }
}

public sealed class FieldQualifier : Coster, ConstantQualifierEquator
{
    internal readonly TypeAdapter adapter;
    internal readonly FieldType fieldType;
    internal readonly long id;
    internal readonly string name;

    internal FieldQualifier(long id, string name, FieldType fieldType, TypeAdapter adapter)
    {
        this.id = id;
        this.name = name;
        this.fieldType = fieldType;
        this.adapter = adapter;
    }

    /// <summary>
    ///     ID is an implementation of the Qualifier interface method.
    /// </summary>
    public long Id()
    {
        return id;
    }

    /// <summary>
    ///     Qualify implements the Qualifier interface method.
    /// </summary>
    public object Qualify(Activation vars, object obj)
    {
        if (obj is Val) obj = ((Val)obj).Value();

        return fieldType.getFrom(obj);
    }

    /// <summary>
    ///     Value implements the ConstantQualifier interface
    /// </summary>
    public Val Value()
    {
        return StringT.StringOf(name);
    }

    public bool QualifierValueEquals(object value)
    {
        if (value is string) return name.Equals(value);

        return false;
    }

    /// <summary>
    ///     Cost returns zero for constant field qualifiers
    /// </summary>
    public Cost Cost()
    {
        return global::Cel.Interpreter.Cost.None;
    }

    public override string ToString()
    {
        return "FieldQualifier{" + "id=" + id + ", name='" + name + '\'' + ", fieldType=" + fieldType +
               ", adapter=" + adapter + '}';
    }
}