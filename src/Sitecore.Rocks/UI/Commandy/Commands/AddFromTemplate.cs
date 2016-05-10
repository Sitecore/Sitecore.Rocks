// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Commandy.Commands
{
    public class AddFromTemplate : CommandBase
    {
        public AddFromTemplate([NotNull] TemplateHeader templateHeader)
        {
            TemplateHeader = templateHeader;
            Assert.ArgumentNotNull(templateHeader, nameof(templateHeader));

            Text = "Add " + templateHeader.Name;
            Group = "Template";
            SortingValue = 1000;
        }

        [NotNull]
        protected TemplateHeader TemplateHeader { get; }

        public override bool CanExecute(object parameter)
        {
            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var itemUri = context.Items.FirstOrDefault();
            if (itemUri == null)
            {
                return;
            }

            var d = new AddInsertOptionDialog();

            d.NewItemName.Text = string.Format(Resources.InsertOptions_CreateItem_New__0_, TemplateHeader.Name);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var newItemUri = AppHost.Server.AddFromTemplate(itemUri.ItemUri, TemplateHeader.TemplateUri, d.ItemName);
            if (newItemUri == ItemUri.Empty)
            {
                return;
            }

            var itemVersionUri = new ItemVersionUri(newItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            Notifications.RaiseItemAdded(this, itemVersionUri, itemUri.ItemUri);

            if (AppHost.CurrentContentTree != null)
            {
                AppHost.CurrentContentTree.Locate(newItemUri);
            }

            DefaultActionPipeline.Run().WithParameters(new ItemSelectionContext(new TemplatedItemDescriptor(itemVersionUri.ItemUri, d.ItemName, TemplateHeader.TemplateUri.ItemId, TemplateHeader.Name)));
        }
    }
}
