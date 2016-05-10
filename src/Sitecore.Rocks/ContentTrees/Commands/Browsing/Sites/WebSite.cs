// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowseWebSite, typeof(ContentTreeContext), Text = "Browse Web Site"), Feature(FeatureNames.Browsing)]
    public class WebSite : BrowseSiteCommand
    {
        public WebSite()
        {
            Text = Resources.WebSite_WebSite_Web_Site;
            Group = "WebSite";
            SortingValue = 1000;
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return "/";
        }
    }
}
