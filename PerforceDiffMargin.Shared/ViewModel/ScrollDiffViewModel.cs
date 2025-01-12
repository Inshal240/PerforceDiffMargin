using System;
using PerforceDiffMargin.Core;
using PerforceDiffMargin.Perforce;

namespace PerforceDiffMargin.ViewModel
{
    internal sealed class ScrollDiffViewModel : DiffViewModel
    {
        internal ScrollDiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
            : base(hunkRangeInfo, marginCore, updateDiffDimensions)
        {
            UpdateDimensions();
        }

        public override bool IsVisible
        {
            get { return true; }
            set { }
        }

        public override double Width
        {
            get
            {
                return MarginCore.ScrollChangeWidth;
            }
        }

    }
}