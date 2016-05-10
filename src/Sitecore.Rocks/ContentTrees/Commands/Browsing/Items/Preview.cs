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
    [Command(Submenu = BrowseItemSubmenu.Name), RuleAction("browse item in Preview", "Web Browsing - Item"), CommandId(CommandIds.SitecoreExplorer.BrowseItemInPreview, typeof(ContentTreeContext), Text = "Browse Item in Preview"), Feature(FeatureNames.Browsing)]
    public class Preview : BrowseItemCommand, IRuleAction
    {
        public Preview()
        {
            Text = Resources.Preview_Preview_Preview;
            Group = "Page";
            SortingValue = 4000;
        }

        protected override string GetUrl(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString(@"/");

            path["sc_itemid"] = item.ItemUri.ItemId.ToString();
            path["sc_mode"] = @"preview";

            var i = item as Item;
            if (i != null)
            {
                path["sc_lang"] = i.Uri.Language.ToString();
            }

            return path.ToString();
        }
    }
}
