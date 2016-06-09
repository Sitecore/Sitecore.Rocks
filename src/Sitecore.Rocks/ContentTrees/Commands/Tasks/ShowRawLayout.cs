// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), ToolbarElement(typeof(IItemSelectionContext), 3000, "Layout", "Design", Icon = "Resources/32x32/Window-Edit.png")]
    public class ShowRawLayout : DesignLayoutBase, IToolbarElement
    {
        public ShowRawLayout()
        {
            Text = "Show Raw Layout";
            Group = "Items";
            SortingValue = 1060;
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

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return false;
            }

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.EditTemplate) == DataServiceFeatureCapabilities.EditTemplate;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.First();
            if (item == null)
            {
                return;
            }

            Site.RequestCompleted callback = delegate(string response)
            {
                var fileName = Path.Combine(Path.GetTempPath(), item.Name + @".rawlayout.xml");
                fileName = IO.File.MakeUniqueFileName(fileName);

                IO.File.Save(fileName, response);

                AppHost.Files.OpenFile(fileName);

                File.Delete(fileName);
            };

            item.ItemUri.Site.Execute("Layouts.GetRawLayout", callback, item.ItemUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString());
        }
    }
}
