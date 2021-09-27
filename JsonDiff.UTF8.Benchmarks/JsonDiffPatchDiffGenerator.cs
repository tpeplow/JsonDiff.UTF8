using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace JsonDiff.UTF8.Benchmarks
{
    public class JsonDiffPatchDiffGenerator : IDiffGenerator
    {
        readonly JsonDiffPatch _jdp = new();
        JToken _baseJson;
        JToken _otherJson;
        JToken _patchList;
        string _baseJsonText;

        public void Setup(string baseJson, string otherJson)
        {
            _baseJsonText = baseJson;
            _baseJson = JToken.Parse(baseJson);
            _otherJson = JToken.Parse(otherJson);
        }

        public void PerformDiff()
        {
            var result = _jdp.Diff(_baseJson, _otherJson);
        }

        public void PerformPatch()
        {
            _patchList ??= _jdp.Diff(_baseJson, _otherJson);
            _jdp.Patch(JToken.Parse(_baseJsonText), _patchList);
        }
    }
}