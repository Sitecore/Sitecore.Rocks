// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowseContentEditor, typeof(ContentTreeContext), Text = "Browse Content Editor"), Feature(FeatureNames.Browsing)]
    public class ContentEditor : BrowseSiteCommand
    {
        public ContentEditor()
        {
            Text = Resources.ContentEditor_ContentEditor_Content_Editor;
            Group = "Client";
            SortingValue = 2000;
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return "/sitecore/shell/Applications/Content Manager/default.aspx";
        }
    }
}
