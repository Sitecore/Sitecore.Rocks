// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI;
using Sitecore.Rocks.UI.LayoutDesigners.Extensions;
using Sitecore.Rocks.UI.Links;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Publishing;
using Sitecore.Rocks.UI.QueryAnalyzers;
using Sitecore.Rocks.UI.SearchAndReplace;
using Sitecore.Rocks.UI.TemplateHierarchies;
using Sitecore.Rocks.UI.XpathBuilder;

namespace Sitecore.Rocks.Shell.Environment
{
    public class WindowsHost
    {
        public enum Dock
        {
            None,

            Left,

            Right,

            Top,

            Bottom
        }

        [CanBeNull]
        public ContentEditor ActiveContentEditor
        {
            get { return ActiveContext.ActiveContentEditor; }
        }

        [CanBeNull]
        public ContentTree ActiveContentTree
        {
            get { return ActiveContext.ActiveContentTree; }
        }

        [NotNull]
        public virtual WindowFactory Factory
        {
            get { throw new InvalidOperationException("Windows host must be overridden"); }
        }

        public virtual void Activate<T>(string caption) where T : class
        {
            throw new InvalidOperationException("Window host must be overridden.");
        }

        [CanBeNull]
        public T GetToolWindow<T>([NotNull] string caption) where T : class
        {
            Assert.ArgumentNotNull(caption, nameof(caption));

            return AppHost.Windows.Factory.GetToolWindow<T>(caption);
        }

        public void OpenArchive([NotNull] DatabaseUri databaseUri, [NotNull] string header)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(header, nameof(header));

            AppHost.Windows.Factory.OpenArchive(databaseUri, header);
        }

        public void OpenContentEditor([NotNull] ItemVersionUri itemVersionUri)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));

            var uris = new List<ItemVersionUri>
            {
                new ItemVersionUri(itemVersionUri.ItemUri, LanguageManager.CurrentLanguage, Data.Version.Latest)
            };

            AppHost.Windows.OpenContentEditor(uris, LoadItemsOptions.Default);
        }

        public void OpenContentEditor([NotNull] IEnumerable<ItemVersionUri> itemVersionUris)
        {
            Assert.ArgumentNotNull(itemVersionUris, nameof(itemVersionUris));

            AppHost.Windows.Factory.OpenContentEditor(itemVersionUris.ToList(), new LoadItemsOptions(true));
        }

        public void OpenContentEditor([NotNull] List<ItemVersionUri> itemVersionUris, [NotNull] LoadItemsOptions options)
        {
            Assert.ArgumentNotNull(itemVersionUris, nameof(itemVersionUris));
            Assert.ArgumentNotNull(options, nameof(options));

            AppHost.Windows.Factory.OpenContentEditor(itemVersionUris, options);
        }

        public void OpenContentTree()
        {
            AppHost.Windows.Factory.OpenContentTree();
        }

        [CanBeNull]
        public T OpenDocumentWindow<T>([NotNull] string documentName) where T : class, new()
        {
            Assert.ArgumentNotNull(documentName, nameof(documentName));

            return AppHost.Windows.Factory.OpenDocument<T>(documentName);
        }

        public void OpenJobViewer([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            AppHost.Windows.Factory.OpenJobViewer(site);
        }

        public void OpenLayoutDesigner([NotNull] string document, [NotNull] Item item)
        {
            Assert.ArgumentNotNull(document, nameof(document));
            Assert.ArgumentNotNull(item, nameof(item));

            AppHost.Env.LayoutDesigner().Open(document, item);
        }

        [CanBeNull]
        public LinkViewer OpenLinkViewer()
        {
            return AppHost.Windows.Factory.OpenLinkViewer();
        }

        public void OpenLogViewer([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            AppHost.Windows.Factory.OpenLogViewer(site);
        }

        public void OpenManagementViewer([NotNull] string caption, [NotNull] IManagementContext context, [NotNull] string defaultItemName)
        {
            Assert.ArgumentNotNull(caption, nameof(caption));
            Assert.ArgumentNotNull(context, nameof(context));
            Assert.ArgumentNotNull(defaultItemName, nameof(defaultItemName));

            AppHost.Windows.Factory.OpenManagementViewer(caption, context, defaultItemName);
        }

        [CanBeNull]
        public MediaViewer OpenMediaLibrary([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            return AppHost.Windows.Factory.OpenMediaViewer(site);
        }

        public void OpenPropertyWindow()
        {
            AppHost.Windows.Factory.OpenPropertyWindow();
        }

        [CanBeNull]
        public PublishingQueue OpenPublishingQueueViewer([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return AppHost.Windows.Factory.OpenPublishingQueueViewer(databaseUri);
        }

        [CanBeNull]
        public QueryAnalyzer OpenQueryAnalyzer([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return AppHost.Windows.Factory.OpenQueryAnalyzer(databaseUri);
        }

        public void OpenRecycleBin([NotNull] DatabaseUri databaseUri, [NotNull] string header)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(header, nameof(header));

            AppHost.Windows.Factory.OpenRecycleBin(databaseUri, header);
        }

        [CanBeNull]
        public SearchAndReplacePanel OpenSearchAndReplace([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return AppHost.Windows.Factory.OpenSearchAndReplace(databaseUri);
        }

        public SearchAndReplacePanel OpenSearchAndReplace([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return AppHost.Windows.Factory.OpenSearchAndReplace(itemUri);
        }

        [CanBeNull]
        public SearchViewer OpenSearchViewer([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            return AppHost.Windows.Factory.OpenSearchViewer(site);
        }

        public void OpenStartPage()
        {
            AppHost.Windows.Factory.OpenStartPageViewer();
        }

        public void OpenTemplateDesigner([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            AppHost.Windows.Factory.OpenTemplateDesigner(templateUri);
        }

        public void OpenTemplateFieldSorter([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            AppHost.Windows.Factory.OpenTemplateFieldSorter(templateUri);
        }

        [CanBeNull]
        public TemplateHierarchyViewer OpenTemplateHierarchyViewer()
        {
            return AppHost.Windows.Factory.OpenTemplateHierarchyViewer();
        }

        public void OpenToolWindow<T>([NotNull] T control, [NotNull] string caption, Dock dock) where T : class
        {
            Assert.ArgumentNotNull(control, nameof(control));
            Assert.ArgumentNotNull(caption, nameof(caption));

            var formsControl = control as Control;
            if (formsControl != null)
            {
                var host = new WindowsFormsHost
                {
                    Child = formsControl
                };

                AppHost.Windows.Factory.OpenToolWindow(host, caption, dock);
                return;
            }

            AppHost.Windows.Factory.OpenToolWindow(control, caption, dock);
        }

        public void OpenValidationIssues([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            AppHost.Windows.Factory.OpenValidationIssues(itemUri);
        }

        [CanBeNull]
        public XpathBuilder OpenXpathBuilder([NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return AppHost.Windows.Factory.OpenXpathBuilder(databaseUri);
        }
    }
}
