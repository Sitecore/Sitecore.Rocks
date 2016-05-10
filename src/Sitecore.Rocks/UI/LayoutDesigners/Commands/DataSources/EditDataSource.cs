// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.DataSources
{
    [Command(Submenu = DataSourceSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4210, "Rendering", "Data Sources", Icon = "Resources/16x16/pencil.png", ElementType = RibbonElementType.SmallButton)]
    public class EditDataSource : CommandBase, IToolbarElement
    {
        public EditDataSource()
        {
            Text = "Edit";
            Group = "DataSource";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return false;
            }

            return !string.IsNullOrEmpty(item.DataSource);
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (item == null)
            {
                return;
            }

            var databaseUri = context.LayoutDesigner.DatabaseUri;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(databaseUri, element);

                AppHost.OpenContentEditor(new ItemVersionUri(itemHeader.ItemUri, LanguageManager.CurrentLanguage, Version.Latest));
            };

            databaseUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", completed, item.DataSource, databaseUri.DatabaseName.ToString());
        }
    }
}
