using Google.Api.Expr.V1Alpha1;

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
namespace Cel.Common.Containers;

/// <summary>
///     Container holds a reference to an optional qualified container name and set of aliases.
///     <para>
///         The program container can be used to simplify variable, function, and type specification
///         within CEL programs and behaves more or less like a C++ namespace. See ResolveCandidateNames for
///         more details.
///     </para>
/// </summary>
public sealed class Container
{
    /// <summary>
    ///     ContainerOption specifies a functional configuration option for a Container.
    ///     <para>
    ///         Note, ContainerOption implementations must be able to handle nil container inputs.
    ///     </para>
    /// </summary>
    public delegate Container? ContainerOption(Container? c);

    /// <summary>
    ///     DefaultContainer has an empty container name.
    /// </summary>
    public static readonly Container DefaultContainer = new("", new Dictionary<string, string>());

    private readonly IDictionary<string, string> aliases;

    private readonly string name;

    private Container(string name, IDictionary<string, string> aliases)
    {
        this.name = name;
        this.aliases = aliases;
    }

    /// <summary>
    ///     NewContainer creates a new Container with the fully-qualified name.
    /// </summary>
    public static Container? NewContainer(params ContainerOption[] opts)
    {
        var c = DefaultContainer;
        foreach (var opt in opts)
        {
            c = opt(c);
            if (c == null) return null;
        }

        return c;
    }

    /// <summary>
    ///     Name returns the fully-qualified name of the container.
    ///     <para>
    ///         The name may conceptually be a namespace, package, or type.
    ///     </para>
    /// </summary>
    public string Name()
    {
        return name;
    }

    public override string ToString()
    {
        return Name();
    }

    /// <summary>
    ///     Extend creates a new Container with the existing settings and applies a series of
    ///     ContainerOptions to further configure the new container.
    /// </summary>
    public Container? Extend(params ContainerOption[] opts)
    {
        // Copy the name and aliases of the existing container.
        IDictionary<string, string> aliasSet = new Dictionary<string, string>(AliasSet());
        var ext = new Container(Name(), aliasSet);

        // Apply the new options to the container.
        foreach (var opt in opts)
        {
            ext = opt(ext);
            if (ext == null) return null;
        }

        return ext;
    }

    /// <summary>
    ///     ResolveCandidateNames returns the candidates name of namespaced identifiers in C++ resolution
    ///     order.
    ///     <para>
    ///         Names which shadow other names are returned first. If a name includes a leading dot ('.'),
    ///         the name is treated as an absolute identifier which cannot be shadowed.
    ///     </para>
    ///     <para>
    ///         Given a container name a.b.c.M.N and a type name R.s, this will deliver in order:
    ///     </para>
    ///     <para>
    ///         {@code a.b.c.M.N.R.s}
    ///         <br>
    ///             {@code a.b.c.M.R.s}
    ///             <br>
    ///                 {@code a.b.c.R.s}
    ///                 <br>
    ///                     {@code a.b.R.s}
    ///                     <br>
    ///                         {@code a.R.s}
    ///                         <br>
    ///                             {@code R.s}<br>
    ///     </para>
    ///     <para>
    ///         If aliases or abbreviations are configured for the container, then alias names will take
    ///         precedence over containerized names.
    ///     </para>
    /// </summary>
    public string[] ResolveCandidateNames(string name)
    {
        string? alias;
        if (name.StartsWith(".", StringComparison.Ordinal))
        {
            var qn = name.Substring(1);
            alias = FindAlias(qn);
            if (alias != null) return new[] { alias };

            return new[] { qn };
        }

        alias = FindAlias(name);
        if (alias != null) return new[] { alias };

        if (Name() == null || Name().Length == 0) return new[] { name };

        var nextCont = Name();
        IList<string> candidates = new List<string>();
        candidates.Add(nextCont + "." + name);
        for (var i = nextCont.LastIndexOf('.'); i >= 0; i = nextCont.LastIndexOf('.', i - 1))
        {
            nextCont = nextCont.Substring(0, i);
            candidates.Add(nextCont + "." + name);
        }

        candidates.Add(name);
        return ((List<string>)candidates).ToArray();
    }

    /// <summary>
    ///     findAlias takes a name as input and returns an alias expansion if one exists.
    ///     <para>
    ///         If the name is qualified, the first component of the qualified name is checked against known
    ///         aliases. Any alias that is found in a qualified name is expanded in the result:
    ///     </para>
    ///     <para>
    ///         {@code alias: R -> my.alias.R}</br> {@code name: R.S.T}</br> {@code output:
    ///         my.alias.R.S.T}</br>
    ///     </para>
    ///     <para>
    ///         Note, the name must not have a leading dot.
    ///     </para>
    /// </summary>
    internal string? FindAlias(string name)
    {
        // If an alias exists for the name, ensure it is searched last.
        var simple = name;
        var qualifier = "";
        var dot = name.IndexOf('.');
        if (dot >= 0)
        {
            simple = name.Substring(0, dot);
            qualifier = name.Substring(dot);
        }

        AliasSet().TryGetValue(simple, out var alias);
        if (alias == null) return null;

        return alias + qualifier;
    }

    /// <summary>
    ///     ToQualifiedName converts an expression AST into a qualified name if possible, with a boolean +
    ///     'found' value that indicates if the conversion is successful.
    /// </summary>
    public static string? ToQualifiedName(Expr e)
    {
        switch (e.ExprKindCase)
        {
            case Expr.ExprKindOneofCase.IdentExpr:
                return e.IdentExpr.Name;
            case Expr.ExprKindOneofCase.SelectExpr:
                var sel = e.SelectExpr;
                if (sel.TestOnly) return null;

                var qual = ToQualifiedName(sel.Operand);
                if (qual != null) return qual + "." + sel.Field;

                break;
        }

        return null;
    }

