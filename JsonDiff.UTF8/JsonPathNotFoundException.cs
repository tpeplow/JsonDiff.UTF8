using System;

namespace JsonDiff.UTF8
{
    public class JsonPathNotFoundException : Exception
    {
        public JsonPathNotFoundException(string message) : base(message)
        {
        }
    }
}