#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Editor.Commanding;

    internal class PerforceDiffMarginCommandBinding
    {
#pragma warning disable CS0649 // Field 'fieldName' is never assigned to, and will always have its default value null

        [Export]
        [CommandBinding(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet, (uint)PerforceDiffMarginCommand.PreviousChange, typeof(PreviousChangeCommandArgs))]
        internal CommandBindingDefinition PreviousChangeCommandBinding;

        [Export]
        [CommandBinding(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet, (uint)PerforceDiffMarginCommand.NextChange, typeof(NextChangeCommandArgs))]
        internal CommandBindingDefinition NextChangeCommandBinding;

        [Export]
        [CommandBinding(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet, (uint)PerforceDiffMarginCommand.RollbackChange, typeof(RollbackChangeCommandArgs))]
        internal CommandBindingDefinition RollbackChangeCommandBinding;

        [Export]
        [CommandBinding(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet, (uint)PerforceDiffMarginCommand.CopyOldText, typeof(CopyOldTextCommandArgs))]
        internal CommandBindingDefinition CopyOldTextCommandBinding;

        [Export]
        [CommandBinding(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet, (uint)PerforceDiffMarginCommand.ShowPopup, typeof(ShowPopupCommandArgs))]
        internal CommandBindingDefinition ShowPopupCommandBinding;

#pragma warning restore CS0649 // Field 'fieldName' is never assigned to, and will always have its default value null
    }
}

#endif