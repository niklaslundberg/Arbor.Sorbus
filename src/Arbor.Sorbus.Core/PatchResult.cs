using System.Collections;
using System.Collections.Generic;

namespace Arbor.Sorbus.Core
{
    public sealed class PatchResult : IReadOnlyCollection<AssemblyInfoPatchResult>
    {
        readonly List<AssemblyInfoPatchResult> _patchResults = new List<AssemblyInfoPatchResult>();

        public IEnumerator<AssemblyInfoPatchResult> GetEnumerator()
        {
            return _patchResults.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _patchResults.Count; }
        }

        public void Add(AssemblyInfoPatchResult assemblyInfoPatchResult)
        {
            _patchResults.Add(assemblyInfoPatchResult);
        }
    }
}