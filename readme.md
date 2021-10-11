# JsonDiff.UTF8

[JSON patch](https://datatracker.ietf.org/doc/html/rfc6902) generation and application built on top of [Json Document](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument?view=net-5.0).

This library was created as as an alternative to the more complete [jsondiffpatch.net](https://github.com/wbish/jsondiffpatch.net) built on .NET 5 native JSON parsing, rather than [JSON.Net](https://www.newtonsoft.com/json).  `System.Text.Json` provides considerable performance improvements over JSON.Net.

# Usage

## Diff - from text:
```
var left = @"{ ""key"": false }";
var right = @"{ ""key"": true }";

PatchList path = JsonComparer.Compare(left.AsMemory(), right.AsMemory());
```

**Note** `JsonComparer` compares the memory first, and only if not equal will it parse and walk the JSON document looking for differences.

## Diff - given JsonDocument already parsed

Generate a patch list given two JSON documents:

```
var left = JsonDocument.Parse(@"{ ""key"": false }");
var right = JsonDocument.Parse(@"{ ""key"": true }");

PatchList path = left.CompareWith(right);
```

## Patch

Apply a given patch list to a json document:

```
var left = JsonDocument.Parse(@"{ ""key"": false }");
var right = JsonDocument.Parse(@"{ ""key"": true }");

var memoryStream = new MemoryStream();
var jsonWriter = new Utf8JsonWriter(memoryStream);
left.CompareWith(right)
    .ApplyPatch(left, jsonWriter);
```

# Benchmarks

This libaries usage of .NET 5 `System.Text.Json` primites offer a performance improvement over the [jsondiffpatch.net](https://github.com/wbish/jsondiffpatch.net) implementation:

```
|                                          Method |             Categories |        Mean |      Error |    StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 |   Allocated |
|------------------------------------------------ |----------------------- |------------:|-----------:|----------:|------:|--------:|---------:|---------:|------------:|
|                         UTF8Diff_HasDifferences |                   Diff |    94.49 us |   1.889 us |  1.855 us |  0.02 |    0.00 |  13.6719 |   1.0986 |    85,800 B |
|                    JsonPatchDiff_HasDifferences |                   Diff | 5,447.68 us |  78.325 us | 73.265 us |  1.00 |    0.00 | 250.0000 |  62.5000 | 1,607,548 B |
|                                                 |                        |             |            |           |       |         |          |          |             |
|                   UTF8Diff_Parse_HasDifferences |           Parse + Diff |   191.31 us |   2.159 us |  2.019 us |  0.03 |    0.00 |  20.0195 |   3.6621 |   127,016 B |
|              JsonPatchDiff_Parse_HasDifferences |           Parse + Diff | 5,715.38 us | 105.890 us | 93.869 us |  1.00 |    0.00 | 281.2500 | 109.3750 | 1,807,396 B |
|                                                 |                        |             |            |           |       |         |          |          |             |
|                             UTF8Diff_ApplyPatch |            Apply Patch |   130.92 us |   2.566 us |  2.746 us |  0.20 |    0.01 |  12.4512 |   1.2207 |    79,608 B |
|                        JsonPatchDiff_ApplyPatch |            Apply Patch |   644.88 us |  12.762 us | 20.242 us |  1.00 |    0.00 |  77.1484 |  18.5547 |   488,969 B |
|                                                 |                        |             |            |           |       |         |          |          |             |
|                          UTF8Diff_NoDifferences |         No differences |    75.68 us |   1.384 us |  1.699 us |  6.25 |    0.24 |  11.1084 |   0.1221 |    70,080 B |
|                     JsonPatchDiff_NoDifferences |         No differences |    12.09 us |   0.239 us |  0.327 us |  1.00 |    0.00 |   0.4883 |        - |     3,072 B |
|                                                 |                        |             |            |           |       |         |          |          |             |
|      IncludesJsonParsing_UTF8Diff_NoDifferences | Parse + no differences |    21.67 us |   0.428 us |  0.440 us |  0.11 |    0.00 |   0.0305 |        - |       272 B |
| IncludesJsonParsing_JsonPatchDiff_NoDifferences | Parse + no differences |   194.36 us |   3.612 us |  4.300 us |  1.00 |    0.00 |  27.5879 |   9.0332 |   173,936 B |
```