using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;

namespace JsonDiff.UTF8
{
    public static class JsonDocumentExtensions
    {
        public static JsonElement EvaluatePath(this JsonDocument document, string pathString)
        {
            var path = JsonPath.Parse(pathString);
            return path.Evaluate(document);
        }

        public static JsonElement EvaluatePath(this JsonDocument document, JsonPath pathString)
        {
            return pathString.Evaluate(document);
        }

        public static PatchList CompareWith(this JsonDocument baseDocument, JsonDocument other, JsonComparerOptions? options = null)
        {
            var comparer = new JsonComparer(options);
            return comparer.Compare(baseDocument, other);
        }
    }
}