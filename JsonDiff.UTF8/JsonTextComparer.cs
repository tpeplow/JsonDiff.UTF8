using System;
using System.Buffers;

namespace JsonDiff.UTF8
{
    public static class JsonTextComparer
    {
        public static bool IsEqual(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            var leftWithoutWhitespace = GetWithoutWhitespace(left);
            var rightWithoutWhitespace = GetWithoutWhitespace(right);

            var isEqual = MemoryExtensions.SequenceEqual(
                new ReadOnlySpan<char>(leftWithoutWhitespace.chars, 0, leftWithoutWhitespace.end),
                new ReadOnlySpan<char>(rightWithoutWhitespace.chars, 0, rightWithoutWhitespace.end));
            
            ArrayPool<char>.Shared.Return(leftWithoutWhitespace.chars, true);
            ArrayPool<char>.Shared.Return(rightWithoutWhitespace.chars, true);

            return isEqual;
        }

        static (char[] chars, int end) GetWithoutWhitespace(ReadOnlySpan<char> chars)
        {
            var newChars = ArrayPool<char>.Shared.Rent(chars.Length);

            var insideQuote = false;
            var y = 0;
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];

                if (!insideQuote)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        continue;
                    }
                }

                if (c == '"')
                {
                    if (i == 0 || chars[i - 1] != '\\')
                    {
                        insideQuote = !insideQuote;
                    }
                }

                newChars[y] = c;
                y++;
            }

            return (newChars, y);
        }
    }
}