﻿using Cel.Common.Types.Ref;

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
///     IActivation used to resolve identifiers by name and references by id.
///     <para>
///         An Activation is the primary mechanism by which a caller supplies input into a CEL program.
///     </para>
/// </summary>
public interface IActivation
{
    /// <summary>
    ///     ResolveName returns a value from the activation by qualified name, or false if the name could
    ///     not be found.
    /// </summary>
    object? ResolveName(string name);

    /// <summary>
    ///     Parent returns the parent of the current activation, may be nil. If non-nil, the parent will be
    ///     searched during resolve calls.
    /// </summary>
    IActivation? Parent();

    /// <summary>
    ///     EmptyActivation returns a variable free activation.
    /// </summary>
    

    /// <summary>
    ///     NewActivation returns an activation based on a map-based binding where the map keys are
    ///     expected to be qualified names used with ResolveName calls.
    ///     <para>
    ///         The input `bindings` may either be of type `Activation` or `map[string]interface{}`.
    ///     </para>
    ///     <para>
    ///         Lazy bindings may be supplied within the map-based input in either of the following forms: -
    ///         func() interface{} - func() ref.Val
    ///     </para>
    ///     <para>
    ///         The output of the lazy binding will overwrite the variable reference in the internal map.
    ///     </para>
    ///     <para>
    ///         Values which are not represented as ref.Val types on input may be adapted to a ref.Val using
    ///         the ref.TypeAdapter configured in the environment.
    ///     </para>
    /// </summary>
    

    /// <summary>
    ///     NewHierarchicalActivation takes two activations and produces a new one which prioritizes
    ///     resolution in the child first and parent(s) second.
    /// </summary>
    

    /// <summary>
    ///     NewPartialActivation returns an Activation which contains a list of AttributePattern values
    ///     representing field and index operations that should result in a 'types.Unknown' result.
    ///     <para>
    ///         The `bindings` value may be any value type supported by the interpreter.NewActivation call,
    ///         but is typically either an existing Activation or map[string]interface{}.
    ///     </para>
    /// </summary>
    
}

/// <summary>
///     mapActivation which implements Activation and maps of named values.
///     <para>
///         Named bindings may lazily supply values by providing a function which accepts no arguments
///         and produces an interface value.
///     </para>
/// </summary>
public sealed class MapActivation : IActivation
{
    private readonly IDictionary<string, object> bindings;

    internal MapActivation(IDictionary<string, object> bindings)
    {
        this.bindings = bindings;
    }

    /// <summary>
    ///     Parent implements the Activation interface method.
    /// </summary>
    public IActivation? Parent()
    {
        return null;
    }

    /// <summary>
    ///     ResolveName implements the Activation interface method.
    /// </summary>
    public object? ResolveName(string name)
    {
        bindings.TryGetValue(name, out var obj);
        if (obj == null) return null;

        if (obj is Func<object>)
        {
            obj = ((Func<object>)obj)();
            bindings[name] = obj;
        }

        return obj;
    }

    public override string ToString()
    {
        return "MapActivation{" + "bindings=" + bindings + '}';
    }
}

/// <summary>
///     functionActivation which implements Activation and a provider of named values.
/// </summary>
public sealed class FunctionActivation : IActivation
{
    private readonly Func<string, object?> provider;

    internal FunctionActivation(Func<string, object?> provider)
    {
        this.provider = provider;
    }

    /// <summary>
    ///     Parent implements the Activation interface method.
    /// </summary>
    public IActivation Parent()
    {
        return null;
    }

    /// <summary>
    ///     ResolveName implements the Activation interface method.
    /// </summary>
    public object? ResolveName(string name)
    {
        return provider.Invoke(name);
    }

    public override string ToString()
    {
        return "FunctionActivation{" + "provider=" + provider + '}';
    }
}

/// <summary>
///     hierarchicalActivation which implements Activation and contains a parent and child activation.
/// </summary>
public sealed class HierarchicalActivation : IActivation
{
    private readonly IActivation child;
    private readonly IActivation parent;

    internal HierarchicalActivation(IActivation parent, IActivation child)
    {
        this.parent = parent;
        this.child = child;
    }

    /// <summary>
    ///     Parent implements the Activation interface method.
    /// </summary>
    public IActivation Parent()
    {
        return parent;
    }

    /// <summary>
    ///     ResolveName implements the Activation interface method.
    /// </summary>
    public object? ResolveName(string name)
    {
        var @object = child.ResolveName(name);
        if (@object != null) return @object;

        return parent.ResolveName(name);
    }

    public override string ToString()
    {
        return "HierarchicalActivation{" + "parent=" + parent + ", child=" + child + '}';
    }
}

/// <summary>
///     IPartialActivation extends the Activation interface with a set of UnknownAttributePatterns.
/// </summary>
public interface IPartialActivation : IActivation
{
    /// <summary>
    ///     UnknownAttributePaths returns a set of AttributePattern values which match Attribute
    ///     expressions for data accesses whose values are not yet known.
    /// </summary>
    AttributePattern[] UnknownAttributePatterns();
}

/// <summary>
///     partActivation is the default implementations of the PartialActivation interface.
/// </summary>
public sealed class PartActivation : IPartialActivation
{
    private readonly IActivation @delegate;
    private readonly AttributePattern[] unknowns;

    internal PartActivation(IActivation @delegate, AttributePattern[] unknowns)
    {
        this.@delegate = @delegate;
        this.unknowns = unknowns;
    }

    public IActivation? Parent()
    {
        return @delegate.Parent();
    }

    public object? ResolveName(string name)
    {
        return @delegate.ResolveName(name);
    }

    /// <summary>
    ///     UnknownAttributePatterns implements the PartialActivation interface method.
    /// </summary>
    public AttributePattern[] UnknownAttributePatterns()
    {
        return unknowns;
    }

    public override string ToString()
    {
        return "PartActivation{" + "delegate=" + @delegate + ", unknowns=" + "[" +
               string.Concat(unknowns.Select(o => o.ToString())) + "]" + "}";
    }
}

/// <summary>
///     varActivation represents a single mutable variable binding.
///     <para>
///         This activation type should only be used within folds as the fold loop controls the object
///         life-cycle.
///     </para>
/// </summary>
public sealed class VarActivation : IActivation
{
    internal string name;
    internal IActivation parent;
    internal IVal val;

    internal VarActivation()
    {
    }

    /// <summary>
    ///     Parent implements the Activation interface method.
    /// </summary>
    public IActivation Parent()
    {
        return parent;
    }

    /// <summary>
    ///     ResolveName implements the Activation interface method.
    /// </summary>
    public object? ResolveName(string name)
    {
        if (name.Equals(this.name)) return val;

        return parent.ResolveName(name);
    }

    public override string ToString()
    {
        return "VarActivation{" + "parent=" + parent + ", name='" + name + '\'' + ", val=" + val + '}';
    }
}