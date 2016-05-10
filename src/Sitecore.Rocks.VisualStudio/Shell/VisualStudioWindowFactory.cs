// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.Shell.Panes;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Archives;
using Sitecore.Rocks.UI.EditorWindowHosts;
using Sitecore.Rocks.UI.JobViewer;
using Sitecore.Rocks.UI.Links;
using Sitecore.Rocks.UI.LogViewer;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Publishing;
using Sitecore.Rocks.UI.PublishingQueue;
using Sitecore.Rocks.UI.QueryAnalyzer;
using Sitecore.Rocks.UI.QueryAnalyzers;
using Sitecore.Rocks.UI.SearchAndReplace;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.TemplateDesigner;
using Sitecore.Rocks.UI.TemplateFieldSorter;
using Sitecore.Rocks.UI.TemplateHierarchies;
using Sitecore.Rocks.UI.ToolWindowPaneHosts;
using Sitecore.Rocks.UI.ValidationIssues;
using Sitecore.Rocks.UI.XpathBuilder;

namespace Sitecore.Rocks.Shell
{
    public class VisualStudioWindowFactory : WindowFactory
    {
        public override void ActivateToolWindow<T>(string caption)
        {
            Assert.ArgumentNotNull(caption, nameof(caption));

            for (var n = 0;; n++)
            {
                var toolWindow = SitecorePackage.Instance.FindToolWindow(typeof(ToolWindowPaneHost), n, false);
                if (toolWindow == null)
                {
                    return;
                }

                if (toolWindow.Caption != caption)
                {
                    continue;
                }

                var frame = toolWindow.Frame as IVsWindowFrame;
                if (frame != null)
                {
                    frame.Show();
                }

                return;
            }
        }

        public override T GetToolWindow<T>(string caption)
        {
            Assert.ArgumentNotNull(caption, nameof(caption));

            for (var n = 0;; n++)
            {
                var toolWindow = SitecorePackage.Instance.FindToolWindow(typeof(ToolWindowPaneHost), n, false);
                if (toolWindow == null)
                {
                    return null;
                }

                if (toolWindow.Caption == caption)
                {
                    return toolWindow.Content as T;
                }
            }
        }

        public override void OpenArchive(DatabaseUri databaseUri, string header)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(header, nameof(header));

            var archivePane = FindToolWindow<ArchivePane>(pane => pane.ArchiveViewer.DatabaseUri == databaseUri);

            archivePane.Initialize(databaseUri, header);
        }

        public override void OpenContentEditor(List<ItemVersionUri> itemVersionUris, LoadItemsOptions options)
        {
            Assert.ArgumentNotNull(itemVersionUris, nameof(itemVersionUris));
            Assert.ArgumentNotNull(options, nameof(options));

            var document = new StringBuilder();
            foreach (var uri in itemVersionUris)
            {
                document.Append(uri);
            }

            var window = ContentEditorFactory.CreateEditor(document.ToString(), options.NewTab);
            if (window == null)
            {
                AppHost.Output.Log("Failed to open Content Editor window");
                return;
            }

            window.LoadItems(itemVersionUris, options);
        }

        public override void OpenContentTree()
        {
            var window = SitecorePackage.Instance.FindToolWindow(typeof(ContentTreePane), 0, true) as ContentTreePane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(Resources.OpenSitecoreExplorer_Execute_Can_not_create_tool_window_);
            }

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public override T OpenDocument<T>(string documentName)
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            documentName = EditorDocumentName.GetDocumentName(documentName);

            IVsWindowFrame frame;
            IVsUIHierarchy hierarchy;
            uint itemId;
            if (VsShellUtilities.IsDocumentOpen(SitecorePackage.Instance, documentName, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemId, out frame))
            {
                frame.Show();
                return null;
            }

            var result = new T();

            FrameworkElement frameworkElement;

            var formsControls = result as Control;
            if (formsControls != null)
            {
                frameworkElement = new WindowsFormsHost
                {
                    Child = formsControls
                };
            }
            else
            {
                frameworkElement = result as FrameworkElement;
            }

            Assert.IsNotNull(frameworkElement, "Parameter 'control' must be of type FrameworkElement");

            EditorWindowFactory.CreateEditor(frameworkElement, documentName);

