using System.IO;
using BenchmarkDotNet.Attributes;

namespace JsonDiff.UTF8.Benchmarks
{
    [MinColumn]
    [MaxColumn]
    [MemoryDiagnoser]
    public class Benchmark
    {
        readonly IDiffGenerator _utf8DiffGenerator;
        readonly IDiffGenerator _jsonDiffGenerator;
        readonly IDiffGenerator _utf8DiffGeneratorNoDifferences;
        readonly IDiffGenerator _jsonDiffGeneratorNoDifferences;

        public Benchmark()
        {
            var baseJson = File.ReadAllText("different.base.json");
            var otherJson = File.ReadAllText("different.other.json");

            _utf8DiffGenerator = new Utf8DiffGenerator();
            _utf8DiffGenerator.Setup(baseJson, otherJson);
            
            _jsonDiffGenerator = new JsonDiffPatchDiffGenerator();
            _jsonDiffGenerator.Setup(baseJson, otherJson);

            _jsonDiffGeneratorNoDifferences = new JsonDiffPatchDiffGenerator();
            _jsonDiffGeneratorNoDifferences.Setup(baseJson, baseJson);
            
            _utf8DiffGeneratorNoDifferences = new Utf8DiffGenerator();
            _utf8DiffGeneratorNoDifferences.Setup(baseJson, baseJson);
            
        }

        [Benchmark]
        public void UTF8Diff_HasDifferences()
        {
            _utf8DiffGenerator.PerformDiff();
        }

        [Benchmark]
        public void JsonPatchDiff_HasDifferences()
        {
            _jsonDiffGenerator.PerformDiff();
        }
        
        [Benchmark]
        public void UTF8Diff_NoDifferences()
        {
            _utf8DiffGeneratorNoDifferences.PerformDiff();
        }

        [Benchmark]
        public void JsonPatchDiff_NoDifferences()
        {
            _jsonDiffGeneratorNoDifferences.PerformDiff();
        }
    }
}