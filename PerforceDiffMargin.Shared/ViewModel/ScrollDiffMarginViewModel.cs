using System;
using PerforceDiffMargin.Core;
using PerforceDiffMargin.Perforce;

namespace PerforceDiffMargin.ViewModel
{
    internal class ScrollDiffMarginViewModel : DiffMarginViewModelBase
    {
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;

        internal ScrollDiffMarginViewModel(IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions) :
            base(marginCore)
        {
            if (updateDiffDimensions == null)
                throw new ArgumentNullException("updateDiffDimensions");

            _updateDiffDimensions = updateDiffDimensions;
        }

        protected override DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo)
        {
            return new ScrollDiffViewModel(hunkRangeInfo, MarginCore, _updateDiffDimensions);
        }
    }
}