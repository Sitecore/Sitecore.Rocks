// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowseDebugAndTrace, typeof(ContentTreeContext), Text = "Browse Debug And Trace"), Feature(FeatureNames.Browsing)]
    public class DebugTrace : BrowseSiteCommand
    {
        public DebugTrace()
        {
            Text = Resources.DebugTrace_DebugTrace_Debug;
            Group = "Page";
            SortingValue = 5000;
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString(@"/default.aspx");

            path["sc_debug"] = @"1";
            path["sc_prof"] = @"1";
            path["sc_trace"] = @"1";
            path["sc_ri"] = @"1";

            return path.ToString();
        }
    }
}
