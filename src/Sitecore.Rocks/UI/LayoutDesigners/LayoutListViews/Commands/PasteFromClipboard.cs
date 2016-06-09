// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
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
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.LayoutListViews.Commands
{
    [Command(Submenu = ClipboardSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 1004, "Home", "Editing", Icon = "Resources/16x16/paste.png", ElementType = RibbonElementType.SmallButton)]
    public class PasteFromClipboard : CommandBase, IDynamicToolbarElement
    {
        public PasteFromClipboard()
        {
            Text = "Paste";
            Group = "Edit";
            SortingValue = 4100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return false;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
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
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            if (!Clipboard.ContainsText())
            {
                return;
            }

            var tabsLayoutDesignerView = context.LayoutDesigner.LayoutDesignerView as LayoutListView;
            if (tabsLayoutDesignerView == null)
            {
                return;
            }

            var layoutListView = tabsLayoutDesignerView.GetActiveListView();
            if (layoutListView == null)
            {
                return;
            }

            var itemUri = context.LayoutDesigner.FieldUris.First().ItemVersionUri.ItemUri;

            var index = -1;
            var item = context.SelectedItem;
            if (item != null)
            {
                index = layoutListView.List.Items.IndexOf(item);
                if (index < 0)
                {
                    index = -1;
                }
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

            var items = GetItems(layoutListView, itemUri.DatabaseUri, text);
            if (items == null)
            {
                return;
            }

            /*
            if (index >= 0)
            {
                items.Reverse();
            }
            */

            // patch unique ids
            var renderingContainer = context.LayoutDesigner.LayoutDesignerView.GetRenderingContainer();
            foreach (var renderingItem in items)
            {
                if (string.IsNullOrEmpty(renderingItem.UniqueId))
                {
                    renderingItem.UniqueId = Guid.NewGuid().ToString("B").ToUpperInvariant();
                    continue;
                }

                if (renderingContainer.Renderings.Any(r => r.UniqueId == renderingItem.UniqueId))
                {
                    renderingItem.UniqueId = Guid.NewGuid().ToString("B").ToUpperInvariant();
                }
            }

            foreach (var renderingItem in items)
            {
                var property = renderingItem.DynamicProperties.FirstOrDefault(p => string.Compare(p.Name, @"Id", StringComparison.InvariantCultureIgnoreCase) == 0);
                if (property != null)
                {
                    var controlId = property.Value?.ToString() ?? string.Empty;
                    property.Value = renderingItem.GetNewControlId(controlId);
                }

                layoutListView.AddRendering(renderingItem, index, index);
                if (index >= 0)
                {
                    index++;
                }
            }

            layoutListView.NoItems.Visibility = Visibility.Collapsed;
            layoutListView.ListContextMenu.Visibility = Visibility.Visible;

            context.LayoutDesigner.Modified = true;
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

            return root.Elements().Select(element => new RenderingItem(container, databaseUri, element, true)).ToList();
        }

        bool IDynamicToolbarElement.CanRender(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            return context != null && context.LayoutDesigner.LayoutDesignerView is LayoutListView;
        }
    }
}