            return result;
        }

        public override void OpenJobViewer(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var logViewerPane = FindToolWindow<JobViewerPane>(pane => pane.JobViewer.Site == site);

            logViewerPane.JobViewer.Initialize(site);
        }

        public override LinkViewer OpenLinkViewer()
        {
            var pane = SitecorePackage.Instance.FindToolWindow(typeof(LinkPane), 0, true) as LinkPane;
            if (pane == null)
            {
                return null;
            }

            var windowFrame = (IVsWindowFrame)pane.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return pane.LinkViewer;
        }

        public override void OpenLogViewer(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var logViewerPane = FindToolWindow<LogViewerPane>(pane => pane.LogViewer.Site == site);

            logViewerPane.LogViewer.Initialize(site);
        }

        public override void OpenManagementViewer(string caption, IManagementContext context, string defaultItemName)
        {
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(defaultItemName, nameof(defaultItemName));

            var managementConsole = FindToolWindow<ManagementViewerPane>(pane => pane.ManagementViewer.Context == context);

            managementConsole.ManagementViewer.Initialize(caption, context, defaultItemName);
        }

        public override MediaViewer OpenMediaViewer(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var mediaViewerPane = FindToolWindow<MediaPane>(pane => pane.MediaViewer.Site == site);

            mediaViewerPane.MediaViewer.Initialize(site);

            return mediaViewerPane.MediaViewer;
        }

        public override void OpenPropertyWindow()
        {
            IVsWindowFrame frame;

            var shell = (IVsUIShell)Package.GetGlobalService(typeof(Microsoft.VisualStudio.Shell.Interop.SVsUIShell));

            var guidPropertyBrowser = new Guid(ToolWindowGuids.PropertyBrowser);
            shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref guidPropertyBrowser, out frame);
            frame.Show();
        }

        public override PublishingQueue OpenPublishingQueueViewer(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var result = FindToolWindow<PublishingQueuePane>(pane => pane.PublishingQueueViewer.DatabaseUri == databaseUri);

            result.PublishingQueueViewer.Initialize(databaseUri);

            return result.PublishingQueueViewer;
        }

        public override QueryAnalyzer OpenQueryAnalyzer(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var window = SitecorePackage.Instance.FindToolWindow(typeof(QueryAnalyzerToolPane), 0, true) as QueryAnalyzerToolPane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException("Not found");
            }

            window.Initialize(databaseUri);

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return (QueryAnalyzer)window.Content;
        }

        public override void OpenRecycleBin(DatabaseUri databaseUri, string header)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(header, nameof(header));

            var recycleBinPane = FindToolWindow<RecycleBinPane>(pane => pane.RecycleBinViewer.DatabaseUri == databaseUri);

            recycleBinPane.Initialize(databaseUri, header);
        }

        public override SearchAndReplacePanel OpenSearchAndReplace(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var window = SitecorePackage.Instance.FindToolWindow(typeof(SearchAndReplacePane), 0, true) as SearchAndReplacePane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(@"Not found");
            }

            window.Initialize(databaseUri);

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return (SearchAndReplacePanel)window.Content;
        }

        public override SearchAndReplacePanel OpenSearchAndReplace(ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var window = SitecorePackage.Instance.FindToolWindow(typeof(SearchAndReplacePane), 0, true) as SearchAndReplacePane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(@"Not found");
            }

            window.Initialize(itemUri);

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return (SearchAndReplacePanel)window.Content;
        }

        public override SearchViewer OpenSearchViewer(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            var searchViewerPane = FindToolWindow<SearchPane>(pane => pane.SearchViewer.Site == site);

            searchViewerPane.SearchViewer.Initialize(site);

            return searchViewerPane.SearchViewer;
        }

        public override StartPageViewer OpenStartPageViewer()
        {
            var startPagePane = FindToolWindow<StartPagePane>(pane => true);

            return startPagePane.StartPageViewer;
        }

        public override void OpenTemplateDesigner(ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            var window = TemplateDesignerFactory.CreateEditor(templateUri.ToString());
            if (window != null)
            {
                window.Initialize(templateUri);
            }
        }

        public override void OpenTemplateFieldSorter(ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            var window = TemplateFieldSorterFactory.CreateEditor(templateUri.ToString());
            if (window != null)
            {
                window.Initialize(templateUri);
            }
        }

        public override TemplateHierarchyViewer OpenTemplateHierarchyViewer()
        {
            var pane = SitecorePackage.Instance.FindToolWindow(typeof(TemplateHierarchyPane), 0, true) as TemplateHierarchyPane;
            if (pane == null)
            {
                return null;
            }

            var windowFrame = (IVsWindowFrame)pane.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return pane.TemplateHierarchyViewer;
        }

        public override void OpenToolWindow<T>(T control, string caption, WindowsHost.Dock dock)
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(caption, nameof(caption));

            var formsControl = control as Control;
            if (formsControl != null)
            {
                ToolWindowPaneHost.Show(formsControl, caption);
                return;
            }

            var frameworkElement = control as FrameworkElement;
            Assert.IsNotNull(frameworkElement, "Parameter 'control' must be of type 'FrameworkElement'.");

            ToolWindowPaneHost.Show(frameworkElement, caption);
        }

        public override void OpenValidationIssues(ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            var window = SitecorePackage.Instance.FindToolWindow(typeof(ValidationIssuesPane), 0, true) as ValidationIssuesPane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(Resources.OpenJobViewer_Execute_Can_not_create_tool_window_);
            }

            window.SetSource(itemUri);

            var windowFrame = (IVsWindowFrame)window.Frame;

            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        public override XpathBuilder OpenXpathBuilder(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var window = SitecorePackage.Instance.FindToolWindow(typeof(XpathBuilderToolPane), 0, true) as XpathBuilderToolPane;
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException("Not found");
            }

            window.Initialize(databaseUri);

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());

            return (XpathBuilder)window.Content;
        }

        [NotNull]
        private T FindToolWindow<T>([NotNull] Func<T, bool> isSame) where T : ToolWindowPane
        {
            Debug.ArgumentNotNull(isSame, nameof(isSame));

            T result;

            for (var n = 0;; n++)
            {
                var toolWindow = SitecorePackage.Instance.FindToolWindow(typeof(T), n, false);
                if (toolWindow != null)
                {
                    var pane = toolWindow as T;
                    if (pane != null && isSame(pane))
                    {
                        result = pane;
                        break;
                    }
                }
                else
                {
                    result = (T)SitecorePackage.Instance.FindToolWindow(typeof(T), n, true);
                    break;
                }
            }

            var frame = (IVsWindowFrame)result.Frame;

            ErrorHandler.ThrowOnFailure(frame.Show());

            return result;
        }
    }
}
