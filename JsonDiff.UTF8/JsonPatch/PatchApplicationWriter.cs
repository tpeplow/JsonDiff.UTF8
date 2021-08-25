using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JsonDiff.UTF8.JsonTraversal;

namespace JsonDiff.UTF8.JsonPatch
{
    class PatchApplicationWriter
    {
        readonly PatchList _patchList;

        public PatchApplicationWriter(PatchList patchList)
        {
            _patchList = patchList;
        }

        public void Patch(JsonDocument jsonDocument, Utf8JsonWriter writer)
        {
            var endTokens = new Stack<EndToken>();
            JsonDocumentTraversal.Context prior = default;
            
            jsonDocument.DepthFirstTraversal(ProcessElement);
            
            WriteEndTokens(endTokens.Count);
            
            JsonDocumentTraversal.TraversalNextStep ProcessElement(JsonDocumentTraversal.Context context)
            {
                if (context.Depth < prior.Depth)
                {
                    WriteEndTokens(prior.Depth - context.Depth);
                }

                if (_patchList.TryGetPatch(context.Path, out var patch))
                {
                    switch (patch)
                    {
                        case Remove:
                            return JsonDocumentTraversal.TraversalNextStep.Skip;
                        case Replace replace:
                            WriteElement(replace.Value, context.Path);
                            return JsonDocumentTraversal.TraversalNextStep.Skip;
                        case Add add:
                            WriteElement(add.Value, context.Path);
                            endTokens.Peek().Remove(add);
                            return JsonDocumentTraversal.TraversalNextStep.Skip;
                    }
                    
                }
                else if (_patchList.IsCurrentElementOrChildPatched(context.Path))
                {
                    var token = WriteBeginToken(context);
                    foreach (var added in _patchList.GetAddedElements(context.Path))
                    {
                        token.Add(added);
                    }
                    return JsonDocumentTraversal.TraversalNextStep.Continue;
                }

                WriteElement(context.JsonElement, context.Path);
                
                return JsonDocumentTraversal.TraversalNextStep.Skip;
            }

            EndToken WriteBeginToken(JsonDocumentTraversal.Context context)
            {
                prior = context;
                var valueKind = context.JsonElement.ValueKind;
                var endToken = new EndToken(context.JsonElement);
                endTokens.Push(endToken);
                switch (valueKind)
                {
                    case JsonValueKind.Object:
                        writer.WriteStartObject();
                        break;
                    case JsonValueKind.Array:
                        writer.WriteStartArray();
                        break;
                    default:
                        throw new InvalidOperationException($"Cannot write begin token for {valueKind}");
                }

                return endToken;
            }

            void WriteElement(JsonElement element, JsonPath path)
            {
                if (path.ValueKind is JsonPathValueKind.Property)
                {
                    writer.WritePropertyName(path.GetPropertyName());
                }
                element.WriteTo(writer);
            }

            void WriteEndTokens(int toClose)
            {
                for (; toClose > 0; toClose--)
                {
                    var endToken = endTokens.Pop();
                    if (endToken.Element.ValueKind == JsonValueKind.Array)
                    {
                        var arrayLength = prior.JsonElement.GetArrayLength();
                        var sorted = endToken.AddedItems
                            .Select(x => (x.Value, Index: x.Path.GetArrayIndex()))
                            .OrderBy(x => x.Index);
                        foreach (var added in sorted)
                        {
                            if (added.Index != arrayLength++)
                            {
                                throw new InvalidOperationException($"Cannot add item at {added.Index}");
                            }
                            added.Value.WriteTo(writer);
                        }
                        writer.WriteEndArray();
                    }
                    else
                    {
                        foreach (var added in endToken.AddedItems)
                        {
                            writer.WritePropertyName(added.Path.GetPropertyName());
                            added.Value.WriteTo(writer);
                        }
                        writer.WriteEndObject();
                    }
                }
            }
        }

        class EndToken
        {
            readonly Dictionary<JsonPath, Add> _addedElements = new();
            
            public EndToken(JsonElement jsonElement)
            {
                Element = jsonElement;
            }

            public JsonElement Element { get; }

            public IEnumerable<Add> AddedItems => _addedElements.Values;
            
            public void Add(Add add)
            {
                _addedElements.Add(add.Path, add);
            }

            public bool Remove(Add add)
            {
                return _addedElements.Remove(add.Path);
            }
        }
    }
}