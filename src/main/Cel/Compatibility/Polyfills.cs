#if NET462
namespace System
{
    internal static class HashCode
    {
        public static int Combine(object? value)
        {
            return value?.GetHashCode() ?? 0;
        }

        public static int Combine(object? value1, object? value2)
        {
            unchecked
            {
                var h1 = value1?.GetHashCode() ?? 0;
                var h2 = value2?.GetHashCode() ?? 0;
                return (h1 * 397) ^ h2;
            }
        }

        public static int Combine(object? value1, object? value2, object? value3)
        {
            return Combine(Combine(value1, value2), value3);
        }

        public static int Combine(object? value1, object? value2, object? value3, object? value4)
        {
            return Combine(Combine(value1, value2), Combine(value3, value4));
        }
    }
}
#endif

#if NET462 || NETSTANDARD2_1
internal static class ReferenceEqualityComparer
{
    public static System.Collections.Generic.IEqualityComparer<System.Type> Instance { get; } =
        new TypeRefEqualityComparer();

    private sealed class TypeRefEqualityComparer : System.Collections.Generic.IEqualityComparer<System.Type>
    {
        public bool Equals(System.Type? x, System.Type? y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(System.Type obj)
        {
            return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
#endif


