using System.Text.Json;

namespace JsonDiff.UTF8.Benchmarks
{
    public class Utf8DiffGenerator : IDiffGenerator
    {
        JsonDocument _baseJsonDocument;
        JsonDocument _otherJsonDocument;

        public void Setup(string baseJson, string otherJson)
        {
            _baseJsonDocument = JsonDocument.Parse(baseJson);
            _otherJsonDocument = JsonDocument.Parse(otherJson);
        }

        public void PerformDiff()
        {
            var result = _baseJsonDocument.CompareWith(_otherJsonDocument);
        }
    }
}