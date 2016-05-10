// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowsePreview, typeof(ContentTreeContext), Text = "Browse Preview"), Feature(FeatureNames.Browsing)]
    public class Preview : BrowseSiteCommand
    {
        public Preview()
        {
            Text = Resources.Preview_Preview_Preview;
            Group = "Page";
            SortingValue = 4000;
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString("/default.aspx");

            path["sc_mode"] = @"preview";

            return path.ToString();
        }
    }
}
