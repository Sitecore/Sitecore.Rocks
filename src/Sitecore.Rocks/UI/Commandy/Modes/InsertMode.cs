// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class InsertMode : ModeBase
    {
        private readonly List<ItemHeader> insertOptions = new List<ItemHeader>();

        public InsertMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Insert Options";
            Alias = "i";

            var context = commandy.Parameter as IItemSelectionContext;
            if (context != null && context.Items.Count() == 1)
            {
                var item = context.Items.First();

                item.ItemUri.Site.DataService.GetInsertOptions(item.ItemUri, LoadInsertOptions);
            }
            else
            {
                IsReady = true;
            }
        }

        [NotNull]
        public IEnumerable<ItemHeader> InsertOptions
        {
            get { return insertOptions; }
        }

        public override string Watermark
        {
            get { return "Insert Option"; }
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            return context.Items.Any();
        }

        public override void Execute(Hit hit, object parameter)
        {
            Assert.ArgumentNotNull(hit, nameof(hit));
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var parentItem = context.Items.First();

            var templateItemHeader = hit.Tag as ItemHeader;
            if (templateItemHeader == null)
            {
                return;
            }

            var d = new AddInsertOptionDialog();

            d.NewItemName.Text = string.Format(Resources.InsertOptions_CreateItem_New__0_, templateItemHeader.Name);

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            var newName = d.ItemName;
            var templateId = templateItemHeader.ItemUri.ItemId;
            var templateUri = new ItemUri(parentItem.ItemUri.DatabaseUri, templateId);

            var newItemUri = AppHost.Server.AddFromTemplate(parentItem.ItemUri, templateUri, newName);
            if (newItemUri == ItemUri.Empty)
            {
                return;
            }

            var itemVersionUri = new ItemVersionUri(newItemUri, LanguageManager.CurrentLanguage, Version.Latest);

            Notifications.RaiseItemAdded(this, itemVersionUri, parentItem.ItemUri);

            if (AppHost.CurrentContentTree != null)
            {
                AppHost.CurrentContentTree.Locate(newItemUri);
            }

            AppHost.OpenContentEditor(itemVersionUri);
        }

        private void LoadInsertOptions([NotNull] IEnumerable<ItemHeader> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            insertOptions.AddRange(items);
            IsReady = true;
        }
    }
}
