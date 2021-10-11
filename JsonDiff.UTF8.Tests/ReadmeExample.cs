using System;
using System.IO;
using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests
{
    public class ReadmeExample
    {
        [Test]
        public void Diff_FromString()
        {
            var left = @"{ ""key"": false }";
            var right = @"{ ""key"": true }";

            PatchList path = JsonComparer.Compare(left.AsMemory(), right.AsMemory());
        }
        
        [Test]
        public void Diff_JsonDocument()
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