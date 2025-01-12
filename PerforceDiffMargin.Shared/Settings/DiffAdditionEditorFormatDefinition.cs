using System.ComponentModel.Composition;
using System.Windows.Media;
using PerforceDiffMargin.Perforce;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace PerforceDiffMargin.Settings
{
    [Export(typeof(EditorFormatDefinition))]
    [Name(DiffFormatNames.Addition)]
    [UserVisible(true)]
    internal sealed class DiffAdditionEditorFormatDefinition : EditorFormatDefinition
    {
        public DiffAdditionEditorFormatDefinition()
        {
            BackgroundColor = Color.FromRgb(180, 255, 180);
            ForegroundCustomizable = false;
            DisplayName = "Perforce Diff Addition";
        }
    }
}