    /// <summary>
    ///     aliasSet returns the alias to fully-qualified name mapping stored in the container.
    /// </summary>
    public IDictionary<string, string> AliasSet()
    {
        return aliases;
    }

    /// <summary>
    ///     Abbrevs configures a set of simple names as abbreviations for fully-qualified names. // // An
    ///     abbreviation (abbrev for short) is a simple name that expands to a fully-qualified name. //
    ///     Abbreviations can be useful when working with variables, functions, and especially types from
    ///     // multiple namespaces: // // // CEL object construction // qual.pkg.version.ObjTypeName{ //
    ///     field: alt.container.ver.FieldTypeName{value: ...} // } // // Only one the qualified names
    ///     above may be used as the CEL container, so at least one of these // references must be a long
    ///     qualified name within an otherwise short CEL program. Using the // following abbreviations, the
    ///     program becomes much simpler: // // // CEL Go option // Abbrevs("qual.pkg.version.ObjTypeName",
    ///     "alt.container.ver.FieldTypeName") // // Simplified Object construction // ObjTypeName{field:
    ///     FieldTypeName{value: ...}} // // There are a few rules for the qualified names and the simple
    ///     abbreviations generated from them: // - Qualified names must be dot-delimited, e.g.
    ///     `package.subpkg.name`. // - The last element in the qualified name is the abbreviation. // -
    ///     Abbreviations must not collide with each other. // - The abbreviation must not collide with
    ///     unqualified names in use. // // Abbreviations are distinct from container-based references in
    ///     the following important ways: // - Abbreviations must expand to a fully-qualified name. // -
    ///     Expanded abbreviations do not participate in namespace resolution. // - Abbreviation expansion
    ///     is done instead of the container search for a matching identifier. // - Containers follow C++
    ///     namespace resolution rules with searches from the most qualified name // to the least qualified
    ///     name. // - Container references within the CEL program may be relative, and are resolved to
    ///     fully // qualified names at either type-check time or program plan time, whichever comes first.
    ///     // // If there is ever a case where an identifier could be in both the container and as an //
    ///     abbreviation, the abbreviation wins as this will ensure that the meaning of a program is //
    ///     preserved between compilations even as the container evolves.
    /// </summary>
    public static ContainerOption Abbrevs(params string[] qualifiedNames)
    {
        return c =>
        {
            foreach (var qn in qualifiedNames)
            {
                var ind = qn.LastIndexOf('.');
                if (ind <= 0 || ind >= qn.Length - 1)
                    throw new ArgumentException(
                        string.Format("invalid qualified name: {0}, wanted name of the form 'qualified.name'", qn));

                var alias = qn.Substring(ind + 1);
                c = AliasAs("abbreviation", qn, alias)(c);
                if (c == null) return null;
            }

            return c;
        };
    }

    /// <summary>
    ///     Alias associates a fully-qualified name with a user-defined alias. // // In general, Abbrevs is
    ///     preferred to Alias since the names generated from the Abbrevs option // are more easily traced
    ///     back to source code. The Alias option is useful for propagating alias // configuration from one
    ///     Container instance to another, and may also be useful for remapping // poorly chosen protobuf
    ///     message / package names. // // Note: all of the rules that apply to Abbrevs also apply to
    ///     Alias.
    /// </summary>
    public static ContainerOption Alias(string qualifiedName, string alias)
    {
        return AliasAs("alias", qualifiedName, alias);
    }

    internal static ContainerOption AliasAs(string kind, string qualifiedName, string alias)
    {
        return c =>
        {
            if (alias.Length == 0 || alias.IndexOf('.') != -1)
                throw new ArgumentException(
                    string.Format("{0} must be non-empty and simple (not qualified): {1}={2}", kind, kind, alias));

            if (qualifiedName[0] == '.')
                throw new ArgumentException(
                    string.Format("qualified name must not begin with a leading '.': {0}", qualifiedName));

            var ind = qualifiedName.LastIndexOf('.');
            if (ind <= 0 || ind == qualifiedName.Length - 1)
                throw new ArgumentException(string.Format("{0} must refer to a valid qualified name: {1}",
                    kind, qualifiedName));

            c.AliasSet().TryGetValue(alias, out var aliasRef);
            if (aliasRef != null)
                throw new ArgumentException(string.Format(
                    "{0} collides with existing reference: name={1}, {2}={3}, existing={4}", kind, qualifiedName,
                    kind, alias, aliasRef));

            if (c.Name().StartsWith(alias + ".") || c.Name().Equals(alias))
                throw new ArgumentException(string.Format(
                    "{0} collides with container name: name={1}, {2}={3}, container={4}", kind, qualifiedName, kind,
                    alias, c.Name()));

            IDictionary<string, string> aliases = new Dictionary<string, string>(c.AliasSet());
            aliases[alias] = qualifiedName;
            c = new Container(c.name, aliases);
            return c;
        };
    }

    /// <summary>
    ///     Name sets the fully-qualified name of the Container.
    /// </summary>
    public static ContainerOption Name(string name)
    {
        return c =>
        {
            if (name.Length > 0 && name[0] == '.')
                throw new ArgumentException(
                    string.Format("container name must not contain a leading '.': {0}", name));

            if (c.name.Equals(name)) return c;

            c = new Container(name, c.aliases);
            return c;
        };
    }
}