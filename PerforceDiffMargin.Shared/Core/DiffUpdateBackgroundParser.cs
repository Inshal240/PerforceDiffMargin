using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PerforceDiffMargin.Perforce;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;

namespace PerforceDiffMargin.Core
{
    public class DiffUpdateBackgroundParser : BackgroundParser
    {
        private readonly ITextDocument _textDocument;
        private readonly ITextBuffer _documentBuffer;

        internal DiffUpdateBackgroundParser(ITextBuffer textBuffer, ITextBuffer documentBuffer, TaskScheduler taskScheduler, ITextDocumentFactoryService textDocumentFactoryService)
            : base(textBuffer, taskScheduler, textDocumentFactoryService)
        {
            _documentBuffer = documentBuffer;
            ReparseDelay = TimeSpan.FromMilliseconds(500);

            if (TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out _textDocument))
            {
                if (PerforceCommands.GetInstance().IsDiffPerformed(_textDocument.FilePath))
                {
                    _textDocument.FileActionOccurred += OnFileActionOccurred;
                    // TODO: implement a mechanism that will monitor perforce submit and update diff after submit
                    // Probably class which will check if current changelist already submitted using p4 describe every second can work as workaround
                    // or "Custom Tool": https://stackoverflow.com/questions/16053503/perforce-client-side-pre-commit-hook
                }
            }
        }

        private void HandleFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            Action action =
                () =>
                {
                    try
                    {
                        ProcessFileSystemChange(e);
                    }
                    catch (Exception ex)
                    {
                        if (ErrorHandler.IsCriticalException(ex))
                            throw;
                    }
                };

            Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private void ProcessFileSystemChange(FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed && Directory.Exists(e.FullPath))
                return;

            if (string.Equals(Path.GetExtension(e.Name), ".lock", StringComparison.OrdinalIgnoreCase))
                return;

            MarkDirty(true);
        }

        private void OnFileActionOccurred(object sender, TextDocumentFileActionEventArgs e)
        {
            if ((e.FileActionType & FileActionTypes.ContentSavedToDisk) != 0)
            {
                MarkDirty(true);
            }
        }

        public override string Name
        {
            get
            {
                return "Perforce Diff Analyzer";
            }
        }

        protected override void ReParseImpl()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();

                var snapshot = TextBuffer.CurrentSnapshot;
                ITextDocument textDocument;
                if (!TextDocumentFactoryService.TryGetTextDocument(_documentBuffer, out textDocument)) return;

                var diff = PerforceCommands.GetInstance().GetDiffFor(textDocument, snapshot);
                var result = new DiffParseResultEventArgs(snapshot, stopwatch.Elapsed, diff.ToList());
                OnParseComplete(result);
            }
            catch (InvalidOperationException)
            {
                MarkDirty(true);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_textDocument != null)
                {
                    _textDocument.FileActionOccurred -= OnFileActionOccurred;
                }
            }
        }
    }
}