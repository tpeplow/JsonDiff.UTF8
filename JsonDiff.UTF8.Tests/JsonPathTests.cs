using System;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests
{
    [TestFixture]
    public class JsonPathTests
    {
        [Test]
        public void when_comparing_whole_document_with_whole_document()
        {
            AssertEqual(JsonPath.WholeDocument, JsonPath.WholeDocument);
        }

        [Test]
        public void when_path_points_to_the_same_property_on_the_root()
        {
            AssertEqual(JsonPath.Parse("/Fred"), JsonPath.Parse("/Fred"));
        }

        [Test]
        public void when_path_points_to_an_array_item_on_the_root()
        {
            AssertEqual(JsonPath.Parse("/1"), JsonPath.Parse("/1"));
        }

        [Test]
        public void when_path_points_to_the_same_child_property()
        {
            AssertEqual(JsonPath.Parse("/Fred/1/Bob"), JsonPath.Parse("/Fred/1/Bob"));
        }
        
        [Test]
        public void when_path_points_to_the_same_child_property_but_different_parent()
        {
            AssertNotEqual(JsonPath.Parse("/Fred/2/Bob"), JsonPath.Parse("/Fred/1/Bob"));
        }

        [Test]
        public void when_paths_are_different_depths()
        {
            AssertNotEqual(JsonPath.Parse("/Fred/2/Bob"), JsonPath.Parse("/Fred/1"));
            AssertNotEqual(JsonPath.Parse("/Fred/2"), JsonPath.Parse("/Fred/1/Bob"));
        }

        [Test]
        public void when_path_points_to_element_outside_of_the_array()
        {
            Action parse = () => JsonDocument.Parse("[0, 1]").EvaluatePath("/2");
            parse.Should().Throw<JsonPathNotFoundException>();
        }

        [Test]
        public void when_root_is_parent()
        {
            var path = JsonPath.Parse("/a");
            path.IsChild(JsonPath.WholeDocument).Should().Be(true);
            var (parent, distance) = path.GetLowestCommonAncestor(JsonPath.WholeDocument);
            parent.Should().Be(JsonPath.WholeDocument);
            distance.Should().Be(1);
        }
        
        [Test]
        public void when_item_is_parent()
        {
            var path = JsonPath.Parse("/a/b");
            var child = path.CreateChild("c");
            child.IsChild(path).Should().Be(true);
        }

        [Test]
        public void when_item_is_grand_parent()
        {
            var path = JsonPath.Parse("/a/b");
            var child = path.CreateChild("c").CreateChild("d");
            child.IsChild(path).Should().Be(true);

            var (parent, distance) = child.GetLowestCommonAncestor(path);
            parent.Should().Be(child.Parent!.Parent);
            distance.Should().Be(2);
        }

        [Test]
        public void when_item_is_not_a_child()
        {
            var path = JsonPath.Parse("/a/b");
            var c = path.CreateChild("c");
            var d = path.CreateChild("d");
            var e = path.Parent!.CreateChild("e");

            d.IsChild(c).Should().Be(false);

            var (parent, distance) = d.GetLowestCommonAncestor(c);
            parent.Should().Be(path);
            distance.Should().Be(1);

            (parent, distance) = c.GetLowestCommonAncestor(d);
            parent.Should().Be(path);
            distance.Should().Be(1);
            
            (parent, distance) = e.GetLowestCommonAncestor(c);
            parent.Should().Be(path.Parent);
            distance.Should().Be(1);
        }
        
        static void AssertEqual(JsonPath path1, JsonPath path2)
        {
            Assert.That(path1, Is.EqualTo(path2));
            Assert.That(path1.GetHashCode(), Is.EqualTo(path2.GetHashCode()));
        }
        
        static void AssertNotEqual(JsonPath path1, JsonPath path2)
        {
            Assert.That(path1, Is.Not.EqualTo(path2));
            Assert.That(path1.GetHashCode(), Is.Not.EqualTo(path2.GetHashCode()));
        }
    }
}