// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Text;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class SetDisplayNames : CommandBase
    {
        public SetDisplayNames()
        {
            Text = Resources.SetDisplayNames_SetDisplayNames_Set_Display_Names;
            Group = "Fields";
            SortingValue = 3500;
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

            if (!item.ItemUri.Site.DataService.CanExecuteAsync("Items.SetDisplayNames"))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First();

            var d = new SetDisplayNamesDialog(item.ItemUri);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var values = new StringBuilder();

            foreach (var displayName in d.DisplayNames)
            {
                if (values.Length > 0)
                {
                    values.Append('^');
                }

                values.Append(displayName.Item1);
                values.Append('|');
                values.Append(displayName.Item2);
            }

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                foreach (var element in root.Elements())
                {
                    var language = element.GetAttributeValue("l");
                    var version = element.GetAttributeValue("v");
                    var fieldId = element.GetAttributeValue("f");
                    var value = element.Value;

                    var itemVersionUri = new ItemVersionUri(item.ItemUri, new Language(language), new Data.Version(int.Parse(version)));
                    var fieldUri = new FieldUri(itemVersionUri, new FieldId(new Guid(fieldId)));

                    Notifications.RaiseFieldChanged(this, fieldUri, value);
                }
            };

            item.ItemUri.Site.DataService.ExecuteAsync("Items.SetDisplayNames", completed, item.ItemUri.ItemId.ToString(), item.ItemUri.DatabaseName.ToString(), values.ToString());
        }
    }
}
