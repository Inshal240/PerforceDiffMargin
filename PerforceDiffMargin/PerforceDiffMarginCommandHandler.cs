namespace PerforceDiffMargin
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using CommunityToolkit.Mvvm.Input;
    using PerforceDiffMargin.ViewModel;
    using Microsoft.VisualStudio.Editor;
    using Microsoft.VisualStudio.Text.Editor;
    using IVsTextView = Microsoft.VisualStudio.TextManager.Interop.IVsTextView;
    using OLECMDEXECOPT = Microsoft.VisualStudio.OLE.Interop.OLECMDEXECOPT;
    using OLECMDF = Microsoft.VisualStudio.OLE.Interop.OLECMDF;

    internal sealed class PerforceDiffMarginCommandHandler : TextViewCommandFilter
    {
        internal const string PerforceDiffMarginCommandSet = "691DB887-6D82-46A9-B0AF-407C7F0E39BE";
        internal const string PerforceDiffMarginStaticToolbarCommandSet = "6DA7066F-F38D-44D5-A52B-ADC48D274176";

        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ITextView _textView;

        public PerforceDiffMarginCommandHandler(IVsTextView textViewAdapter, IVsEditorAdaptersFactoryService editorAdaptersFactoryService, ITextView textView)
            : base(textViewAdapter)
        {
            if (editorAdaptersFactoryService == null)
                throw new ArgumentNullException("editorAdaptersFactoryService");
            if (textView == null)
                throw new ArgumentNullException("textView");

            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _textView = textView;
        }

        protected override OLECMDF QueryCommandStatus(ref Guid commandGroup, uint commandId, OleCommandText oleCommandText)
        {
            if (commandGroup == typeof(PerforceDiffMarginCommand).GUID)
            {
                switch ((PerforceDiffMarginCommand)commandId)
                {
                case PerforceDiffMarginCommand.ShowPopup:
                    {
                    EditorDiffMarginViewModel viewModel;
                    if (!TryGetMarginViewModel(out viewModel))
                        return 0;
                    
                    var diffViewModel = GetCurrentDiffViewModel(viewModel);

                    if (diffViewModel != null)
                        return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                    else
                        return OLECMDF.OLECMDF_SUPPORTED;
                }
                case PerforceDiffMarginCommand.PreviousChange:
                case PerforceDiffMarginCommand.NextChange:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        // First look for a diff already showing a popup
                        EditorDiffViewModel diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            RelayCommand<DiffViewModel> command = (PerforceDiffMarginCommand)commandId == PerforceDiffMarginCommand.NextChange ? viewModel.NextChangeCommand : viewModel.PreviousChangeCommand;
                            if (command.CanExecute(diffViewModel))
                                return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                            else
                                return OLECMDF.OLECMDF_SUPPORTED;
                        }

                        diffViewModel = GetDiffViewModelToMoveTo(commandId, viewModel);

                        if (diffViewModel != null)
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        else
                            return OLECMDF.OLECMDF_SUPPORTED;
                    }

                case PerforceDiffMarginCommand.RollbackChange:
                case PerforceDiffMarginCommand.CopyOldText:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        EditorDiffViewModel diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);
                        if (diffViewModel != null)
                        {
                            ICommand command = (PerforceDiffMarginCommand)commandId == PerforceDiffMarginCommand.RollbackChange ? diffViewModel.RollbackCommand : diffViewModel.CopyOldTextCommand;
                            if (command.CanExecute(diffViewModel))
                                return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        }

                        // This command only works when a popup is open
                        return OLECMDF.OLECMDF_SUPPORTED;
                    }

                case PerforceDiffMarginCommand.ShowDiff:
                    {
                        EditorDiffMarginViewModel viewModel;
                        if (!TryGetMarginViewModel(out viewModel))
                            return 0;

                        if (viewModel.DiffViewModels.Any())
                            return OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED;
                        else
                            return OLECMDF.OLECMDF_SUPPORTED;
                    }
                case PerforceDiffMarginCommand.PerforceDiffToolbar:
                case PerforceDiffMarginCommand.PerforceDiffToolbarGroup:
                    // these aren't actually commands, but IDs of the command bars and groups
                    break;

                default:
                    break;
                }
            }

            return 0;
        }

        protected override bool HandlePreExec(ref Guid commandGroup, uint commandId, OLECMDEXECOPT executionOptions, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (commandGroup == typeof(PerforceDiffMarginCommand).GUID)
            {
                EditorDiffMarginViewModel viewModel = null;
                EditorDiffViewModel diffViewModel = null;
                if (TryGetMarginViewModel(out viewModel))
                    diffViewModel = viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(i => i.ShowPopup);

                switch ((PerforceDiffMarginCommand)commandId)
                {
                case PerforceDiffMarginCommand.ShowPopup:
                {                  
                    diffViewModel = GetCurrentDiffViewModel(viewModel);

                    if (diffViewModel != null)
                    {
                        diffViewModel.ShowPopup = true;
                        return true;
                    }

                    return false;
                }
                case PerforceDiffMarginCommand.PreviousChange:
                case PerforceDiffMarginCommand.NextChange:
                    {
                        if (viewModel == null)
                            return false;

                        RelayCommand<DiffViewModel> command = (PerforceDiffMarginCommand)commandId == PerforceDiffMarginCommand.NextChange ? viewModel.NextChangeCommand : viewModel.PreviousChangeCommand;

                        // First look for a diff already showing a popup
                        if (diffViewModel != null)
                        {
                            command.Execute(diffViewModel);
                            return true;
                        }

                        diffViewModel = GetDiffViewModelToMoveTo(commandId, viewModel);

                        if (diffViewModel == null) return false;

                        viewModel.MoveToChange(diffViewModel, 0);
                        return true;
                    }

                case PerforceDiffMarginCommand.RollbackChange:
                case PerforceDiffMarginCommand.CopyOldText:
                    {
                        if (diffViewModel == null)
                            return false;

                        ICommand command = (PerforceDiffMarginCommand)commandId == PerforceDiffMarginCommand.RollbackChange ? diffViewModel.RollbackCommand : diffViewModel.CopyOldTextCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                case PerforceDiffMarginCommand.ShowDiff:
                    {
                        if (diffViewModel == null)
                            return false;

                        ICommand command = diffViewModel.ShowDifferenceCommand;
                        command.Execute(diffViewModel);
                        return true;
                    }

                case PerforceDiffMarginCommand.PerforceDiffToolbar:
                case PerforceDiffMarginCommand.PerforceDiffToolbarGroup:
                    // these aren't actually commands, but IDs of the command bars and groups
                    break;

                default:
                    break;
                }
            }

            return false;
        }

        private EditorDiffViewModel GetDiffViewModelToMoveTo(uint commandId, DiffMarginViewModelBase viewModel)
        {
            var lineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

            return (PerforceDiffMarginCommand) commandId == PerforceDiffMarginCommand.NextChange ?
                viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(model => model.LineNumber > lineNumber) :
                viewModel.DiffViewModels.OfType<EditorDiffViewModel>().LastOrDefault(model => model.LineNumber < lineNumber);
        }

        private EditorDiffViewModel GetCurrentDiffViewModel(DiffMarginViewModelBase viewModel)
        {
            var caretLineNumber = _textView.Caret.Position.BufferPosition.GetContainingLine().LineNumber;

            return viewModel.DiffViewModels.OfType<EditorDiffViewModel>().FirstOrDefault(diff => diff.IsLineNumberBetweenDiff(caretLineNumber));
        }

        private bool TryGetMarginViewModel(out EditorDiffMarginViewModel viewModel)
        {
            viewModel = null;

            IWpfTextViewHost textViewHost = _editorAdaptersFactoryService.GetWpfTextViewHost(TextViewAdapter);
            if (textViewHost == null)
                return false;

            EditorDiffMargin margin = textViewHost.GetTextViewMargin(EditorDiffMargin.MarginNameConst) as EditorDiffMargin;
            if (margin == null)
                return false;

            viewModel = margin.VisualElement.DataContext as EditorDiffMarginViewModel;
            return viewModel != null;
        }
    }
}
