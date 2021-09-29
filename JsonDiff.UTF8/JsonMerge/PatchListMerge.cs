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
                    if (!CanIgnoreConflict(patch, otherOperation))
                    {
                        mergeResult.AddConflict(patch, otherOperation);
                    }
                    continue;
                }

                patchList.Add(patch);
            }

            return mergeResult;
        }

        static bool CanIgnoreConflict(Operation patch, Operation otherOperation)
        {
            if (patch.Path.Equals(otherOperation.Path) && patch is Remove && otherOperation is Remove)
            {
                return true;
            }

            return false;
        }
    }
}