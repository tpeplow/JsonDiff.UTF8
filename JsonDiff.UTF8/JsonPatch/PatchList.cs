using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace JsonDiff.UTF8.JsonPatch
{
    /// <summary>
    /// Based on https://datatracker.ietf.org/doc/html/rfc6902
    /// </summary>
    public class PatchList : IEnumerable<Operation>
    {
        readonly Dictionary<JsonPath, Operation> _inPlaceOperations = new();
        readonly Dictionary<JsonPath, List<Add>> _addOperations = new();
        readonly HashSet<JsonPath> _patchedPaths = new();

        public void Add(Operation operation)
        {
            switch (operation)
            {
                case Add add:
                    var addTo = add.Path.Parent ?? JsonPath.WholeDocument;
                    if (!_addOperations.TryGetValue(addTo, out var items))
                    {
                        items = new List<Add>();
                        _addOperations.Add(addTo, items);
                    }
                    items.Add(add);
                    // also add it as an in-place operation in case someone uses add to mean replace
                    // rfc6902 says 'If the target location specifies an object member that does exist, that member's value is replaced.'
                    _inPlaceOperations.Add(operation.Path, operation);
                    break;
                case Remove:
                case Replace:
                    _inPlaceOperations.Add(operation.Path, operation);
                    break;
            }
            
            var parent = operation.Path.Parent;
            while (parent != null && _patchedPaths.Add(parent))
            {
                parent = parent.Parent;
            }
        }
        
        public int Count => _inPlaceOperations.Count;
        public Operation this[JsonPath value] => _inPlaceOperations[value];

        public bool IsCurrentElementOrChildPatched(JsonPath path)
        {
            if (Equals(path, JsonPath.WholeDocument) && Count > 0) return true;
            return _inPlaceOperations.ContainsKey(path) || _patchedPaths.Contains(path) || _addOperations.ContainsKey(path);
        }

        public bool TryGetPatch(JsonPath path, [MaybeNullWhen(false)]out Operation operation)
        {
            return _inPlaceOperations.TryGetValue(path, out operation);
        }

        public IEnumerable<Add> GetAddedElements(JsonPath path)
        {
            return _addOperations.TryGetValue(path, out var addOperations) ? addOperations : Enumerable.Empty<Add>();
        }

        public void ApplyPatch(JsonDocument jsonDocument, Utf8JsonWriter writeTo)
        {
            var patchWriter = new PatchApplicationWriter(this);
            patchWriter.Patch(jsonDocument, writeTo);
        }

        public void WriteTo(Utf8JsonWriter writer)
        {
            
        }

        public IEnumerator<Operation> GetEnumerator()
        {
            return _inPlaceOperations.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}