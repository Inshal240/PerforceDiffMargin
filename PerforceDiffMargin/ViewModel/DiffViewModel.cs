using System;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using PerforceDiffMargin.Core;
using PerforceDiffMargin.Perforce;

namespace PerforceDiffMargin.ViewModel
{
    internal abstract class DiffViewModel : ObservableRecipient
    {
        private double _height;
        private double _top;
        protected readonly HunkRangeInfo HunkRangeInfo;
        protected readonly IMarginCore MarginCore;
        private readonly Action<DiffViewModel, HunkRangeInfo> _updateDiffDimensions;
        private bool _isVisible;

        protected DiffViewModel(HunkRangeInfo hunkRangeInfo, IMarginCore marginCore, Action<DiffViewModel, HunkRangeInfo> updateDiffDimensions)
        {
            HunkRangeInfo = hunkRangeInfo;
            MarginCore = marginCore;
            _updateDiffDimensions = updateDiffDimensions;

            MarginCore.BrushesChanged += HandleBrushesChanged;
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if ( _height != value )
                    return;

                var oldValue = _height;
                _height = value;

                OnPropertyChanged(nameof(Height));
                Broadcast(oldValue, _height, nameof(Height));
            }
        }

        public double Top
        {
            get { return _top; }
            set
            {
                if ( _top != value )
                    return;

                var oldValue = _top;
                _top = value;

                OnPropertyChanged(nameof(Top));
                Broadcast(oldValue, _top, nameof(Top));
            }
        }

        public virtual double Width
        {
            get
            {
                return MarginCore.EditorChangeWidth;
            }
        }

        public Thickness Margin
        {
            get
            {
                return new Thickness(MarginCore.EditorChangeLeft, 0, 0, 0);
            }
        }

        public bool IsDeletion { get { return HunkRangeInfo.IsDeletion;} }

        public Brush DiffBrush
        {
            get
            {
                if (HunkRangeInfo.IsAddition)
                {
                    return MarginCore.AdditionBrush;
                }
                return HunkRangeInfo.IsModification ? MarginCore.ModificationBrush : MarginCore.RemovedBrush;
            }
        }

        public int LineNumber { get { return HunkRangeInfo.NewHunkRange.StartingLineNumber; } }

        public int NumberOfLines { get { return HunkRangeInfo.NewHunkRange.NumberOfLines; } }

        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if ( _isVisible != value )
                    return;

                var oldValue = _isVisible;
                _isVisible = value;

                OnPropertyChanged(nameof(IsVisible));
                Broadcast(oldValue, _isVisible, nameof(IsVisible));
            }
        }

        public double ScaleFactor => MarginCore.ScaleFactor;

        public void RefreshPosition()
        {
            UpdateDimensions();
        }

        public bool IsLineNumberBetweenDiff(int lineNumber)
        {
            var diffStartLine = LineNumber;
            var diffEndLine = diffStartLine + NumberOfLines - 1;

            if (IsDeletion)
            {
                diffEndLine = diffStartLine + NumberOfLines + 2;
            }

            return lineNumber >= diffStartLine && lineNumber <= diffEndLine;
        }

        protected override void OnDeactivated()
        {
            MarginCore.BrushesChanged -= HandleBrushesChanged;

            base.OnDeactivated();
        }

        private void HandleBrushesChanged(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(DiffBrush));
            Broadcast(DiffBrush, DiffBrush, nameof(DiffBrush));
        }

        protected virtual void UpdateDimensions()
        {
            _updateDiffDimensions(this, HunkRangeInfo);
        }
    }
}