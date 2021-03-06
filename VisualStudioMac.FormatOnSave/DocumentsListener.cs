using System;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System;
using System.Linq;
namespace MonoDevelop.AutoFormatOnSave
{
    public class DocumentsListener
    {
        private Document[] _currentDocuments;

        public event EventHandler DocumentSaved;

        public bool IsListening { get; private set; }

        public DocumentsListener()
        {
        }

        public void StartListening()
        {
            if (IsListening)
            {
                LoggingService.LogDebug($"{nameof(DocumentsListener)} is already listening");
                return;
            }

            IsListening = true;

            IdeApp.Workbench.DocumentOpened += OnDocumentsChanged;
            IdeApp.Workbench.DocumentClosed += OnDocumentsChanged;
            
            _currentDocuments = IdeApp.Workbench.Documents.ToArray();
            SubscribeDocumentsSaved();
        }

        public void StopListening()
        {
            IdeApp.Workbench.DocumentOpened -= OnDocumentsChanged;
            IdeApp.Workbench.DocumentClosed -= OnDocumentsChanged;
            UnsubscribeDocumentsSaved();
            _currentDocuments = null;

            IsListening = false;
        }

        private void OnDocumentsChanged(object sender, EventArgs e)
        {
            UnsubscribeDocumentsSaved();

            _currentDocuments = IdeApp.Workbench.Documents.ToArray();

            SubscribeDocumentsSaved();
        }

        private void SubscribeDocumentsSaved()
        {
            var currentDocuments = _currentDocuments;
            if (currentDocuments is null)
            {
                return;
            }

            foreach (var currentDocument in currentDocuments)
            {
                currentDocument.Saved += OnCurrentDocumentSaved;
            }
        }

        private void UnsubscribeDocumentsSaved()
        {
            var currentDocuments = _currentDocuments;
            if (currentDocuments is null)
            {
                return;
            }

            foreach (var currentDocument in currentDocuments)
            {
                currentDocument.Saved -= OnCurrentDocumentSaved;
            }
        }

        private void OnCurrentDocumentSaved(object sender, EventArgs e)
        {
            DocumentSaved?.Invoke(sender, e);
        }
    }
}