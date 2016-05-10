// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.ContentTrees.Commands.Adding
{
    [Command, CommandId(CommandIds.SitecoreExplorer.NewFolder, typeof(ContentTreeContext))]
    public class NewFolder : CommandBase
    {
        public NewFolder()
        {
            Text = Resources.NewFolder_NewFolder_New_Folder;
            Group = "Add";
            SortingValue = 5410;
            Icon = new Icon("Resources/16x16/newfolder.png");

            TemplateId = new Guid(@"{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}");
            TemplateName = "Folder";
        }

        protected Guid TemplateId { get; set; }

        [NotNull]
        protected string TemplateName { get; set; }

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
            if (item == null)
            {
                return false;
            }

            if (!CanExecute(item))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var parent = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (parent == null)
            {
                return;
            }

            var templateUri = new ItemUri(parent.ItemUri.DatabaseUri, new ItemId(TemplateId));

            var itemUri = parent.ItemUri.Site.DataService.AddFromTemplate(parent.ItemUri, templateUri, Resources.NewFolder_Execute_New_Folder);
            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            var item = context.ContentTree.ExpandTo(itemUri);
            if (item == null)
            {
                return;
            }

            item.Rename();

            Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest), parent.ItemUri);
        }

        protected virtual bool CanExecute([NotNull] ItemTreeViewItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return true;
        }
    }
}
