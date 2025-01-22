#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(NextChangeCommandHandler))]
    internal class NextChangeCommandHandler : PerforceDiffMarginCommandHandler<NextChangeCommandArgs>
    {
        public NextChangeCommandHandler()
            : base(PerforceDiffMarginCommand.NextChange)
        {
        }

        public override string DisplayName => "Next Change";
    }
}

#endif