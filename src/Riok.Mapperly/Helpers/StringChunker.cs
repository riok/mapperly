using System.Text;

namespace Riok.Mapperly.Helpers;

public static class StringChunker
{
    internal static IEnumerable<string> ChunkPascalCase(string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str)
        {
            if (!char.IsUpper(c))
            {
                sb.Append(c);
                continue;
            }

            if (sb.Length != 0)
            {
                yield return sb.ToString();
                sb.Clear();
            }

            sb.Append(c);
        }

        if (sb.Length != 0)
            yield return sb.ToString();
    }
}
