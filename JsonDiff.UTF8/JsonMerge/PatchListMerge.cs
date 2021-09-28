using System.Text.Json;
using JsonDiff.UTF8.JsonPatch;

namespace JsonDiff.UTF8.JsonMerge
{
    public static class PatchListMerge
    {
        public static MergeResult ThreeWayMerge(this JsonDocument @base, JsonDocument left, JsonDocument right)
        {
            var leftPatch = @base.CompareWith(left);
            var rightPatch = @base.CompareWith(right);

            return leftPatch.TryMerge(rightPatch);
        }
        
        public static MergeResult TryMerge(this PatchList left, PatchList right)
        {
            var patchList = new PatchList(left);
            var mergeResult = new MergeResult(patchList);

            foreach (var patch in right)
            {
                if (left.TryGetExistingPatch(patch.Path, out var otherOperation))
                {
                    mergeResult.AddConflict(patch, otherOperation);
                }
                else
                {
                    patchList.Add(patch);
                }
            }

            return mergeResult;
        }
    }
}