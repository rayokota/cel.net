using Cel.Common.Containers;
using Cel.Common.Types;
using Cel.Common.Types.Ref;
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
///     AttributePattern represents a top-level variable with an optional set of qualifier patterns.
///     <para>
///         When using a CEL expression within a container, e.g. a package or namespace, the variable name
///         in the pattern must match the qualified name produced during the variable namespace resolution.
///         For example, if variable `c` appears in an expression whose container is `a.b`, the variable name
///         supplied to the pattern must be `a.b.c`
///     </para>
///     <para>
///         The qualifier patterns for attribute matching must be one of the following:
///         <ul>
///             <li>
///                 valid map key type: string, int, uint, bool
///                 <li>wildcard (*)
///         </ul>
///     </para>
///     <para>
///         Examples:
///         <ol>
///             <li>
///                 ns.myvar["complex-value"]
///                 <li>
///                     ns.myvar["complex-value"][0]
///                     <li>ns.myvar["complex-value"].*.name
///         </ol>
///     </para>
///     <para>
///         The first example is simple: match an attribute where the variable is 'ns.myvar' with a field
///         access on 'complex-value'. The second example expands the match to indicate that only a specific
///         index `0` should match. And lastly, the third example matches any indexed access that later
///         selects the 'name' field.
///     </para>
/// </summary>
public sealed class AttributePattern
{
    private readonly IList<AttributeQualifierPattern> qualifierPatterns;
    private readonly string variable;

    internal AttributePattern(string variable, IList<AttributeQualifierPattern> qualifierPatterns)
    {
        this.variable = variable;
        this.qualifierPatterns = qualifierPatterns;
    }

    /// <summary>
    ///     NewAttributePattern produces a new mutable AttributePattern based on a variable name.
    /// </summary>
    public static AttributePattern NewAttributePattern(string variable)
    {
        return new AttributePattern(variable, new List<AttributeQualifierPattern>());
    }

    /// <summary>
    ///     QualString adds a string qualifier pattern to the AttributePattern. The string may be a valid
    ///     identifier, or string map key including empty string.
    /// </summary>
    public AttributePattern QualString(string pattern)
    {
        qualifierPatterns.Add(AttributeQualifierPattern.ForValue(pattern));
        return this;
    }

    /// <summary>
    ///     QualInt adds an int qualifier pattern to the AttributePattern. The index may be either a map or
    ///     list index.
    /// </summary>
    public AttributePattern QualInt(long pattern)
    {
        qualifierPatterns.Add(AttributeQualifierPattern.ForValue(pattern));
        return this;
    }

    /// <summary>
    ///     QualUint adds an uint qualifier pattern for a map index operation to the AttributePattern.
    /// </summary>
    public AttributePattern QualUint(ulong pattern)
    {
        qualifierPatterns.Add(AttributeQualifierPattern.ForValue(pattern));
        return this;
    }

    /// <summary>
    ///     QualBool adds a bool qualifier pattern for a map index operation to the AttributePattern.
    /// </summary>
    public AttributePattern QualBool(bool pattern)
    {
        qualifierPatterns.Add(AttributeQualifierPattern.ForValue(pattern));
        return this;
    }

    /// <summary>
    ///     Wildcard adds a special sentinel qualifier pattern that will match any single qualifier.
    /// </summary>
    public AttributePattern Wildcard()
    {
        qualifierPatterns.Add(AttributeQualifierPattern.Wildcard());
        return this;
    }

    /// <summary>
    ///     VariableMatches returns true if the fully qualified variable matches the AttributePattern fully
    ///     qualified variable name.
    /// </summary>
    public bool VariableMatches(string variable)
    {
        return this.variable.Equals(variable);
    }

    /// <summary>
    ///     QualifierPatterns returns the set of AttributeQualifierPattern values on the AttributePattern.
    /// </summary>
    public IList<AttributeQualifierPattern> QualifierPatterns()
    {
        return qualifierPatterns;
    }

