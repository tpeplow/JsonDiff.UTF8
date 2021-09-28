using System.Collections.Generic;
using JsonDiff.UTF8.JsonPatch;

namespace JsonDiff.UTF8.JsonMerge
{
    public class MergeResult
    {
        readonly List<(Operation, Operation)> _conflicts = new();

        public MergeResult(PatchList patchList)
        {
            PatchList = patchList;
        }

        internal void AddConflict(Operation left, Operation right)
        {
            _conflicts.Add((left, right));
        }

        public bool Success => _conflicts.Count == 0;
        
        public PatchList PatchList { get; }

        public IEnumerable<(Operation Left, Operation Right)> Conflicts => _conflicts;
    }
}