// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Publishing;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class SetPublishingRestrictions : CommandBase
    {
        public SetPublishingRestrictions()
        {
            Text = Resources.SetPublishingRestrictions_SetPublishingRestrictions_Set_Publishing_Restrictions;
            Group = "Fields";
            SortingValue = 4000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (context.Items.Count() != 1)
            {
                return false;
            }

            var item = context.Items.First();

            return item.ItemUri.Site.CanExecute;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First();

            var itemVersionUri = new ItemVersionUri(item.ItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            var d = new PublishingRestrictions(itemVersionUri);

            AppHost.Shell.ShowDialog(d);
        }
    }
}
