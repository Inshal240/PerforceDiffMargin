using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using PerforceDiffMargin.Core;
using PerforceDiffMargin.Perforce;

namespace PerforceDiffMargin.ViewModel
{
    internal abstract class DiffMarginViewModelBase : ObservableRecipient
    {
        protected readonly IMarginCore MarginCore;

        protected DiffMarginViewModelBase(IMarginCore marginCore)
        {
            if (marginCore == null)
                throw new ArgumentNullException("marginCore");

            MarginCore = marginCore;

            DiffViewModels = new ObservableCollection<DiffViewModel>();

            MarginCore.HunksChanged += HandleHunksChanged;
        }

        public ObservableCollection<DiffViewModel> DiffViewModels { get; private set; }

        public void RefreshDiffViewModelPositions()
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.RefreshPosition();
            }
        }

        protected virtual void HandleHunksChanged(object sender, HunksChangedEventArgs e)
        {
            foreach (var diffViewModel in DiffViewModels)
            {
                diffViewModel.IsActive = false;
            }
            DiffViewModels.Clear();

            foreach (var diffViewModel in e.Hunks.Select(CreateDiffViewModel))
            {
                DiffViewModels.Add(diffViewModel);
            }
        }

        protected abstract DiffViewModel CreateDiffViewModel(HunkRangeInfo hunkRangeInfo);

        protected override void OnDeactivated()
        {
            MarginCore.HunksChanged -= HandleHunksChanged;

            base.OnDeactivated();
        }
    }
}