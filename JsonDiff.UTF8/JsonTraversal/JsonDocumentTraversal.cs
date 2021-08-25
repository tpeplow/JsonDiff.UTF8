using System;
using System.Net.Sockets;
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
        
        public static void DepthFirstTraversal(this JsonDocument document, Func<JsonPath, JsonElement, TraversalNextStep> processElement)
        {
            var stack = new DepthFirstTraversalStack<(JsonPath, JsonElement)>();
            stack.Push((JsonPath.WholeDocument, document.RootElement));
            while (stack.Count > 0)
            {
                var (path, element) = stack.Pop();
                var result = processElement(path, element);
                switch (result)
                {
                    case TraversalNextStep.Skip:
                        continue;
                    case TraversalNextStep.Stop:
                        return;
                    case TraversalNextStep.Continue:
                    default:
                        switch (element.ValueKind)
                        {
                            case JsonValueKind.Object:
                                using (stack.ReverseOrder())
                                {
                                    foreach (var item in element.EnumerateObject())
                                    {
                                        stack.Push((path.CreateChild(item.Name), item.Value));
                                    }
                                }
                                break;
                            case JsonValueKind.Array:
                                using (stack.ReverseOrder())
                                {
                                    var i = 0;
                                    foreach (var item in element.EnumerateArray())
                                    {
                                        stack.Push((path.CreateChild(i++), item));
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