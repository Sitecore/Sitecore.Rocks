// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Items
{
    [Command(Submenu = BrowseItemSubmenu.Name), RuleAction("browse item in Debug and Trace", "Web Browsing - Item"), CommandId(CommandIds.SitecoreExplorer.BrowseItemInDebugAndTrace, typeof(ContentTreeContext), Text = "Browse Item in Debug and Trace"), Feature(FeatureNames.Browsing)]
    public class DebugTrace : BrowseItemCommand, IRuleAction
    {
        public DebugTrace()
        {
            Text = Resources.DebugPage_DebugPage_Debug_and_Trace;
            Group = "Page";
            SortingValue = 5000;
        }

        protected override string GetUrl(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString(@"/default.aspx");

            path["sc_itemid"] = item.ItemUri.ItemId.ToString();

            path["sc_debug"] = @"1";
            path["sc_prof"] = @"1";
            path["sc_trace"] = @"1";
            path["sc_ri"] = @"1";

            var i = item as Item;
            if (i != null)
            {
                path["sc_lang"] = i.Uri.Language.ToString();
            }

            return path.ToString();
        }
    }
}
