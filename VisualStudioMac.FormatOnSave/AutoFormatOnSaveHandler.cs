using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Threading;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.AutoFormatOnSave
{
    public class AutoFormatOnSaveHandler
    {
        private static AutoFormatOnSaveHandler _instance;
        public static AutoFormatOnSaveHandler Instance => _instance ?? (_instance = new AutoFormatOnSaveHandler());

        DocumentsListener _documentsListener = new DocumentsListener();

        public void Startup()
        {
            IdeApp.Workspace.SolutionLoaded += Workspace_SolutionLoaded;
            IdeApp.Workspace.SolutionUnloaded += Workspace_SolutionUnloaded;
        }

        private void Workspace_SolutionLoaded(object sender, Projects.SolutionEventArgs e)
        {
            _documentsListener.StartListening();
            _documentsListener.DocumentSaved += DocumentsListener_DocumentSaved;
        }

        private void Workspace_SolutionUnloaded(object sender, Projects.SolutionEventArgs e)
        {
            _documentsListener.DocumentSaved -= DocumentsListener_DocumentSaved;
            _documentsListener.StopListening();
        }

        private Command _formatCommand;

        private Command GetFormatCommand()
        {
            if (_formatCommand is null)
            {
                var commands = IdeApp.CommandService.GetCommands();
                //commands.ToList().ForEach(c =>
                //{
                //    if (!string.IsNullOrEmpty(c.DisplayName))
                //    {
                //        Debug.WriteLine($"{c.DisplayName} {c.AccelKey}-{c.AlternateAccelKeys}");
                //    }
                //});
                _formatCommand = commands.FirstOrDefault(cmd => cmd.DisplayName == "Format Document");
            }
            return _formatCommand;
        }

        private Command _removeUnusedAndSortCommand;

        private Command GetRemoveUnusedAndSortCommand()
        {
            if (_removeUnusedAndSortCommand is null)
            {
                var commands = IdeApp.CommandService.GetCommands();
                _removeUnusedAndSortCommand = commands.FirstOrDefault(cmd => cmd.DisplayName == "Remove Unused and Sort (Usings)");
            }
            return _removeUnusedAndSortCommand;
        }

        private async void DocumentsListener_DocumentSaved(object sender, EventArgs e)
        {
            if (Settings.AutoFormatOnSave)
            {
                await FormatDocument(sender as Document);
            }
        }

        private bool _skipDocumentSaved;
        private async Task FormatDocument(Document document)
        {
            if (document is null)
                return;

            if (_skipDocumentSaved)
                return;

            await Runtime.RunInMainThread(() => IdeApp.Workbench.StatusBar.ShowMessage($"Formatting and fixing usings ({document.Name})..."));

            var activeDoc = IdeApp.Workbench.ActiveDocument;

            if (IdeApp.Workbench.ActiveDocument != document)
            {
                await IdeApp.Workbench.OpenDocument(document.FilePath, project: null);
            }

            var cmd = GetFormatCommand();
            if (cmd != null)
            {
                IdeApp.CommandService.DispatchCommand(cmd.Id);
            }

            cmd = GetRemoveUnusedAndSortCommand();
            if (cmd != null)
            {
                IdeApp.CommandService.DispatchCommand(cmd.Id);
            }

            var bo = IdeApp.ProjectOperations.CurrentBuildOperation;
            if (bo is null || bo.IsCompleted)
            {
                _skipDocumentSaved = true;
                await document.Save();
                _skipDocumentSaved = false;
            }

            if (IdeApp.Workbench.ActiveDocument != activeDoc)
            {
                await IdeApp.Workbench.OpenDocument(activeDoc.FilePath, project: null);
            }
            ResetStatusBar();
        }

        private CancellationTokenSource cts;
        private void ResetStatusBar()
        {
            if (cts != null)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();
            Task.Delay(3000)
                .ContinueWith((x) =>
                {
                    if (!x.IsCanceled)
                    {
                        Runtime.RunInMainThread(() => IdeApp.Workbench.StatusBar.ShowReady());
                    }
                })
                .WithCancellation(cts.Token);
        }
    }
}