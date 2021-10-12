using System.Collections.Generic;
using System.Text.Json;
using DiffMatchPatch;

namespace JsonDiff.UTF8.JsonPatch
{
    public abstract record Operation (JsonPath Path, string OperationName)
    { }

    public record Add (JsonPath Path, JsonElement Value) : Operation(Path, "Add")
    { }
    
    public record Replace (JsonPath Path, JsonElement Value) : Operation(Path, "Replace")
    { }

    public record PatchText(JsonPath Path, List<Patch> Patches) : Operation(Path, "Replace")
    { }

    public record Remove(JsonPath Path) : Operation(Path, "Remove")
    { }
}