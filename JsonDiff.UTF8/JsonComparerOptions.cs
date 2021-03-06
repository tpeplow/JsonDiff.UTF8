using System;

namespace JsonDiff.UTF8
{
    public class JsonComparerOptions
    {
        public StringComparison StringComparison { get; set; } = StringComparison.Ordinal;
        public bool UseDiffMatchPatchForStrings { get; set; } = true;
    }
}