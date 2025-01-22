#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Commanding;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(ICommandHandler))]
    [ContentType("text")]
    [Name(nameof(RollbackChangeCommandHandler))]
    internal class RollbackChangeCommandHandler : PerforceDiffMarginCommandHandler<RollbackChangeCommandArgs>
    {
        public RollbackChangeCommandHandler()
            : base(PerforceDiffMarginCommand.RollbackChange)
        {
        }

        public override string DisplayName => "Rollback Change";
    }
}

#endif