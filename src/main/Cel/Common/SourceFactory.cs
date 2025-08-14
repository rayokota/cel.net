using Google.Api.Expr.V1Alpha1;

namespace Cel.Common;

public static class SourceFactory
{
    public static ISource NewTextSource(string text)
    {
        return NewStringSource(text, "<input>");
    }

    public static ISource NewStringSource(string contents, string description)
    {
        IList<int> offsets = new List<int>();
        for (var i = 0; i <= contents.Length;)
        {
            if (i > 0) offsets.Add(i);
            var nl = contents.IndexOf('\n', i);
            if (nl == -1)
            {
                offsets.Add(contents.Length + 1);
                break;
            }
            i = nl + 1;
        }

        return new SourceImpl(contents, description, offsets);
    }

    public static ISource NewInfoSource(SourceInfo info)
    {
        return new SourceImpl("", info.Location, info.LineOffsets, info.Positions);
    }
}

