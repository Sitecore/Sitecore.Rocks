// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Media;
using Sitecore.Rocks.Searching;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Links;
using Sitecore.Rocks.UI.Management;
using Sitecore.Rocks.UI.Publishing;
using Sitecore.Rocks.UI.QueryAnalyzers;
using Sitecore.Rocks.UI.SearchAndReplace;
using Sitecore.Rocks.UI.StartPage;
using Sitecore.Rocks.UI.TemplateHierarchies;
using Sitecore.Rocks.UI.XpathBuilder;

namespace Sitecore.Rocks.Shell
{
    public abstract class WindowFactory
    {
        public abstract void ActivateToolWindow<T>([NotNull] string caption) where T : class;

        [CanBeNull]
        public abstract T GetToolWindow<T>([NotNull] string caption) where T : class;

        public abstract void OpenArchive([NotNull] DatabaseUri databaseUri, [NotNull] string header);

        public abstract void OpenContentEditor([NotNull] List<ItemVersionUri> itemVersionUris, [NotNull] LoadItemsOptions options);

        public abstract void OpenContentTree();

        [CanBeNull]
        public abstract T OpenDocument<T>([NotNull] string documentName) where T : class, new();

        public abstract void OpenJobViewer([NotNull] Site site);

        [CanBeNull]
        public abstract LinkViewer OpenLinkViewer();

        public abstract void OpenLogViewer([NotNull] Site site);

        public abstract void OpenManagementViewer([NotNull] string caption, [NotNull] IManagementContext context, [NotNull] string defaultItemName);

        [CanBeNull]
        public abstract MediaViewer OpenMediaViewer([NotNull] Site site);

        public abstract void OpenPropertyWindow();

        [CanBeNull]
        public abstract PublishingQueue OpenPublishingQueueViewer([NotNull] DatabaseUri databaseUri);

        [CanBeNull]
        public abstract QueryAnalyzer OpenQueryAnalyzer([NotNull] DatabaseUri databaseUri);

        public abstract void OpenRecycleBin([NotNull] DatabaseUri databaseUri, [NotNull] string header);

        [CanBeNull]
        public abstract SearchAndReplacePanel OpenSearchAndReplace([NotNull] DatabaseUri databaseUri);

        [CanBeNull]
        public abstract SearchAndReplacePanel OpenSearchAndReplace([NotNull] ItemUri itemUri);

        [CanBeNull]
        public abstract SearchViewer OpenSearchViewer([NotNull] Site site);

        [CanBeNull]
        public abstract StartPageViewer OpenStartPageViewer();

        public abstract void OpenTemplateDesigner([NotNull] ItemUri templateUri);

        public abstract void OpenTemplateFieldSorter([NotNull] ItemUri templateUri);

        [CanBeNull]
        public abstract TemplateHierarchyViewer OpenTemplateHierarchyViewer();

        public abstract void OpenToolWindow<T>([NotNull] T control, [Localizable(false), NotNull] string caption, WindowsHost.Dock dock) where T : class;

        public abstract void OpenValidationIssues([NotNull] ItemUri itemUri);

        [CanBeNull]
        public abstract XpathBuilder OpenXpathBuilder([NotNull] DatabaseUri databaseUri);
    }
}
