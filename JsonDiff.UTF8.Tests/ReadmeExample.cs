using System.IO;
using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests
{
    public class ReadmeExample
    {
        [Test]
        public void Diff()
        {
            var left = JsonDocument.Parse(@"{ ""key"": false }");
            var right = JsonDocument.Parse(@"{ ""key"": true }");

            PatchList path = left.CompareWith(right);
        }

        [Test]
        public void Patch()
        {
            var left = JsonDocument.Parse(@"{ ""key"": false }");
            var right = JsonDocument.Parse(@"{ ""key"": true }");

            var memoryStream = new MemoryStream();
            var jsonWriter = new Utf8JsonWriter(memoryStream);
            left.CompareWith(right)
                .ApplyPatch(left, jsonWriter);
        }
    }
}