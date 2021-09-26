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

        public Benchmark()
        {
            var baseJson = File.ReadAllText("different.base.json");
            var otherJson = File.ReadAllText("different.other.json");

            _utf8DiffGenerator = new Utf8DiffGenerator();
            _jsonDiffGenerator = new JsonDiffPatchDiffGenerator();
            
            _utf8DiffGenerator.Setup(baseJson, otherJson);
            _jsonDiffGenerator.Setup(baseJson, otherJson);
        }

        [Benchmark]
        public void UTF8Diff()
        {
            _utf8DiffGenerator.PerformDiff();
        }

        [Benchmark(Baseline = true)]
        public void JsonPatchDiff()
        {
            _jsonDiffGenerator.PerformDiff();
        }
    }
}