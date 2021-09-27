namespace JsonDiff.UTF8.Benchmarks
{
    public interface IDiffGenerator
    {
        void Setup(string baseJson, string otherJson);
        void PerformDiff();
        void PerformPatch();
    }
}