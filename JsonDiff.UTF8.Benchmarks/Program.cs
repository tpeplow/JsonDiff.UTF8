using BenchmarkDotNet.Running;

namespace JsonDiff.UTF8.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Benchmark>();
        }
    }
}