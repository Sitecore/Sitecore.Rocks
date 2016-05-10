// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), Feature(FeatureNames.AdvancedOperations)]
    public class ApplyFieldPainter : CommandBase
    {
        static ApplyFieldPainter()
        {
            Fields = new List<Tuple<Field, string>>();
        }

        public ApplyFieldPainter()
        {
            Text = "Apply Field Painter";
            Group = "Fields";
            SortingValue = 4015;
        }

        [NotNull]
        public static List<Tuple<Field, string>> Fields { get; }

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

            if (!Fields.Any())
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

            foreach (var field in Fields)
            {
                foreach (var item in context.Items)
                {
                    var itemVersionUri = new ItemVersionUri(item.ItemUri, Language.Current, Data.Version.Latest);

                    ItemModifier.Edit(itemVersionUri, field.Item1.Name, field.Item2);
                }
            }
        }
    }
}