    public override string ToString()
    {
        return "AttributePattern{" + "variable='" + variable + '\'' + ", qualifierPatterns=" +
               string.Join(",\n    ", qualifierPatterns.Select(o => o.ToString()));
    }

    /// <summary>
    ///     NewPartialAttributeFactory returns an AttributeFactory implementation capable of performing
    ///     AttributePattern matches with PartialActivation inputs.
    /// </summary>
    public static IAttributeFactory NewPartialAttributeFactory(Container container, TypeAdapter adapter,
        ITypeProvider provider)
    {
        var fac = IAttributeFactory.NewAttributeFactory(container, adapter, provider);
        return new PartialAttributeFactory(fac, container, adapter, provider);
    }

    /// <summary>
    ///     AttributeQualifierPattern holds a wilcard or valued qualifier pattern.
    /// </summary>
    public sealed class AttributeQualifierPattern
    {
        private readonly object? value;
        private readonly bool wildcard;

        internal AttributeQualifierPattern(bool wildcard, object? value)
        {
            this.wildcard = wildcard;
            this.value = value;
        }

        internal static AttributeQualifierPattern Wildcard()
        {
            return new AttributeQualifierPattern(true, null);
        }

        internal static AttributeQualifierPattern ForValue(object value)
        {
            return new AttributeQualifierPattern(false, value);
        }

        /// <summary>
        ///     Matches returns true if the qualifier pattern is a wildcard, or the Qualifier implements the
        ///     qualifierValueEquator interface and its IsValueEqualTo returns true for the qualifier
        ///     pattern.
        /// </summary>
        public bool Matches(IQualifier q)
        {
            if (wildcard) return true;

            if (q is IQualifierValueEquator)
            {
                var qve = (IQualifierValueEquator)q;
                return qve.QualifierValueEquals(value);
            }

            return false;
        }

        public override string ToString()
        {
            return "AttributeQualifierPattern{" + "wildcard=" + wildcard + ", value=" + value + '}';
        }
    }

    /// <summary>
    ///     qualifierValueEquator defines an interface for determining if an input value, of valid map key
    ///     type, is equal to the value held in the Qualifier. This interface is used by the
    ///     AttributeQualifierPattern to determine pattern matches for non-wildcard qualifier patterns.
    ///     <para>
    ///         Note: Attribute values are also Qualifier values; however, Attriutes are resolved before
    ///         qualification happens. This is an implementation detail, but one relevant to why the Attribute
    ///         types do not surface in the list of implementations.
    ///     </para>
    ///     <para>
    ///         See: partialAttributeFactory.matchesUnknownPatterns for more details on how this interface
    ///         is used.
    ///     </para>
    /// </summary>
    public interface IQualifierValueEquator
    {
        /// <summary>
        ///     QualifierValueEquals returns true if the input value is equal to the value held in the
        ///     Qualifier.
        /// </summary>
        bool QualifierValueEquals(object? value);
    }

    internal sealed class PartialAttributeFactory : IAttributeFactory
    {
        private readonly TypeAdapter adapter;
        private readonly Container container;
        private readonly IAttributeFactory fac;
        private readonly ITypeProvider provider;

        internal PartialAttributeFactory(IAttributeFactory fac, Container container, TypeAdapter adapter,
            ITypeProvider provider)
        {
            this.fac = fac;
            this.container = container;
            this.adapter = adapter;
            this.provider = provider;
        }

        public IAttribute ConditionalAttribute(long id, IInterpretable expr,
            IAttribute t, IAttribute f)
        {
            return fac.ConditionalAttribute(id, expr, t, f);
        }

        public IAttribute RelativeAttribute(long id, IInterpretable operand)
        {
            return fac.RelativeAttribute(id, operand);
        }

        public IQualifier NewQualifier(Type? objType, long qualId, object val)
        {
            return fac.NewQualifier(objType, qualId, val);
        }

