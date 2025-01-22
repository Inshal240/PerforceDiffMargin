#if !LEGACY_COMMANDS

namespace PerforceDiffMargin.Commands
{
    using System;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Text.Editor.Commanding;

    internal abstract class PerforceDiffMarginCommandHandler<T> : ShimCommandHandler<T>
        where T : EditorCommandArgs
    {
        protected PerforceDiffMarginCommandHandler(PerforceDiffMarginCommand commandId)
            : base(new Guid(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet), (uint)commandId)
        {
        }

        protected override IOleCommandTarget GetCommandTarget(T args)
            => args.TextView.Properties.GetProperty<PerforceDiffMarginCommandHandler>(typeof(PerforceDiffMarginCommandHandler));
    }
}

#endif