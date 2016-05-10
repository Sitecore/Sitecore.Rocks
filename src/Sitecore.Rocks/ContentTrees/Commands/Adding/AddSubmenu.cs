// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Dialogs;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Controls.TemplateSelector;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Adding
{
    [Command(ExcludeFromSearch = true)]
    public class AddSubmenu : CommandBase
    {
        public const string Name = "Add";

        public AddSubmenu()
        {
            Text = Resources.Add_Add_Add;
            Group = "Add";
            SortingValue = 5300;

            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected ContentTreeContext Context { get; set; }

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

            if (!(context.SelectedItems.FirstOrDefault() is ItemTreeViewItem))
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        public override IEnumerable<ICommand> GetSubmenuCommands(object parameter)
        {
            Assert.ArgumentNotNull(parameter, nameof(parameter));

            return CommandManager.GetCommands(parameter, Name);
        }

        private void CreateItem([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var parent = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (parent == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var templateHeader = menuItem.Tag as TemplateHeader;
            if (templateHeader == null)
            {
                return;
            }

            var d = new AddInsertOptionDialog();
            d.NewItemName.Text = templateHeader.Name;

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return;
            }

            RecentTemplateManager.AddToRecent(templateHeader);

            var newName = d.ItemName;
            var templateId = templateHeader.TemplateUri.ItemId;
            var templateUri = new ItemUri(parent.ItemUri.DatabaseUri, templateId);

            ItemUri itemUri;

            var isBranch = IdManager.GetTemplateType(templateId) != "template";
            if (isBranch)
            {
                itemUri = parent.ItemUri.Site.DataService.AddFromMaster(parent.ItemUri, templateUri, newName);
            }
            else
            {
                itemUri = parent.ItemUri.Site.DataService.AddFromTemplate(parent.ItemUri, templateUri, newName);
            }

            if (itemUri == ItemUri.Empty)
            {
                return;
            }

            parent.RefreshAndExpand(false);
            parent.SelectChildItems(new[]
            {
                itemUri
            });

            var itemVersionUri = new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest);
            AppHost.OpenContentEditor(itemVersionUri);

            Notifications.RaiseItemAdded(this, itemVersionUri, parent.ItemUri);
        }

        private void LoadInsertOptions([NotNull] MenuItem menuItem, [NotNull] IEnumerable<ItemHeader> insertOptions)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(insertOptions, nameof(insertOptions));

            var index = -1;

            var loading = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (loading != null)
            {
                index = menuItem.Items.IndexOf(loading);

                menuItem.Items.RemoveAt(index);
            }

            if (insertOptions.Any())
            {
                // menuItem.Items.RemoveAt(index);
                foreach (var insertOption in insertOptions.Reverse())
                {
                    var item = new MenuItem
                    {
                        Header = insertOption.Name,
                        Tag = new TemplateHeader(insertOption.ItemUri, insertOption.Name, insertOption.Icon.IconPath, insertOption.Path, insertOption.ParentName, false)
                    };

                    item.Click += CreateItem;

                    if (index < 0)
                    {
                        menuItem.Items.Add(item);
                    }
                    else
                    {
                        menuItem.Items.Insert(index, item);
                    }
                }
            }

            /*
      if (this.Context == null)
      {
        return;
      }

      var itemUri = this.Context.SelectedItems.FirstOrDefault() as IItemUri;
      if (itemUri == null)
      {
        return;
      }

      index += insertOptions.Count() + 1;

      var recent = RecentTemplateManager.GetTemplates(itemUri.ItemUri.DatabaseUri);

      if (recent.Any() && index >= 0)
      {
        menuItem.Items.Insert(index, new Separator());
      }

      foreach (var templateHeader in recent.Reverse())
      {
        var item = new MenuItem
          {
            Header = templateHeader.Name,
            Tag = templateHeader
          };

        item.Click += this.CreateItem;

        if (index < 0)
        {
          menuItem.Items.Add(item);
        }
        else
        {
          menuItem.Items.Insert(index, item);
        }
      }
      */
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = Context;
            if (context == null)
            {
                return;
            }

            var parent = context.SelectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (parent == null)
            {
                return;
            }

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var item = menuItem.Items.OfType<MenuItem>().FirstOrDefault(i => i.Tag as string == @"loading");
            if (item == null)
            {
                return;
            }

            var index = menuItem.Items.IndexOf(item);
            menuItem.Items.Insert(index + 1, new Separator());

            parent.ItemUri.Site.DataService.GetInsertOptions(parent.ItemUri, options => LoadInsertOptions(menuItem, options));
        }
    }
}
