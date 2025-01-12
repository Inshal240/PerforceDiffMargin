namespace PerforceDiffMargin
{
    using System.Runtime.InteropServices;

    [Guid(PerforceDiffMarginCommandHandler.PerforceDiffMarginCommandSet)]
    public enum PerforceDiffMarginCommand
    {
        PreviousChange = 0,
        NextChange = 1,
        RollbackChange = 2,
        ShowDiff = 3,
        CopyOldText = 4,
        ShowPopup = 5,

        PerforceDiffToolbar = 100,

        PerforceDiffToolbarGroup = 150,
    }
}