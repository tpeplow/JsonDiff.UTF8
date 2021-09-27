using System.IO;
using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;

namespace JsonDiff.UTF8.Benchmarks
{
    public class Utf8DiffGenerator : IDiffGenerator
    {
        readonly MemoryStream _patchBuffer = new();
        JsonDocument _baseJsonDocument;
        JsonDocument _otherJsonDocument;
        PatchList _patchList;
        string _baseJsonText;

        public void Setup(string baseJson, string otherJson)
        {
            _baseJsonText = baseJson;
            _baseJsonDocument = JsonDocument.Parse(baseJson);
            _otherJsonDocument = JsonDocument.Parse(otherJson);
        }

        public void PerformDiff()
        {
            var result = _baseJsonDocument.CompareWith(_otherJsonDocument);
        }

        public void PerformPatch()
        {
            _patchList ??= _baseJsonDocument.CompareWith(_otherJsonDocument);
            var writer = new Utf8JsonWriter(_patchBuffer);
            // to match the way jpd works, parse the document each time for a fair test
            _patchList.ApplyPatch(JsonDocument.Parse(_baseJsonText), writer);
            _patchBuffer.Position = 0;
        }
    }
}