using System.Collections.Generic;
using System.Text.Json;

namespace JsonDiff.UTF8.JsonPatch
{
    /// <summary>
    /// Based on https://datatracker.ietf.org/doc/html/rfc6902
    /// </summary>
    public class PatchList : List<Operation>
    {
        public void WriteTo(Utf8JsonWriter writer)
        {
            
        }
    }
}