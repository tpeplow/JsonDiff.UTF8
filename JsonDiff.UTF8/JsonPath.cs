using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace JsonDiff.UTF8
{
    public class JsonPath
    {
        public static readonly JsonPath WholeDocument = new();
        
        readonly string? _stringValue;
        readonly int _intValue;
        readonly JsonPath? _parent;

        JsonPath()
        {
            _stringValue = null;
        }

        JsonPath(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("token value must not be null or empty", nameof(value));
            if (value.Contains(" ")) throw new ArgumentException("token cannot contain spaces");
            _stringValue = value;
        }

        JsonPath(int value)
        {
            _stringValue = string.Empty;
            _intValue = value;
        }

        public static JsonPath Parse(string path)
        {
            var elements = path.Split('/');
            var result = WholeDocument;
            for (var i = 1; i < elements.Length; i++)
            {
                var element = elements[i];
                result = int.TryParse(element, out var n) ? result.CreateChild(n) : result.CreateChild(element);
            }

            return result;
        }

        public JsonPath? Parent
        {
            get => _parent;
            init
            {
                if (value == null || ReferenceEquals(WholeDocument, value))
                {
                    return;
                } 
                _parent = value;
            }
        }

        string Token => _stringValue == null ? string.Empty : _stringValue == string.Empty ? _intValue.ToString() : _stringValue;
        
        public JsonPathValueKind ValueKind =>
            _stringValue == null ? JsonPathValueKind.Root :
            _stringValue == string.Empty ? JsonPathValueKind.ArrayItem : JsonPathValueKind.Property;

        public JsonElement Evaluate(JsonDocument element)
        {
            InternalTryEvaluate(element, true, out var result);
            return result;
        }
        
        public bool TryEvaluate(JsonDocument document, out JsonElement result)
        {
            return InternalTryEvaluate(document, false, out result);
        }

        bool InternalTryEvaluate(JsonDocument document, bool throwIfNotFound, out JsonElement result)
        {
            JsonPath? notFound = null;
            string message = null;
            var current = document.RootElement;
            foreach (var item in WalkFromRoot())
            {
                if (item._stringValue == null) continue;
                if (item._stringValue != string.Empty)
                {
                    if (current.ValueKind != JsonValueKind.Object)
                    {
                        notFound = item;
                        message = $"{item} evaluated to {current.ValueKind} but should be an Object";
                        break;
                    }
                    if (!current.TryGetProperty(item._stringValue, out current))
                    {
                        notFound = item;
                        break;
                    }
                }
                else
                {
                    if (item._intValue < current.GetArrayLength())
                    {
                        current = current[item._intValue];
                        continue;
                    }
                    notFound = item;
                    break;
                }
            }

            if (notFound != null)
            {
                if (!throwIfNotFound)
                {
                    result = default;
                    return false;
                }
                if (notFound.Equals(this))
                {
                    throw new JsonPathNotFoundException($"'{this}' could not be found.");
                }

                message ??= $"Part of path '{notFound}' could not be found in path '{this}'";
                throw new JsonPathNotFoundException(message);
            }
            
            result = current;
            return true;
        }
        
        public JsonPath CreateChild(string token)
        {
            return new(token) {Parent = this};
        }

        public JsonPath CreateChild(int arrayIndex)
        {
            return new(arrayIndex) {Parent = this};
        }
        
        public override string ToString()
        {
            if (ReferenceEquals(this, WholeDocument))
            {
                return string.Empty;
            }
            
            var sb = new StringBuilder();
            foreach (var token in WalkFromRoot())
            {
                sb.Append($"/{token.Token}");
            }

            return sb.ToString();
        }

        IEnumerable<JsonPath> WalkFromRoot()
        {
            var parents = new Stack<JsonPath>();
            parents.Push(this);
            var current = this;
            while (current.Parent != null)
            {
                parents.Push(current.Parent);
                current = current.Parent;
            }

            while (parents.Count > 0)
            {
                var item = parents.Pop();
                yield return item;
            }
        }

        bool Equals(JsonPath other)
        {
            var current = this;
            var otherCurrent = other;

            while (current != null || otherCurrent != null)
            {
                if (current == null || otherCurrent == null)
                {
                    return false;
                }

                var same = current._stringValue == otherCurrent._stringValue && current._intValue == otherCurrent._intValue;
                if (!same)
                {
                    return false;
                }

                current = current._parent;
                otherCurrent = otherCurrent._parent;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JsonPath) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            var current = this;
            while (current != null)
            {
                hashCode.Add(current._stringValue);
                hashCode.Add(current._intValue);
                current = current._parent;
            }

            return hashCode.ToHashCode();
        }

        public string GetPropertyName()
        {
            if (string.IsNullOrEmpty(_stringValue))
            {
                throw new InvalidOperationException("JsonPath leaf is not a property of an object");
            }

            return _stringValue;
        }

        public int GetArrayIndex()
        {
            if (!string.IsNullOrEmpty(_stringValue))
            {
                throw new InvalidOperationException("JsonPath leaf is not a property of an array element");
            }

            return _intValue;
        }
    }
}