        /// <summary>
        ///     AbsoluteAttribute implementation of the AttributeFactory interface which wraps the
        ///     NamespacedAttribute resolution in an internal attributeMatcher object to dynamically match
        ///     unknown patterns from PartialActivation inputs if given.
        /// </summary>
        public INamespacedAttribute AbsoluteAttribute(long id, params string[] names)
        {
            var attr = fac.AbsoluteAttribute(id, names);
            return new AttributeMatcher(this, attr, new List<IQualifier>());
        }

        /// <summary>
        ///     MaybeAttribute implementation of the AttributeFactory interface which ensure that the set of
        ///     'maybe' NamespacedAttribute values are produced using the PartialAttributeFactory rather than
        ///     the base AttributeFactory implementation.
        /// </summary>
        public IAttribute MaybeAttribute(long id, string name)
        {
            IList<INamespacedAttribute> attrs = new List<INamespacedAttribute>();
            attrs.Add(AbsoluteAttribute(id, container.ResolveCandidateNames(name)));
            return new MaybeAttribute(id, attrs, adapter, provider, this);
        }

        /// <summary>
        ///     matchesUnknownPatterns returns true if the variable names and qualifiers for a given
        ///     Attribute value match any of the ActivationPattern objects in the set of unknown activation
        ///     patterns on the given PartialActivation.
        ///     <para>
        ///         For example, in the expression `a.b`, the Attribute is composed of variable `a`, with
        ///         string qualifier `b`. When a PartialActivation is supplied, it indicates that some or all of
        ///         the data provided in the input is unknown by specifying unknown AttributePatterns. An
        ///         AttributePattern that refers to variable `a` with a string qualifier of `c` will not match
        ///         `a.b`; however, any of the following patterns will match Attribute `a.b`:
        ///         <ul>
        ///             <li>
        ///                 `AttributePattern("a")`
        ///                 <li>
        ///                     `AttributePattern("a").Wildcard()`
        ///                     <li>
        ///                         `AttributePattern("a").QualString("b")`
        ///                         <li>`AttributePattern("a").QualString("b").QualInt(0)`
        ///         </ul>
        ///     </para>
        ///     <para>
        ///         Any AttributePattern which overlaps an Attribute or vice-versa will produce an Unknown
        ///         result for the last pattern matched variable or qualifier in the Attribute. In the first
        ///         matching example, the expression id representing variable `a` would be listed in the Unknown
        ///         result, whereas in the other pattern examples, the qualifier `b` would be returned as the
        ///         Unknown.
        ///     </para>
        /// </summary>
        internal object? MatchesUnknownPatterns(IPartialActivation vars, long attrId,
            string[] variableNames, IList<IQualifier> qualifiers)
        {
            var patterns = vars.UnknownAttributePatterns();
            ISet<int> candidateIndices = new HashSet<int>();
            foreach (var variable in variableNames)
                for (var i = 0; i < patterns.Length; i++)
                {
                    var pat = patterns[i];
                    if (pat.VariableMatches(variable)) candidateIndices.Add(i);
                }

            // Determine whether to return early if there are no candidate unknown patterns.
            if (candidateIndices.Count == 0) return null;

            // Determine whether to return early if there are no qualifiers.
            if (qualifiers.Count == 0) return UnknownT.UnknownOf(attrId);

            // Resolve the attribute qualifiers into a static set. This prevents more dynamic
            // Attribute resolutions than necessary when there are multiple unknown patterns
            // that traverse the same Attribute-based qualifier field.
            var newQuals = new IQualifier[qualifiers.Count];
            for (var i = 0; i < qualifiers.Count; i++)
            {
                var qual = qualifiers[i];
                if (qual is IAttribute)
                {
                    var val = ((IAttribute)qual).Resolve(vars);
                    if (UnknownT.IsUnknown(val)) return val;

                    // If this resolution behavior ever changes, new implementations of the
                    // qualifierValueEquator may be required to handle proper resolution.
                    qual = fac.NewQualifier(null, qual.Id(), val);
                }

                newQuals[i] = qual;
            }

            // Determine whether any of the unknown patterns match.
            foreach (var patIdx in candidateIndices)
            {
                var pat = patterns[patIdx];
                var isUnk = true;
                var matchExprId = attrId;
                var qualPats = pat.QualifierPatterns();
                for (var i = 0; i < newQuals.Length; i++)
                {
                    var qual = newQuals[i];
                    if (i >= qualPats.Count) break;

                    matchExprId = qual.Id();
                    var qualPat = qualPats[i];
                    // Note, the AttributeQualifierPattern relies on the input Qualifier not being an
                    // Attribute, since there is no way to resolve the Attribute with the information
                    // provided to the Matches call.
                    if (!qualPat.Matches(qual))
                    {
                        isUnk = false;
                        break;
                    }
                }

                if (isUnk) return UnknownT.UnknownOf(matchExprId);
            }

            return null;
        }
    }

