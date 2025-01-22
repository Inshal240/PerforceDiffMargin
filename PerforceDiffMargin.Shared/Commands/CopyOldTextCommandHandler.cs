#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(CopyOldTextCommandHandler))]
    internal class CopyOldTextCommandHandler : PerforceDiffMarginCommandHandler<CopyOldTextCommandArgs>
    {
        public CopyOldTextCommandHandler()
            : base(PerforceDiffMarginCommand.CopyOldText)
        {
        }

        public override string DisplayName => "Copy Old Text";
    }
}

#endif