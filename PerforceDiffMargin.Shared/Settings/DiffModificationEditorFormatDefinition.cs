using System.ComponentModel.Composition;
using System.Windows.Media;
using PerforceDiffMargin.Perforce;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PerforceDiffMargin.Settings
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(DiffFormatNames.Modification)]
    [UserVisible(true)]
    internal sealed class DiffModificationEditorFormatDefinition : EditorFormatDefinition
    {
        public DiffModificationEditorFormatDefinition()
        {
            BackgroundColor = Color.FromRgb(160, 200, 255);
            ForegroundCustomizable = false;
            DisplayName = "Perforce Diff Modification";
        }
    }
}