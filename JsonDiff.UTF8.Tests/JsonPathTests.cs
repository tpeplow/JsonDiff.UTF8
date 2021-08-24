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