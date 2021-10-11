using System;
using FluentAssertions;
using NUnit.Framework;

namespace JsonDiff.UTF8.Tests
{
    [TestFixture]
    public class JsonTextCompareTests
    {
        [Test]
        public void when_empty_string()
        {
            var result = JsonTextComparer.IsEqual("".AsSpan(), "".AsSpan());
            result.Should().BeTrue();
        }
        
        [Test]
        public void when_same_string()
        {
            var result = JsonTextComparer.IsEqual("{ }".AsSpan(), "{ }".AsSpan());
            result.Should().BeTrue();
        }

        [Test]
        public void when_same_string_with_whitespace_difference()
        {
            var result = JsonTextComparer.IsEqual("{\t\n}".AsSpan(), "{   \r     }".AsSpan());
            result.Should().BeTrue();
        }

        [Test]
        public void when_whitespace_differences_are_inside_quote()
        {
            var result = JsonTextComparer.IsEqual("{\"a\" : \"aaa\" }".AsSpan(), "{\"a\" : \"aaa   \" }".AsSpan());
            result.Should().BeFalse();
        }
        
        [Test]
        public void when_whitespace_differences_are_inside_quote_that_contains_escaped_quote()
        {
            var result = JsonTextComparer.IsEqual("{\"a\" : \"a\\\"aa\" }".AsSpan(), "{\"a\" : \"a\\\"a   a\" }".AsSpan());
            result.Should().BeFalse();
        }
    }
}