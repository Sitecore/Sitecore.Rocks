// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Exporting
{
    [Command(Submenu = "XML"), CommandId(CommandIds.SitecoreExplorer.PasteItemXml, typeof(ContentTreeContext)), Feature(FeatureNames.Exporting)]
    public class PasteItemXml : CommandBase
    {
        public PasteItemXml()
        {
            Text = Resources.PasteItemXml_PasteItemXml_Paste_Item_Xml___;
            Group = "Paste";
            SortingValue = 2000;
            Icon = new Icon("Resources/16x16/paste.png");
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

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.CopyPasteItemXml) == DataServiceFeatureCapabilities.CopyPasteItemXml;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var item = context.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var dialog = new PasteItemXmlDialog
            {
                ItemUri = item.ItemUri
            };

            AppHost.Shell.ShowDialog(dialog);
        }
    }
}
