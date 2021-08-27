using System.IO;
using System.Text.Json;
using FluentAssertions;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests
{
    [TestFixture]
    public class JsonComparerTests
    {
        [Test]
        public void when_root_nodes_are_both_properties_with_the_same_value()
        {
            Compare("{ \"Property\" : 1 }", "{ \"Property\" : 1 }");

            AssertMatch();
        }
        
        [Test]
        public void when_root_node_is_object_and_other_is_array()
        {
            Compare("{ \"Property\" : 1 }", "[1, 2]");
            
            AssertOperation(new Replace(JsonPath.WholeDocument, OtherJsonDocument.RootElement));
        }

        [Test]
        public void when_root_node_has_different_property_value()
        {
            Compare("{ \"Property\" : 1 }", "{ \"Property\" : 2 }");
            
            AssertSingleReplaced("/Property");
        }

        [Test]
        public void when_root_node_has_different_property_types()
        {
            Compare("{ \"Property\" : \"1\" }", "{ \"Property\" : 1 }");
            
            AssertSingleReplaced("/Property");
        }

        [Test]
        public void when_root_nodes_have_different_properties()
        {
            Compare("{ \"Property\" : \"1\" }", "{ \"Another\" : 1 }");
            
            AssertRemoved("/Property");
            AssertAdded("/Another");
            AssertCount(2);
        }
        
        [Test]
        public void when_root_node_has_one_matching_property_and_one_different_property()
        {
            Compare("{ \"Property\" : 1, \"Another\" : 0 }", "{ \"Property\" : 1, \"Another\" : 1 }");
            
            AssertSingleReplaced("/Another");
        }
        
        [Test]
        public void when_root_node_is_an_array_and_matches_other()
        {
            Compare("[0, 1]", "[0, 1]");
            
            AssertMatch();
        }

        [Test]
        public void when_root_node_is_an_array_and_has_different_element()
        {
            Compare("[0, 1]", "[1, 1]");
            
            AssertSingleReplaced("/0");
        }

        [Test]
        public void when_root_node_has_more_elements_than_the_other()
        {
            Compare("[0, 1]", "[0]");

            AssertCount(1);
            AssertRemoved("/1");
        }
        
        [Test]
        public void when_root_node_has_more_elements_than_the_other_and_elements_are_not_the_same()
        {
            Compare("[0, 1]", "[1]");

            AssertCount(2);
            AssertRemoved("/1");
            AssertReplaced("/0");
        }

        [Test]
        public void when_root_node_has_fewer_elements_than_the_other()
        {
            Compare("[0, 1]", "[0, 1, 2]");
            AssertCount(1);
            AssertAdded("/2");
        }

        [Test]
        public void when_root_node_has_fewer_elements_than_the_other_and_elements_are_not_the_same()
        {
            Compare("[0, 2]", "[0, 1, 2]");
            AssertCount(2);
            AssertReplaced("/1");
            AssertAdded("/2");
        }

        [Test]
        public void when_property_is_an_object_and_matches_other()
        {
            Compare("{ \"Property\" : { \"Inner\" : 0 } }", "{ \"Property\" : { \"Inner\" : 0 } }");
            AssertMatch();
        }
        
        [Test]
        public void when_property_is_an_object_and_inner_property_values_are_different()
        {
            Compare("{ \"Property\" : { \"Inner\" : 0 } }", "{ \"Property\" : { \"Inner\" : 1 } }");
            
            AssertSingleReplaced("/Property/Inner");
        }
        
        [Test]
        public void when_property_is_an_object_and_inner_properties_are_different()
        {
            Compare("{ \"Property\" : { \"Inner\" : 0 } }", "{ \"Property\" : { \"NewInner\" : 1 } }");
            
            AssertAdded("/Property/NewInner");
            AssertRemoved("/Property/Inner");
        }

        [Test]
        public void when_root_is_an_array_with_objects_that_match()
        {
            Compare("[{ \"Property\" : \"1\" }]", "[{ \"Property\" : \"1\" }]");
            
            AssertMatch();
        }
        
        [Test]
        public void when_root_is_an_array_with_objects_and_property_values_are_different()
        {
            Compare("[{ \"Property\" : \"0\" }]", "[{ \"Property\" : \"1\" }]");
            
            AssertSingleReplaced("/0/Property");
        }

        [Test]
        public void when_root_is_an_array_with_objects_that_have_different_properties()
        {
            Compare("[{ \"Property\" : \"0\" }]", "[{ \"Different\" : \"1\" }]");
            
            AssertCount(2);
            AssertRemoved("/0/Property");
            AssertAdded("/0/Different");
        }

        void AssertMatch()
        {
            Assert.That(Result.Count, Is.EqualTo(0));
        }

        void AssertCount(int count)
        {
            Assert.That(Result.Count, Is.EqualTo(count));
        }

        void AssertSingleReplaced(string replacedPath)
        {
            Assert.That(Result.Count, Is.EqualTo(1));
            AssertReplaced(replacedPath);
        }

        void AssertReplaced(string replacedPath)
        {
            var path = JsonPath.Parse(replacedPath);
            var newValue = OtherJsonDocument.EvaluatePath(path);
            var patch = ((Replace)Result[path]).Value;
            Assert.That(newValue, Is.EqualTo(patch));
        }

        void AssertAdded(string addedPath)
        {
            var path = JsonPath.Parse(addedPath);
            var newValue = OtherJsonDocument.EvaluatePath(path);
            var patch = ((Add)Result[path]).Value;
            Assert.That(newValue, Is.EqualTo(patch));
        }

        void AssertRemoved(string removed)
        {
            AssertOperation(new Remove(JsonPath.Parse(removed)));
        }

        void AssertOperation<T>(T operation)
            where T: Operation
        {
            Assert.AreEqual(operation, Result[operation.Path]);
        }
        
        PatchList Result { get; set; }
        JsonDocument OtherJsonDocument { get; set; }
        JsonDocument BaseJsonDocument { get; set; }
        
        void Compare(string baseJson, string otherJson)
        {
            BaseJsonDocument = JsonDocument.Parse(baseJson);
            OtherJsonDocument = JsonDocument.Parse(otherJson);

            Result = BaseJsonDocument.CompareWith(OtherJsonDocument);

            ApplyPatchToBaseAndAssertItMatchesOther();
        }

        void ApplyPatchToBaseAndAssertItMatchesOther()
        {
            var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                Result.ApplyPatch(BaseJsonDocument, writer);
            }

            stream.Position = 0;
            var patchedDocument = JsonDocument.Parse(stream);
            patchedDocument.CompareWith(OtherJsonDocument).Count.Should().Be(0);
        }
    }
}