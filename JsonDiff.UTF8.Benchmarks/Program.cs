using System;
using BenchmarkDotNet.Running;

namespace JsonDiff.UTF8.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            // Benchmark json made here: https://www.json-generator.com/
            if (args.Length == 0)
            {
                BenchmarkRunner.Run<Benchmark>();
                return;
            }

            var testName = args[0];
            var testMethod = typeof(Benchmark).GetMethod(testName);
            if (testMethod == null) throw new ArgumentException($"{testName} couldn't be found");
            testMethod.Invoke(new Benchmark(), Array.Empty<object>());
        }
    }
}