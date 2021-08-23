using System.Text.Json;

namespace JsonDiff.UTF8.JsonPatch
{
    public abstract record Operation (JsonPath Path)
    { }

    public record Add (JsonPath Path, JsonElement Value) : Operation(Path)
    { }
    
    public record Replace (JsonPath Path, JsonElement Value) : Operation(Path)
    { }

    public record Remove(JsonPath Path) : Operation(Path)
    { }
}