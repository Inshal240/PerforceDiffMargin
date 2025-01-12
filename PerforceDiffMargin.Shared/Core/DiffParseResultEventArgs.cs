using System;
using System.Collections.Generic;
using PerforceDiffMargin.Perforce;
using Microsoft.VisualStudio.Text;

namespace PerforceDiffMargin.Core
{
    public class DiffParseResultEventArgs : ParseResultEventArgs
    {
        private readonly List<HunkRangeInfo> _diff;

        public DiffParseResultEventArgs(ITextSnapshot snapshot, TimeSpan elapsedTime, List<HunkRangeInfo> diff)
            : base(snapshot, elapsedTime)
        {
            _diff = diff;
        }

        public IEnumerable<HunkRangeInfo> Diff
        {
            get
            {
                return _diff;
            }
        }
    }
}