    /// <summary>
    ///     attributeMatcher embeds the NamespacedAttribute interface which allows it to participate in
    ///     AttributePattern matching against Attribute values without having to modify the code paths that
    ///     identify Attributes in expressions.
    /// </summary>
    internal sealed class AttributeMatcher : INamespacedAttribute
    {
        private readonly INamespacedAttribute attr;
        private readonly PartialAttributeFactory fac;
        private readonly IList<IQualifier> qualifiers;

        internal AttributeMatcher(PartialAttributeFactory fac, INamespacedAttribute attr,
            IList<IQualifier> qualifiers)
        {
            this.fac = fac;
            this.attr = attr;
            this.qualifiers = qualifiers;
        }

        public long Id()
        {
            return attr.Id();
        }

        public string[] CandidateVariableNames()
        {
            return attr.CandidateVariableNames();
        }

        public IList<IQualifier> Qualifiers()
        {
            return attr.Qualifiers();
        }

        /// <summary>
        ///     AddQualifier implements the Attribute interface method.
        /// </summary>
        public IAttribute AddQualifier(IQualifier qual)
        {
            // Add the qualifier to the embedded NamespacedAttribute. If the input to the Resolve
            // method is not a PartialActivation, or does not match an unknown attribute pattern, the
            // Resolve method is directly invoked on the underlying NamespacedAttribute.
            attr.AddQualifier(qual);
            // The attributeMatcher overloads TryResolve and will attempt to match unknown patterns
            // against
            // the variable name and qualifier set contained within the Attribute. These values are not
            // directly inspectable on the top-level NamespacedAttribute interface and so are tracked
            // within
            // the attributeMatcher.
            qualifiers.Add(qual);
            return this;
        }

        /// <summary>
        ///     Resolve is an implementation of the Attribute interface method which uses the
        ///     attributeMatcher TryResolve implementation rather than the embedded NamespacedAttribute
        ///     Resolve implementation.
        /// </summary>
        public object? Resolve(IActivation vars)
        {
            var obj = TryResolve(vars);
            if (obj == null) throw Err.NoSuchAttributeException(this);

            return obj;
        }

        /// <summary>
        ///     TryResolve is an implementation of the NamespacedAttribute interface method which tests for
        ///     matching unknown attribute patterns and returns types.Unknown if present. Otherwise, the
        ///     standard Resolve logic applies.
        /// </summary>
        public object? TryResolve(IActivation vars)
        {
            var id = attr.Id();
            if (vars is IPartialActivation)
            {
                var partial = (IPartialActivation)vars;
                var unk = fac.MatchesUnknownPatterns(partial, id, CandidateVariableNames(), qualifiers);
                if (unk != null) return unk;
            }

            return attr.TryResolve(vars);
        }

        /// <summary>
        ///     Qualify is an implementation of the Qualifier interface method.
        /// </summary>
        public object? Qualify(IActivation vars, object obj)
        {
            var val = Resolve(vars);
            if (UnknownT.IsUnknown(val)) return val;

            var qual = fac.NewQualifier(null, Id(), val);
            return qual.Qualify(vars, obj);
        }

        public override string ToString()
        {
            return "AttributeMatcher{" + "attr=" + attr + ", fac=" + fac + ", qualifiers=" +
                   string.Join(",\n    ", qualifiers.Select(o => o.ToString()));
        }
    }
}