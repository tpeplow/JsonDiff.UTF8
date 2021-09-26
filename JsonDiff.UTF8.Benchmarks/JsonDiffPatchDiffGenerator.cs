using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace JsonDiff.UTF8.Benchmarks
{
    public class JsonDiffPatchDiffGenerator : IDiffGenerator
    {
        JToken _baseJson;
        JToken _otherJson;

        public void Setup(string baseJson, string otherJson)
        {
            _baseJson = JToken.Parse(baseJson);
            _otherJson = JToken.Parse(otherJson);
        }

        public void PerformDiff()
        {
            var jdp = new JsonDiffPatch();
            var result = jdp.Diff(_baseJson, _otherJson);
        }
    }
}