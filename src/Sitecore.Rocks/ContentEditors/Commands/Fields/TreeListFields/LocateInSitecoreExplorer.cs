// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.TreeListFields
{
    [Command]
    public class LocateInSitecoreExplorer : CommandBase
    {
        public LocateInSitecoreExplorer()
        {
            Text = Resources.Locate_in_Sitecore_Explorer;
            Group = "Link";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as TreeListFieldContext;
            if (context == null)
            {
                return false;
            }

            return ActiveContext.ActiveContentTree != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as TreeListFieldContext;
            if (context == null)
            {
                return;
            }

            var itemUri = context.ItemUri;
            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            var contentTree = ActiveContext.ActiveContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(itemUri);
            }
        }
    }
}
