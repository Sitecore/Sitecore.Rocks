// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Controls.ItemDependencyListViews.Commands
{
    [Command]
    public class SelectTemplates : CommandBase
    {
        public SelectTemplates()
        {
            Text = "Select Templates";
            Group = "Selection";
            SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ItemDependencyContext;
            if (context == null)
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ItemDependencyContext;
            if (context == null)
            {
                return;
            }

            var templateId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template");
            var templateSectionId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template section");
            var templateFieldId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template field");

            context.Control.SetCheckBoxes(delegate(ItemDependencyListView.ItemDescriptor i)
            {
                if (i.IsChecked)
                {
                    return;
                }

                i.IsChecked = i.Item.TemplateId == templateId || i.Item.TemplateId == templateSectionId || i.Item.TemplateId == templateFieldId;
            });

            context.Control.RefreshItems();
        }
    }
}
