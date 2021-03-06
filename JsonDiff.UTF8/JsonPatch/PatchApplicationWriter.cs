using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using DiffMatchPatch;
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
            
            jsonDocument.DepthFirstTraversal(ProcessElement);
            
            WriteEndTokens(endTokens.Count);
            
            JsonDocumentTraversal.TraversalNextStep ProcessElement(JsonDocumentTraversal.Context context)
            {
                if (endTokens.Count > 0)
                {
                    var prior = endTokens.Peek();
                    if (!context.Path.IsChild(prior.Path))
                    {
                        var (_, distance) = prior.Path.GetLowestCommonAncestor(context.Path);
                        WriteEndTokens(distance);
                    }
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
                        case PatchText patchText:
                            if (context.Path.ValueKind == JsonPathValueKind.Property)
                            {
                                writer.WritePropertyName(context.Path.GetPropertyName());
                            }
                            var toPatch = context.JsonElement.GetString();
                            var patchResult = patchText.Patches.Apply(toPatch);
                            if (patchResult.results.Any(x => x == false))
                            {
                                throw new InvalidOperationException("Merge conflict applying patch");
                            }
                            writer.WriteStringValue(patchResult.newText);
                            return JsonDocumentTraversal.TraversalNextStep.Skip;
                    }
                }
                else if (_patchList.IsCurrentElementOrChildPatched(context.Path))
                {
                    if (context.Path.ValueKind is JsonPathValueKind.Property)
                    {
                        writer.WritePropertyName(context.Path.GetPropertyName());
                    }
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
                var valueKind = context.JsonElement.ValueKind;
                var endToken = new EndToken(context.JsonElement, context.Path);
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
                        var arrayLength = endToken.Element.GetArrayLength();
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
            
            public EndToken(JsonElement jsonElement, JsonPath path)
            {
                Element = jsonElement;
                Path = path;
            }

            public JsonElement Element { get; }
            public JsonPath Path { get; }

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