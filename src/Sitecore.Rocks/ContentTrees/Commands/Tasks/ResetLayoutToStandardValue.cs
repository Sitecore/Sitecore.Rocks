// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensions.IItemSelectionContextExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name)]
    public class ResetLayoutToStandardValue : CommandBase
    {
        public ResetLayoutToStandardValue()
        {
            Text = "Reset Layout to Standard Value";
            Group = "Fields";
            SortingValue = 4010;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            if (!context.Items.Any())
            {
                return false;
            }

            if (!context.IsSameDatabase())
            {
                return false;
            }

            if (!context.Items.All(i => i.ItemUri.Site.DataService.CanExecuteAsync("Items.Save")))
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

            if (AppHost.MessageBox("Are you sure you want to reset the layout to the standard value?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var databaseUri = DatabaseUri.Empty;
            var fields = new List<Field>();
            var layoutFieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Layout/Layout/__Renderings");

            foreach (var item in context.Items)
            {
                var field = new Field
                {
                    ResetOnSave = true,
                    HasValue = true,
                    Value = string.Empty
                };

                field.FieldUris.Add(new FieldUri(new ItemVersionUri(item.ItemUri, Language.Current, Version.Latest), layoutFieldId));

                fields.Add(field);

                if (databaseUri == DatabaseUri.Empty)
                {
                    databaseUri = item.ItemUri.DatabaseUri;
                }
            }

            ItemModifier.Edit(databaseUri, fields, false);
        }
    }
}
