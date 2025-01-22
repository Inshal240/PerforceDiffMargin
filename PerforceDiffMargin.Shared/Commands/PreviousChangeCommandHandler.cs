#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(PreviousChangeCommandHandler))]
    internal class PreviousChangeCommandHandler : PerforceDiffMarginCommandHandler<PreviousChangeCommandArgs>
    {
        public PreviousChangeCommandHandler()
            : base(PerforceDiffMarginCommand.PreviousChange)
        {
        }

        public override string DisplayName => "Previous Change";
    }
}

#endif