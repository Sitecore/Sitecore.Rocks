// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentEditors.Fields;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.ImageFields
{
    [Command]
    public class RecentMedia : CommandBase
    {
        public RecentMedia()
        {
            Text = Resources.RecentMedia_RecentMedia_Recent;
            Group = "Navigate";
            SortingValue = 100;
            SubmenuOpened = Opened;
        }

        [CanBeNull]
        protected ContentEditorFieldContext Context { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentEditorFieldContext;
            if (context == null)
            {
                return false;
            }

            var field = context.Field;
            if (!(field.Control is ImageField))
            {
                return false;
            }

            Context = context;

            return true;
        }

        public override void Execute(object parameter)
        {
        }

        private void LoadRecentMedia([NotNull] MenuItem menuItem, [NotNull] IEnumerable<ItemHeader> insertOptions)
        {
            Debug.ArgumentNotNull(menuItem, nameof(menuItem));
            Debug.ArgumentNotNull(insertOptions, nameof(insertOptions));

            menuItem.Items.Clear();

            if (!insertOptions.Any())
            {
                var item = new MenuItem
                {
                    Header = Resources.InsertOptions_LoadInsertOptions_none,
                    Foreground = SystemColors.GrayTextBrush
                };

                menuItem.Items.Add(item);

                return;
            }

            var count = 0;

            foreach (var insertOption in insertOptions)
            {
                var item = new MenuItem
                {
                    Header = insertOption.Name,
                    Tag = insertOption
                };

                item.Click += SetMedia;

                menuItem.Items.Add(item);

                count++;
                if (count > 8)
                {
                    break;
                }
            }
        }

        private void Opened([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            if (menuItem.Items.Count != 1)
            {
                return;
            }

            var m = menuItem.Items[0] as MenuItem;
            if (m == null || m.Tag as string != @"loading")
            {
                return;
            }

            if (Context == null)
            {
                return;
            }

            var to = DateTime.Now.AddDays(1).ToString(@"yyyyMMdd");
            var from = DateTime.Now.AddDays(-7).ToString(@"yyyyMMdd");
            var query = string.Format(@"[{0} TO {1}]", from, to);
            var fieldUri = Context.Field.FieldUris.First();
            var root = new ItemUri(fieldUri.ItemVersionUri.DatabaseUri, IdManager.GetItemId("/sitecore/media library"));

            GetItemsCompleted<ItemHeader> callback = items => LoadRecentMedia(menuItem, items);

            fieldUri.Site.DataService.SearchMedia(query, fieldUri.ItemVersionUri.DatabaseUri, "__updated", string.Empty, root, 0, callback);
        }

        private void SetMedia([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var menuItem = e.Source as MenuItem;
            if (menuItem == null)
            {
                return;
            }

            var itemHeader = menuItem.Tag as ItemHeader;
            if (itemHeader == null)
            {
                return;
            }

            if (Context == null)
            {
                return;
            }

            var imageField = Context.Field.Control as ImageField;
            if (imageField == null)
            {
                return;
            }

            imageField.MediaUri = itemHeader.ItemUri;
            imageField.UpdateValue();
            imageField.ReloadImage();
            imageField.SetModifiedFlag();
        }
    }
}
