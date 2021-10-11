using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace JsonDiff.UTF8.Benchmarks
{
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    [MemoryDiagnoser, CategoriesColumn]
    public class Benchmark
    {
        const string ApplyPatch = "Apply Patch";
        const string Diff = "Diff";
        const string NoDifferences = "No differences";
        const string NoDifferencesParseJson = "Parse + no differences";
        
        readonly IDiffGenerator _utf8DiffGenerator;
        readonly IDiffGenerator _jsonDiffGenerator;
        readonly IDiffGenerator _utf8DiffGeneratorNoDifferences;
        readonly IDiffGenerator _jsonDiffGeneratorNoDifferences;
        readonly string _baseJson;
        readonly string _otherJson;

        public Benchmark()
        {
            _baseJson = File.ReadAllText("different.base.json");
            _otherJson = File.ReadAllText("different.other.json");

            _utf8DiffGenerator = new Utf8DiffGenerator();
            _utf8DiffGenerator.Setup(_baseJson, _otherJson);
            
            _jsonDiffGenerator = new JsonDiffPatchDiffGenerator();
            _jsonDiffGenerator.Setup(_baseJson, _otherJson);

            _jsonDiffGeneratorNoDifferences = new JsonDiffPatchDiffGenerator();
            _jsonDiffGeneratorNoDifferences.Setup(_baseJson, _baseJson);
            
            _utf8DiffGeneratorNoDifferences = new Utf8DiffGenerator();
            _utf8DiffGeneratorNoDifferences.Setup(_baseJson, _baseJson);
            
        }
        
        [BenchmarkCategory(Diff), Benchmark]
        public void UTF8Diff_HasDifferences()
        {
            _utf8DiffGenerator.PerformDiff();
        }

        [BenchmarkCategory(Diff), Benchmark(Baseline = true)]
        public void JsonPatchDiff_HasDifferences()
        {
            _jsonDiffGenerator.PerformDiff();
        }

        [BenchmarkCategory(ApplyPatch), Benchmark]
        public void UTF8Diff_ApplyPatch()
        {
            _utf8DiffGenerator.PerformPatch();
        }
        
        [BenchmarkCategory(ApplyPatch), Benchmark(Baseline = true)]
        public void JsonPatchDiff_ApplyPatch()
        {
            _jsonDiffGenerator.PerformPatch();
        }
        
        [BenchmarkCategory(NoDifferences), Benchmark]
        public void UTF8Diff_NoDifferences()
        {
            _utf8DiffGeneratorNoDifferences.PerformDiff();
        }

        [BenchmarkCategory(NoDifferences), Benchmark(Baseline = true)]
        public void JsonPatchDiff_NoDifferences()
        {
            _jsonDiffGeneratorNoDifferences.PerformDiff();
        }
        
        [BenchmarkCategory(NoDifferencesParseJson), Benchmark]
        public void IncludesJsonParsing_UTF8Diff_NoDifferences()
        {
            _utf8DiffGeneratorNoDifferences.Setup(_baseJson, _otherJson);
            _utf8DiffGeneratorNoDifferences.PerformDiff();
        }

        [BenchmarkCategory(NoDifferencesParseJson), Benchmark(Baseline = true)]
        public void IncludesJsonParsing_JsonPatchDiff_NoDifferences()
        {
            _jsonDiffGeneratorNoDifferences.Setup(_baseJson, _baseJson);
            _jsonDiffGeneratorNoDifferences.PerformDiff();
        }
    }
}