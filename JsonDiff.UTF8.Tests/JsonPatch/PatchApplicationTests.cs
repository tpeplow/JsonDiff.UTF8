using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests.JsonPatch
{
    [TestFixture]
    public class PatchApplicationTests
    {
        [Test]
        public void when_nothing_to_patch_within_an_array()
        {
            Patch("[ 10 ]", new PatchList()).EvaluatePath("/0").GetInt32().Should().Be(10);
        }

        [Test]
        public void when_nothing_to_patch_within_an_object()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : 20 }", new PatchList()); 
            patched.EvaluatePath("/a").GetInt32().Should().Be(10);
            patched.EvaluatePath("/b").GetInt32().Should().Be(20);
        }
        
        [Test]
        public void when_patching_the_first_element_of_an_array()
        {
            var patched = Patch("[0, 1]", new PatchList
            {
                new Replace(JsonPath.Parse("/0"), JsonDocument.Parse("10").RootElement)
            });

            patched.EvaluatePath("/0").GetInt32().Should().Be(10);
            patched.EvaluatePath("/1").GetInt32().Should().Be(1);
        }

        [Test]
        public void when_replacing_a_property_value()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : 20 }", new PatchList()
            {
                new Replace(JsonPath.Parse("/b"), JsonDocument.Parse("30").RootElement)
            });
            
            patched.EvaluatePath("/a").GetInt32().Should().Be(10);
            patched.EvaluatePath("/b").GetInt32().Should().Be(30);
        }

        [Test]
        public void when_removing_a_property_value()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : 20 }", new PatchList()
            {
                new Remove(JsonPath.Parse("/b"))
            });

            JsonPath.Parse("/a/b").TryEvaluate(patched, out _).Should().BeFalse();
        }

        [Test]
        public void when_removing_a_property_value_from_an_object()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : { \"c\" : 20 } }", new PatchList()
            {
                new Remove(JsonPath.Parse("/b/c"))
            });

            JsonPath.Parse("/a/b").TryEvaluate(patched, out _).Should().BeFalse();
        }
        
        [Test]
        public void when_replacing_a_property_value_from_a_child_object()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : { \"c\" : 20 } }", new PatchList()
            {
                new Replace(JsonPath.Parse("/b/c"), JsonDocument.Parse("30").RootElement)
            });

            patched.EvaluatePath("/b/c").GetInt32().Should().Be(30);
        }

        [Test]
        public void when_removing_an_item_from_an_array()
        {
            var patched = Patch("[0, 1]", new PatchList
            {
                new Remove(JsonPath.Parse("/0"))
            });

            JsonPath.Parse("/1").TryEvaluate(patched, out _).Should().BeFalse();
        }
        
        [Test]
        public void when_adding_an_item_to_an_array()
        {
            var patched = Patch("[1]", new PatchList
            {
                new Add(JsonPath.Parse("/1"), JsonDocument.Parse("10").RootElement)
            });

            patched.EvaluatePath("/1").GetInt32().Should().Be(10);
        }

        [Test]
        public void when_adding_an_item_to_an_object()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : 20 }", new PatchList()
            {
                new Add(JsonPath.Parse("/c"), JsonDocument.Parse("10").RootElement)
            });

            patched.EvaluatePath("/c").GetInt32().Should().Be(10);
            patched.RootElement.EnumerateObject().Select(x => x.Name).Should().Equal("a", "b", "c");
        }

        [Test]
        public void when_adding_an_item_with_invalid_index()
        {
            Action action = () => Patch("[1]", new PatchList
            {
                new Add(JsonPath.Parse("/2"), JsonDocument.Parse("10").RootElement)
            });

            action.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void when_adding_an_item_to_an_array_at_an_existing_element_index()
        {
            var patched = Patch("[1]", new PatchList
            {
                new Add(JsonPath.Parse("/0"), JsonDocument.Parse("10").RootElement)
            });

            patched.EvaluatePath("/0").GetInt32().Should().Be(10);
        }
        
        [Test]
        public void when_adding_an_property_that_already_exists()
        {
            var patched = Patch("{ \"a\" : 10, \"b\" : 20 }", new PatchList
            {
                new Add(JsonPath.Parse("/b"), JsonDocument.Parse("100").RootElement)
            });

            patched.EvaluatePath("/b").GetInt32().Should().Be(100);
            patched.RootElement.EnumerateObject().Select(x => x.Name).Should().Equal("a", "b");
        }

        [Test]
        public void when_array_is_empty_after_all_removals()
        {
            var patched = Patch("{\"a\" : 1, \"b\": [0] }", new PatchList
            {
                new Remove(JsonPath.Parse("/b/0"))
            });

            patched.EvaluatePath("/b").GetArrayLength().Should().Be(0);
        }

        public JsonDocument Patch(string json, PatchList patchList)
        {
            var stream = new MemoryStream();
            var document = JsonDocument.Parse(json);
            using (var writer = new Utf8JsonWriter(stream))
            {
                patchList.ApplyPatch(document, writer);
            }
            stream.Position = 0;
            return JsonDocument.Parse(stream);
        }
    }
}