// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioTasksHost : TasksHost
    {
        private readonly Dictionary<string, ErrorListProvider> errorLists = new Dictionary<string, ErrorListProvider>();

        public override void Add(string taskListName, Task task)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));
            Assert.ArgumentNotNull(task, nameof(task));

            var provider = GetProvider(taskListName);

            var category = TaskErrorCategory.Message;
            switch (task.Category)
            {
                case TaskCategory.Error:
                    category = TaskErrorCategory.Error;
                    break;
                case TaskCategory.Warning:
                    category = TaskErrorCategory.Warning;
                    break;
            }

            var t = new ErrorTask
            {
                Line = task.Line,
                Column = task.Column,
                Text = task.Text,
                ErrorCategory = category,
                Document = task.FileName
            };

            t.Navigate += NavigateErrorTask;

            provider.Tasks.Add(t);
        }

        public override void Clear(string taskListName, string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));

            ErrorListProvider provider;
            if (!errorLists.TryGetValue(taskListName, out provider))
            {
                return;
            }

            var errorTasks = provider.Tasks.OfType<ErrorTask>().ToList();
            for (var index = errorTasks.Count - 1; index >= 0; index--)
            {
                var task = errorTasks[index];

                if (task.Document.EndsWith(fileName))
                {
                    provider.Tasks.Remove(task);
                }
            }
        }

        public override void Show(string taskListName)
        {
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));

            var provider = GetProvider(taskListName);
            provider.Show();
        }

        [NotNull]
        private ErrorListProvider GetProvider([NotNull] string taskListName)
        {
            Debug.ArgumentNotNull(taskListName, nameof(taskListName));

            ErrorListProvider provider;
            if (!errorLists.TryGetValue(taskListName, out provider))
            {
                provider = new ErrorListProvider(SitecorePackage.Instance)
                {
                    ProviderGuid = new Guid(EnvDTE.Constants.vsViewKindCode),
                    ProviderName = taskListName,
                    DisableAutoRoute = false
                };

                errorLists[taskListName] = provider;
            }

            return provider;
        }

        private void NavigateErrorTask([NotNull] object sender, [NotNull] EventArgs arguments)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(arguments, nameof(arguments));

            var task = sender as ErrorTask;
            if (task == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(task.Document))
            {
                return;
            }

            IVsUIHierarchy hierarchy;
            uint itemId;
            IVsWindowFrame frame;
            if (!VsShellUtilities.IsDocumentOpen(SitecorePackage.Instance, task.Document, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemId, out frame))
            {
                VsShellUtilities.OpenDocument(SitecorePackage.Instance, task.Document, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemId, out frame);
            }

            object docData;
            frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out docData);

            var textBuffer = docData as VsTextBuffer;
            if (textBuffer == null)
            {
                var bufferProvider = docData as IVsTextBufferProvider;
                if (bufferProvider != null)
                {
                    IVsTextLines lines;
                    ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));

                    textBuffer = lines as VsTextBuffer;
                    if (textBuffer == null)
                    {
                        return;
                    }
                }
            }

            var textManagerService = SitecorePackage.Instance.GetTextManagerService();
            if (textManagerService == null)
            {
                return;
            }

            var logicalView = VSConstants.LOGVIEWID_Code;
            textManagerService.NavigateToLineAndColumn(textBuffer, ref logicalView, task.Line, task.Column, task.Line, task.Column);
        }
    }
}
