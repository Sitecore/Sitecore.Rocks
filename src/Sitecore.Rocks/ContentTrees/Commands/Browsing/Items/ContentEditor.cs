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
    [Command(Submenu = BrowseItemSubmenu.Name), RuleAction("browse item in Content Editor", "Web Browsing - Item"), CommandId(CommandIds.SitecoreExplorer.BrowseItemInContentEditor, typeof(ContentTreeContext), Text = "Browse Item in Content Editor"), Feature(FeatureNames.Browsing)]
    public class ContentEditor : BrowseItemCommand, IRuleAction
    {
        public ContentEditor()
        {
            Text = Resources.ContentEditor_ContentEditor_Content_Editor;
            Group = "Client";
            SortingValue = 2000;
        }

        protected override string GetUrl(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString(@"/sitecore/shell/Applications/Content Manager/default.aspx");

            path["fo"] = item.ItemUri.ItemId.ToString();

            path["sc_content"] = item.ItemUri.DatabaseName.Name;

            var i = item as Item;
            if (i != null)
            {
                path["la"] = i.Uri.Language.ToString();
            }

            return path.ToString();
        }
    }
}
