using Cel.Checker;
using Cel.Common;
using Google.Api.Expr.V1Alpha1;
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
namespace Cel;

/// <summary>
///     Ast representing the checked or unchecked expression, its source, and related metadata such as
///     source position information.
/// </summary>
public sealed class Ast
{
    internal readonly IDictionary<long, Reference> refMap;
    internal readonly IDictionary<long, Type> typeMap;

    public Ast(Expr expr, SourceInfo info, Source source) : this(expr, info, source,
        new Dictionary<long, Reference>(), new Dictionary<long, Type>())
    {
    }

    public Ast(Expr expr, SourceInfo info, Source source, IDictionary<long, Reference> refMap,
        IDictionary<long, Type> typeMap)
    {
        this.Expr = expr;
        this.SourceInfo = info;
        this.Source = source;
        this.refMap = refMap;
        this.typeMap = typeMap;
    }

    /// <summary>
    ///     Expr returns the proto serializable instance of the parsed/checked expression.
    /// </summary>
    public Expr Expr { get; }

    /// <summary>
    ///     IsChecked returns whether the Ast value has been successfully type-checked.
    /// </summary>
    public bool Checked => typeMap != null && typeMap.Count > 0;

    public Source Source { get; }

    /// <summary>
    ///     SourceInfo returns character offset and newling position information about expression elements.
    /// </summary>
    public SourceInfo SourceInfo { get; }

    /// <summary>
    ///     ResultType returns the output type of the expression if the Ast has been type-checked, else
    ///     returns decls.Dyn as the parse step cannot infer the type.
    /// </summary>
    public Type ResultType
    {
        get
        {
            if (!Checked) return Decls.Dyn;

            typeMap.TryGetValue(Expr.Id, out Type t);
            return t;
        }
    }

    /// <summary>
    ///     Source returns a view of the input used to create the Ast. This source may be complete or
    ///     constructed from the SourceInfo.
    /// </summary>
    public override string ToString()
    {
        return Source.Content();
    }
}