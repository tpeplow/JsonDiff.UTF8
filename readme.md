# JsonDiff.UTF8

[JSON patch](https://datatracker.ietf.org/doc/html/rfc6902) generation and application built on top of [Json Document](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument?view=net-5.0).

This library was created as as an alternative to the more complete [jsondiffpatch.net](https://github.com/wbish/jsondiffpatch.net) built on .NET 5 native JSON parsing, rather than [JSON.Net](https://www.newtonsoft.com/json).  `System.Text.Json` provides considerable performance improvements over JSON.Net.

# Usage

## Diff

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
|                                          Method |             Categories |        Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |    Gen 1 | Allocated |
|------------------------------------------------ |----------------------- |------------:|-----------:|-----------:|------:|--------:|---------:|---------:|----------:|
|                         UTF8Diff_HasDifferences |                   Diff |    89.23 us |   0.872 us |   0.773 us |  0.02 |    0.00 |  13.6719 |   1.0986 |     84 KB |
|                    JsonPatchDiff_HasDifferences |                   Diff | 5,203.79 us | 103.153 us | 105.931 us |  1.00 |    0.00 | 250.0000 |  62.5000 |  1,570 KB |
|                                                 |                        |             |            |            |       |         |          |          |           |
|                             UTF8Diff_ApplyPatch |            Apply Patch |   125.23 us |   1.589 us |   1.486 us |  0.20 |    0.00 |  12.4512 |   1.2207 |     78 KB |
|                        JsonPatchDiff_ApplyPatch |            Apply Patch |   619.95 us |  10.119 us |   9.465 us |  1.00 |    0.00 |  77.1484 |  18.5547 |    478 KB |
|                                                 |                        |             |            |            |       |         |          |          |           |
|                          UTF8Diff_NoDifferences |         No differences |    74.07 us |   1.157 us |   1.082 us |  6.40 |    0.12 |  11.1084 |   0.1221 |     68 KB |
|                     JsonPatchDiff_NoDifferences |         No differences |    11.58 us |   0.139 us |   0.123 us |  1.00 |    0.00 |   0.4883 |        - |      3 KB |
|                                                 |                        |             |            |            |       |         |          |          |           |
|      IncludesJsonParsing_UTF8Diff_NoDifferences | Parse + no differences |   163.04 us |   1.922 us |   1.605 us |  0.03 |    0.00 |  20.0195 |   3.9063 |    124 KB |
| IncludesJsonParsing_JsonPatchDiff_NoDifferences | Parse + no differences | 5,808.24 us |  53.617 us |  47.530 us |  1.00 |    0.00 | 281.2500 | 109.3750 |  1,765 KB |
```