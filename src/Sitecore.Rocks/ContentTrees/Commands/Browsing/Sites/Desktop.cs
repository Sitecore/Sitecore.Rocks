// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowseDesktop, typeof(ContentTreeContext), Text = "Browse Desktop"), Feature(FeatureNames.Browsing)]
    public class Desktop : BrowseSiteCommand
    {
        public Desktop()
        {
            Text = Resources.Desktop_Desktop_Desktop;
            Group = "Client";
            SortingValue = 3000;
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return "/sitecore/shell/default.aspx";
        }
    }
}
