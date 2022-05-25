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

        private Command GetFormatCommand()
        {
            var commands = IdeApp.CommandService.GetCommands();
            //commands.ToList().ForEach(c =>
            //{
            //    if (!string.IsNullOrEmpty(c.DisplayName))
            //    {
            //        Debug.WriteLine($"{c.DisplayName} {c.AccelKey}-{c.AlternateAccelKeys}");
            //    }
            //});

            return commands.FirstOrDefault(cmd => cmd.DisplayName == "Format Document");
        }

        private Command GetRemoveUnusedAndSortCommand()
        {
            var commands = IdeApp.CommandService.GetCommands();
            return commands.FirstOrDefault(cmd => cmd.DisplayName == "Remove Unused and Sort (Usings)");
        }

        private bool _skipDocumentSaved;

        private void DocumentsListener_DocumentSaved(object sender, EventArgs e)
        {
            if (_skipDocumentSaved)
                return;

            if (Settings.AutoFormatOnSave)
            {
                var savedDocument = sender as Document;
                //Runtime.RunInMainThread(
                //    () => IdeApp.Workbench.StatusBar.ShowMessage($"Formatting and fixing usings ({savedDocument.Name})..."));

                var activeDoc = IdeApp.Workbench.ActiveDocument;

                if (IdeApp.Workbench.ActiveDocument != savedDocument)
                {
                    IdeApp.Workbench.OpenDocument(savedDocument.FilePath, project: null);
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

                _skipDocumentSaved = true;
                savedDocument.Save();
                _skipDocumentSaved = false;

                if (IdeApp.Workbench.ActiveDocument != activeDoc)
                {
                    IdeApp.Workbench.OpenDocument(activeDoc.FilePath, project: null);
                }
                //ResetStatusBar();
            }
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