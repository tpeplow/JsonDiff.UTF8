using System;
using System.Collections.Generic;
using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;
using JsonDiff.UTF8.JsonTraversal;

namespace JsonDiff.UTF8
{
    public class JsonComparer
    {
        readonly JsonComparerOptions _options;

        public JsonComparer(JsonComparerOptions? options = null)
        {
            _options = options ?? new JsonComparerOptions();
        }

        public PatchList Compare(JsonDocument baseJsonDocument, JsonDocument otherJsonDocument)
        {
            var comparision = new JsonComparision(_options, baseJsonDocument, otherJsonDocument);
            comparision.Execute();
            return comparision.PatchList;
        }

        class JsonComparision
        {
            readonly JsonComparerOptions _options;
            readonly DepthFirstTraversalStack<(JsonElement Base, JsonElement Other, JsonPath Path)> _elementQueue = new();
            readonly Dictionary<string, JsonElement> _objectElements = new();

            public JsonComparision(JsonComparerOptions options, JsonDocument baseJsonDocument,
                JsonDocument otherJsonDocument)
            {
                _options = options;
                _elementQueue.Push((baseJsonDocument.RootElement, otherJsonDocument.RootElement, JsonPath.WholeDocument));
            }

            public PatchList PatchList { get; } = new();

            public void Execute()
            {
                while (_elementQueue.Count > 0)
                {
                    var (baseElement, otherElement, path) = _elementQueue.Pop();
                    if (baseElement.ValueKind != otherElement.ValueKind)
                    {
                        PatchList.Add(new Replace(path, otherElement));
                        continue;
                    }

                    switch (baseElement.ValueKind)
                    {
                        case JsonValueKind.Object:
                            _elementQueue.PushReversed(() => CompareObject(baseElement, otherElement, path));
                            break;
                        case JsonValueKind.Array:
                            _elementQueue.PushReversed(() => CompareArray(baseElement, otherElement, path));
                            break;
                        case JsonValueKind.String:
                            var equals = string.Equals(baseElement.GetString(), otherElement.GetString(), _options.StringComparison);
                            if (equals) continue;
                            PatchList.Add(new Replace(path, otherElement));
                            break;
                        case JsonValueKind.Number:
                        case JsonValueKind.True:
                        case JsonValueKind.False:
                            var baseText = baseElement.GetRawText();
                            var otherText = otherElement.GetRawText();
                            if (baseText.Equals(otherText)) continue;
                            PatchList.Add(new Replace(path, otherElement));
                            break;
                        case JsonValueKind.Null:
                            // validating the two elements are the same type means this will never be entered
                            break;
                        case JsonValueKind.Undefined:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            IEnumerable<(JsonElement basePropertyElement, JsonElement Value, JsonPath newElementPath)> CompareObject(JsonElement baseElement, JsonElement otherElement, JsonPath path)
            {
                foreach (var item in baseElement.EnumerateObject())
                {
                    _objectElements.Add(item.Name, item.Value);
                }
                
                foreach (var newElement in otherElement.EnumerateObject())
                {
                    var newElementName = newElement.Name;
                    var newElementPath = path.CreateChild(newElementName);
                    var isFound = _objectElements.TryGetValue(newElementName, out var basePropertyElement);
                    if (isFound)
                    {
                        yield return (basePropertyElement, newElement.Value, newElementPath);
                        _objectElements.Remove(newElementName);
                        continue;
                    }

                    PatchList.Add(new Add(newElementPath, newElement.Value));
                }

                foreach (var removed in _objectElements)
                {
                    PatchList.Add(new Remove(path.CreateChild(removed.Key)));
                }
                
                _objectElements.Clear();
            }

            IEnumerable<(JsonElement, JsonElement, JsonPath arrayItemPath)> CompareArray(JsonElement baseElement, JsonElement otherElement, JsonPath path)
            {
                var i = 0;
                var baseElementEnumerator = baseElement.EnumerateArray();
                var otherElementEnumerator = otherElement.EnumerateArray();
                bool baseElementMovedNext;
                bool otherElementMovedNext;


                do
                {
                    var arrayItemPath = path.CreateChild(i);
                    baseElementMovedNext = baseElementEnumerator.MoveNext();
                    otherElementMovedNext = otherElementEnumerator.MoveNext();

                    if (otherElementMovedNext && baseElementMovedNext)
                    {
                        yield return (baseElementEnumerator.Current, otherElementEnumerator.Current, arrayItemPath);
                    }
                    else if (otherElementMovedNext)
                    {
                        PatchList.Add(new Add(arrayItemPath, otherElementEnumerator.Current));
                    }
                    else if (baseElementMovedNext)
                    {
                        PatchList.Add(new Remove(arrayItemPath));
                    }

                    i++;
                } while (baseElementMovedNext || otherElementMovedNext);
            }
        }
    }
}