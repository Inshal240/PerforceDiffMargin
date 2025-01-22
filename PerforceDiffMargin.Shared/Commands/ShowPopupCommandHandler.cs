#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(ShowPopupCommandHandler))]
    internal class ShowPopupCommandHandler : PerforceDiffMarginCommandHandler<ShowPopupCommandArgs>
    {
        public ShowPopupCommandHandler()
            : base(PerforceDiffMarginCommand.ShowPopup)
        {
        }

        public override string DisplayName => "Show Popup";
    }
}

#endif