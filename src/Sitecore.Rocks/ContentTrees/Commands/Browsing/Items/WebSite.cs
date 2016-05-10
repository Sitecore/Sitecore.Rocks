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
    [Command(Submenu = BrowseItemSubmenu.Name), RuleAction("browse published item", "Web Browsing - Item"), CommandId(CommandIds.SitecoreExplorer.BrowseItemInWebSite, typeof(ContentTreeContext), Text = "Browse Published Item"), Feature(FeatureNames.Browsing)]
    public class WebSite : BrowseItemCommand, IRuleAction
    {
        public WebSite()
        {
            Text = Resources.WebSite_WebSite_Web_Site;
            Group = "WebSite";
            SortingValue = 1000;
        }

        protected override string GetUrl(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString(@"/default.aspx");

            path["sc_itemid"] = item.ItemUri.ItemId.ToString();

            var i = item as Item;
            if (i != null)
            {
                path["sc_lang"] = i.Uri.Language.ToString();
            }

            return path.ToString();
        }
    }
}
