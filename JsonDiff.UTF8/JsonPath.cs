using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace JsonDiff.UTF8
{
    public class JsonPath
    {
        public static readonly JsonPath WholeDocument = new();
        
        readonly string _stringValue;
        readonly int _intValue;
        readonly JsonPath? _parent;

        JsonPath()
        {
            _stringValue = string.Empty;
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

        JsonPath? Parent
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

        string Token => _stringValue == string.Empty ? _intValue.ToString() : _stringValue;

        public JsonElement Evaluate(JsonDocument element)
        {
            var current = element.RootElement;
            foreach (var item in WalkFromRoot())
            {
                current = item._stringValue != string.Empty ? 
                    current.GetProperty(item._stringValue) 
                    : current[item._intValue];
            }

            return current;
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
    }
}