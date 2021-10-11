using System;
using System.Buffers;

namespace JsonDiff.UTF8
{
    public static class JsonTextComparer
    {
        public static bool IsEqual(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            bool isEqual;
            int leftEnd = 0;
            int rightEnd = 0;
            do
            {
                var leftWithoutWhitespace = ReadToNextWhitespace(left, ref leftEnd);
                var rightWithoutWhitespace = ReadToNextWhitespace(right, ref rightEnd);

                isEqual = leftWithoutWhitespace.SequenceEqual(rightWithoutWhitespace);
            } while (isEqual && leftEnd < left.Length && rightEnd < right.Length);

            return isEqual;
        }

        static ReadOnlySpan<char> ReadToNextWhitespace(ReadOnlySpan<char> chars, ref int end)
        {
            var insideQuote = false;
            var startAt = end;
            var length = 0;
            var foundNonWhitespaceChar = false;
            var foundWhitespace = false;
            for (var i = startAt; i < chars.Length; i++)
            {
                var c = chars[i];

                if (!insideQuote && char.IsWhiteSpace(c))
                {
                    foundWhitespace = true;
                    if (foundNonWhitespaceChar)
                    {
                        break;
                    }

                    startAt = i;
                }
                else if (foundWhitespace)
                {
                    break;
                }
                else
                {
                    foundNonWhitespaceChar = true;
                    if (c == '"')
                    {
                        if (i == 0 || chars[i - 1] != '\\')
                        {
                            insideQuote = !insideQuote;
                        }
                    }
                    length++;
                }

                end++;
            }
           
            return !foundNonWhitespaceChar ? ReadOnlySpan<char>.Empty : chars.Slice(startAt, length);
        }
    }
}