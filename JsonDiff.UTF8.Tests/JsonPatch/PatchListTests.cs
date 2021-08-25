using System.IO;
using System.Text.Json;
using FluentAssertions;
using JsonDiff.UTF8.JsonPatch;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests.JsonPatch
{
    [TestFixture]
    public class PatchListTests
    {
        [Test]
        public void when_patch_list_is_empty()
        {
            new PatchList().IsCurrentElementOrChildPatched(JsonPath.WholeDocument).Should().BeFalse();
        }

        [Test]
        public void when_property_is_added_to_root_element()
        {
            var patchList = new PatchList
            {
                new Add(JsonPath.Parse("/something"), new JsonElement())
            };
 
            patchList.IsCurrentElementOrChildPatched(JsonPath.WholeDocument).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/something")).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/different")).Should().BeFalse();
        }

        [Test]
        public void when_property_is_added_to_an_element_in_an_array()
        {
            var patchList = new PatchList
            {
                new Add(JsonPath.Parse("/0/something/child"), new JsonElement())
            };
            
            patchList.IsCurrentElementOrChildPatched(JsonPath.WholeDocument).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/something")).Should().BeFalse();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/0")).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/0/something")).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/0/something/child")).Should().BeTrue();
            patchList.IsCurrentElementOrChildPatched(JsonPath.Parse("/1")).Should().BeFalse();
        }
    }
}