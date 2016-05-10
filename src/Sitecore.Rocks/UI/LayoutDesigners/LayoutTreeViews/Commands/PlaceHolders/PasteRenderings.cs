// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Commands.Clipboard;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Commands.PlaceHolders
{
    [Command(Submenu = ClipboardSubmenu.Name)]
    public class PasteRenderings : CommandBase
    {
        public PasteRenderings()
        {
            Text = "Paste";
            Group = "Edit";
            SortingValue = 4100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutTreeViewContext;
            if (context == null)
            {
                return false;
            }

            if (context.TreeViewItems.Count() != 1)
            {
                return false;
            }

            var placeHolderTreeViewItem = context.TreeViewItems.FirstOrDefault() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return false;
            }

            if (!Clipboard.ContainsText())
            {
                return false;
            }

            string text;
            try
            {
                text = Clipboard.GetText();
            }
            catch
            {
                return false;
            }

            return text.StartsWith(@"Sitecore.Clipboard.Renderings:");
        }

        public override void Execute(object parameter)
        {
            var context = parameter as LayoutTreeViewContext;
            if (context == null)
            {
                return;
            }

            var placeHolderTreeViewItem = context.TreeViewItems.FirstOrDefault() as PlaceHolderTreeViewItem;
            if (placeHolderTreeViewItem == null)
            {
                return;
            }

            if (!Clipboard.ContainsText())
            {
                return;
            }

            string text;
            try
            {
                text = Clipboard.GetText();
            }
            catch
            {
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var device = placeHolderTreeViewItem.DeviceTreeViewItem.Device;

            var items = GetItems(device, device.DatabaseUri, text);
            if (items == null)
            {
                return;
            }

            foreach (var rendering in items)
            {
                placeHolderTreeViewItem.DeviceTreeViewItem.AddRendering(placeHolderTreeViewItem, rendering, -1, -1);
            }

            device.PageModel.RaiseModified();
        }

        [CanBeNull]
        public List<RenderingItem> GetItems([NotNull] IRenderingContainer container, [NotNull] DatabaseUri databaseUri, [NotNull] string text)
        {
            Assert.ArgumentNotNull(container, nameof(container));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(text, nameof(text));

            if (text.StartsWith(@"Sitecore.Clipboard.Renderings:"))
            {
                text = text.Mid(30);
            }

            var root = text.ToXElement();
            if (root == null)
            {
                return null;
            }

            return root.Elements().Select(element => new RenderingItem(container, databaseUri, element)).ToList();
        }
    }
}
