using Cel.Common.Types.Ref;

namespace Cel.Interpreter;

public static class ActivationFactory
{
    public static IActivation EmptyActivation()
    {
        return NewActivation(new Dictionary<string, object>());
    }

    public static IActivation NewActivation(object bindings)
    {
        if (bindings == null) throw new NullReferenceException("bindings must be non-nil");

        if (bindings is IActivation) return (IActivation)bindings;

        if (bindings is IDictionary<string, object>)
            return new MapActivation((IDictionary<string, object>)bindings);

        if (bindings is Func<string, object?>) return new FunctionActivation((Func<string, object?>)bindings);

        throw new ArgumentException(string.Format(
            "activation input must be an activation or map[string]interface: got {0}", bindings.GetType().FullName));
    }

    public static IActivation NewHierarchicalActivation(IActivation parent, IActivation child)
    {
        return new HierarchicalActivation(parent, child);
    }

    public static IPartialActivation NewPartialActivation(object bindings, params AttributePattern[] unknowns)
    {
        var a = NewActivation(bindings);
        return new PartActivation(a, unknowns);
    }
}

