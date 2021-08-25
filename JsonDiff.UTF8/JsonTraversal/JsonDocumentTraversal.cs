using System;
using System.Text.Json;

namespace JsonDiff.UTF8.JsonTraversal
{
    public static class JsonDocumentTraversal
    {
        public enum TraversalNextStep
        {
            Continue,
            Skip,
            Stop
        }

        public readonly struct Context
        {
            public Context(JsonPath path, int depth, JsonElement jsonElement)
            {
                Path = path;
                Depth = depth;
                JsonElement = jsonElement;
            }

            public JsonPath Path { get; }
            public int Depth { get; }
            public JsonElement JsonElement { get; }
        }
        
        public static void DepthFirstTraversal(this JsonDocument document, Func<Context, TraversalNextStep> processElement)
        {
            var stack = new DepthFirstTraversalStack<Context>();
            stack.Push(new Context(JsonPath.WholeDocument, 0, document.RootElement));
            while (stack.Count > 0)
            {
                var context = stack.Pop();
                var result = processElement(context);
                switch (result)
                {
                    case TraversalNextStep.Skip:
                        continue;
                    case TraversalNextStep.Stop:
                        return;
                    case TraversalNextStep.Continue:
                    default:
                        switch (context.JsonElement.ValueKind)
                        {
                            case JsonValueKind.Object:
                                using (stack.ReverseOrder())
                                {
                                    var depth = context.Depth + 1;
                                    foreach (var item in context.JsonElement.EnumerateObject())
                                    {
                                        stack.Push(new Context(context.Path.CreateChild(item.Name), depth, item.Value));
                                    }
                                }
                                break;
                            case JsonValueKind.Array:
                                using (stack.ReverseOrder())
                                {
                                    var i = 0;
                                    var depth = context.Depth + 1;
                                    foreach (var item in context.JsonElement.EnumerateArray())
                                    {
                                        stack.Push(new Context(context.Path.CreateChild(i++), depth, item));
                                    }
                                }
                                break;
                            case JsonValueKind.String:
                            case JsonValueKind.Number:
                            case JsonValueKind.True:
                            case JsonValueKind.False:
                            case JsonValueKind.Null:
                            case JsonValueKind.Undefined:
                                continue;
                        }
                        break;
                }
            }
        }
    }
}