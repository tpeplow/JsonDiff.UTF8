using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using JsonDiff.UTF8.JsonMerge;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests.JsonMerge
{
    public class JsonMergeTests
    {
        [Test]
        public void when_the_same_property_is_added()
        {
            var left = new PatchList
            {
                new Add(JsonPath.Parse("/a"), JsonDocument.Parse("1").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(false);
        }

        [Test]
        public void when_a_different_property_is_added()
        {
            var left = new PatchList
            {
                new Add(JsonPath.Parse("/a"), JsonDocument.Parse("1").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/b"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(true);
            merged.PatchList.Should().Equal(left.Concat(right));
        }

        [Test]
        public void when_item_added_to_the_same_array_position()
        {
            var left = new PatchList
            {
                new Add(JsonPath.Parse("/a/0"), JsonDocument.Parse("1").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a/0"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(false);
        }

        [Test]
        public void when_item_added_to_the_different_array_position()
        {
            var left = new PatchList
            {
                new Add(JsonPath.Parse("/a/0"), JsonDocument.Parse("1").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a/1"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(true);
            merged.PatchList.Should().Equal(left.Concat(right));
        }

        [Test]
        public void when_property_added_to_removed_parent()
        {
            var left = new PatchList
            {
                new Remove(JsonPath.Parse("/a")),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a/0"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(false);
        }

        [Test]
        public void when_property_added_to_replaced_parent()
        {
            var left = new PatchList
            {
                new Replace(JsonPath.Parse("/a"), JsonDocument.Parse("[0,1,2]").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a/0"), JsonDocument.Parse("2").RootElement),
            };

            var merged = left.TryMerge(right);
            merged.Success.Should().Be(false);
        }

        [Test]
        public void when_item_added_to_child_of_changed_sibling()
        {
            var left = new PatchList
            {
                new Add(JsonPath.Parse("/a/b"), JsonDocument.Parse("1").RootElement),
            };
            var right = new PatchList
            {
                new Add(JsonPath.Parse("/a/c/d"), JsonDocument.Parse("2").RootElement),
            };
            
            var merged = left.TryMerge(right);
            merged.Success.Should().Be(true);
            merged.PatchList.Should().Equal(left.Concat(right));
        }

        [Test]
        public void when_three_way_merging()
        {
            ThreeWayMerge(
                    @"{ ""a"" : 1 }", 
                    @"{ ""a"" : 1, ""b"" : 1 }", 
                    @"{ ""a"" : 1, ""c"" : 1 }")
                .Should().Be(@"{""a"":1,""b"":1,""c"":1}");

            ThreeWayMerge(
                    @"{ ""a"" : 1 }", 
                    @"{ ""a"" : 1, ""b"" : 1 }", 
                    @"{ ""c"" : 1 }")
                .Should().Be(@"{""b"":1,""c"":1}");
            
            ThreeWayMerge(
                    @"{ ""a"" : 1 }", 
                    @"{ ""a"" : 2, ""b"" : 1 }", 
                    @"{ ""a"" : 1, ""c"" : 1 }")
                .Should().Be(@"{""a"":2,""b"":1,""c"":1}");
        }
        
        string ThreeWayMerge(string baseJson, string leftJson, string rightJson)
        {
            var baseDocument = JsonDocument.Parse(baseJson);
            var leftDocument = JsonDocument.Parse(leftJson);
            var rightDocument = JsonDocument.Parse(rightJson);

            var result = baseDocument.ThreeWayMerge(leftDocument, rightDocument);
            using var stream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(stream);
            result.PatchList.ApplyPatch(baseDocument, jsonWriter);
            jsonWriter.Dispose();
            
            stream.Position = 0;
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }
}