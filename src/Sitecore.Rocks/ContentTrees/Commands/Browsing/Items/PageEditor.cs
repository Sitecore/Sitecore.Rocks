// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Rules;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Items
{
    [Command(Submenu = BrowseItemSubmenu.Name), RuleAction("browse item in Page Editor", "Web Browsing - Item"), CommandId(CommandIds.SitecoreExplorer.BrowseItemInPageEditor, typeof(ContentTreeContext), Text = "Browse Item in Page Editor"), Feature(FeatureNames.Browsing)]
    public class PageEditor : BrowseItemCommand, IRuleAction
    {
        public PageEditor()
        {
            Text = Resources.PageEditor_PageEditor_Page_Editor;
            Group = "Page";
            SortingValue = 4500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            Text = item.ItemUri.Site.SitecoreVersion >= Constants.Versions.Version80 ? "Experience Editor" : Resources.PageEditor_PageEditor_Page_Editor;

            return base.CanExecute(parameter);
        }

        protected override string GetUrl(IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString("/");

            path["sc_itemid"] = item.ItemUri.ItemId.ToString();
            path["sc_mode"] = @"edit";

            var i = item as Item;
            if (i != null)
            {
                path["sc_lang"] = i.Uri.Language.ToString();
            }

            return path.ToString();
        }
    }
}
