using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using JsonDiff.UTF8.JsonTraversal;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests.JsonTraversal
{
    [TestFixture]
    public class JsonDocumentTraversalTests
    {
        [Test]
        public void when_traversing_a_json_document_depth_first()
        {
            var json = JsonDocument.Parse(
                "{" +
                "  \"a\" : 1," +
                "  \"b\" :  [2, 3, 4, 5], " +
                "  \"c\" : { \"ca\" : 6, \"cb\" : [7, 8], \"cc\" : { \"cca\" : 9 } }," +
                "  \"d\" : 10" +
                "}");

            var items = new List<(JsonPath, JsonElement)>();
            json.DepthFirstTraversal(context =>
            {
                var path = context.Path;
                var element = context.JsonElement;
                Console.WriteLine($"{context.Depth} :: {path} :: {element}");
                items.Add((path, element));
                return JsonDocumentTraversal.TraversalNextStep.Continue;
            });

            var i = 0; 
            items[i++].Item1.Should().Be(JsonPath.WholeDocument);
            items[i++].Item1.Should().Be(JsonPath.Parse("/a"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/b"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/b/0"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/b/1"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/b/2"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/b/3"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c/ca"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c/cb"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c/cb/0"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c/cb/1"));
            items[i++].Item1.Should().Be(JsonPath.Parse("/c/cc"));
            items[i].Item1.Should().Be(JsonPath.Parse("/c/cc/cca"));

            items
                .Where(x => x.Item2.ValueKind == JsonValueKind.Number)
                .Select(x => x.Item2.GetInt32())
                .Should().Equal(Enumerable.Range(1, 10));
        }
    }
}