// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.ContentTrees.Commands.Browsing.Sites
{
    [Command(Submenu = BrowseSiteSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.BrowsePageEditor, typeof(ContentTreeContext), Text = "Browse Page Editor"), Feature(FeatureNames.Browsing)]
    public class PageEditor : BrowseSiteCommand
    {
        public PageEditor()
        {
            Text = Resources.PageEditor_PageEditor_Page_Editor;
            Group = "Page";
            SortingValue = 4500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as SiteTreeViewItem;
            if (item == null)
            {
                return false;
            }

            Text = item.Site.SitecoreVersion >= Constants.Versions.Version80 ? "Experience Editor" : Resources.PageEditor_PageEditor_Page_Editor;

            return base.CanExecute(parameter);
        }

        protected override string GetUrl(SiteTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var path = new UrlString("/default.aspx");

            path["sc_mode"] = @"edit";

            return path.ToString();
        }
    }
}
