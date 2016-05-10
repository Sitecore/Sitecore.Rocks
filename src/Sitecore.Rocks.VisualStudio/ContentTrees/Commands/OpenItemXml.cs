// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command(Submenu = "XML")]
    public class OpenItemXml : CommandBase
    {
        public OpenItemXml()
        {
            Text = Resources.OpenItemXml_OpenItemXml_Open_Item_Xml;
            Group = "Copy";
            SortingValue = 1500;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            return item != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return;
            }

            var item = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            GetValueCompleted<string> callback = delegate(string value)
            {
                var fileName = Path.Combine(Path.GetTempPath(), item.Item.Name + ".xml");
                fileName = IO.File.MakeUniqueFileName(fileName);

                value = @"<xml>" + value + @"</xml>";

                IO.File.Save(fileName, value);

                SitecorePackage.Instance.Dte.Application.ItemOperations.OpenFile(fileName);

                File.Delete(fileName);
            };

            item.ItemUri.Site.DataService.GetItemXmlAsync(item.ItemUri, true, callback);
        }
    }
}
