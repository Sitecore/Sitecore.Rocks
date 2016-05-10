// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Pipelines.DefaultActions;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Shell;
using TaskDialogInterop;

namespace Sitecore.Rocks.ContentTrees.Commands.Adding
{
    [Command(Submenu = AddSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.AddFromTemplate, typeof(ContentTreeContext), ToolBar = ToolBars.SitecoreExplorer.Id, Group = ToolBars.SitecoreExplorer.Editing, Icon = "Resources/16x16/item_new.png", Priority = 0x0100)]
    public class AddFromTemplate : CommandBase
    {
        public AddFromTemplate()
        {
            Text = Resources.AddFromTemplate_AddFromTemplate_New_Item___;
            Group = "NewItem";
            SortingValue = 1000;
            Icon = new Icon("Resources/16x16/new_document.png");
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

            return context.SelectedItems.FirstOrDefault() is ItemTreeViewItem;
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

            var dialog = new AddFromTemplateDialog
            {
                DatabaseUri = parent.ItemUri.DatabaseUri
            };

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var template = dialog.SelectedTemplate;
            if (template == null)
            {
                return;
            }

            var items = new List<ItemVersionUri>();

            foreach (var itemName in dialog.ItemNames)
            {
                ItemUri itemUri;

                if (template.IsBranch)
                {
                    itemUri = parent.ItemUri.Site.DataService.AddFromMaster(parent.ItemUri, template.TemplateUri, itemName);
                }
                else
                {
                    itemUri = parent.ItemUri.Site.DataService.AddFromTemplate(parent.ItemUri, template.TemplateUri, itemName);
                }

                if (itemUri == ItemUri.Empty)
                {
                    continue;
                }

                var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest);

                items.Add(itemVersionUri);
            }

            if (dialog.AddInsertOptionsCheckBox.IsChecked == true)
            {
                AddInsertOption(parent.Item, template.TemplateUri);
            }

            parent.RefreshAndExpand(false);

            if (items.Count() == 1)
            {
                var item = items.First();

                var treeView = parent.GetAncestor<ItemTreeView>();
                if (treeView != null)
                {
                    treeView.ExpandTo(item.ItemUri);
                }

                DefaultActionPipeline.Run().WithParameters(new ItemSelectionContext(new TemplatedItemDescriptor(item.ItemUri, string.Empty, template.TemplateId, template.Name)));
            }
            else
            {
                var list = items.Select(i => i.ItemUri).ToList();
                foreach (var o in parent.Items)
                {
                    var item = o as ItemTreeViewItem;
                    if (item == null)
                    {
                        continue;
                    }

                    if (list.Contains(item.Item.ItemUri))
                    {
                        item.IsSelected = true;
                        item.Focus();
                    }
                }

                AppHost.Windows.OpenContentEditor(items, LoadItemsOptions.Default);
            }

            foreach (var itemVersionUri in items)
            {
                Notifications.RaiseItemAdded(this, itemVersionUri, parent.ItemUri);
            }
        }

        private void AddInsertOption([NotNull] ItemHeader itemHeader, [NotNull] ItemUri templateUri)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));

            var itemUri = itemHeader.ItemUri;

            if (itemHeader.StandardValuesId != ItemId.Empty)
            {
                var options = new TaskDialogOptions
                {
                    Title = "Add to Insert Options",
                    CommonButtons = TaskDialogCommonButtons.None,
                    MainInstruction = "Where do you want to add the Insert Option?",
                    MainIcon = VistaTaskDialogIcon.Information,
                    DefaultButtonIndex = 0,
                    CommandButtons = new[]
                    {
                        "Standard Values Item",
                        string.Format("The \"{0}\" Item", itemHeader.Name)
                    },
                    AllowDialogCancellation = true
                };

                var r = TaskDialog.Show(options).CommandButtonResult;
                if (r == null)
                {
                    return;
                }

                if (r == 0)
                {
                    itemUri = new ItemUri(itemUri.DatabaseUri, itemHeader.StandardValuesId);
                }
            }

            var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest);

            GetValueCompleted<Item> completed = delegate(Item item)
            {
                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Insert Options/Insert Options/__Masters");

                var value = templateUri.ItemId.ToString();

                var field = item.Fields.FirstOrDefault(f => f != null && f.FieldUris.First().FieldId == fieldId);
                if (field == null)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(field.Value) && !field.Value.Contains(value))
                {
                    value = field.Value + @"|" + value;
                }

                AppHost.Server.UpdateItem(itemUri, fieldId, value);
            };

            AppHost.Server.GetItem(itemVersionUri, completed);
        }
    }
}
