// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.ContentTrees.Pipelines.DragCopy;
using Sitecore.Rocks.ContentTrees.Pipelines.DragMove;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.ContentTrees.Commands.Editing
{
    [Command(Submenu = ClipboardSubmenu.Name), CommandId(CommandIds.SitecoreExplorer.PasteFromClipboard, typeof(ContentTreeContext))]
    public class PasteFromClipboard : CommandBase
    {
        public PasteFromClipboard()
        {
            Text = Resources.PasteFromClipboard_PasteFromClipboard_Paste;
            Group = "Edit";
            SortingValue = 2200;
            Icon = new Icon("Resources/16x16/paste.png");
        }

        protected bool IsCopy { get; set; }

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

            if (!text.StartsWith(@"Sitecore.Clipboard.Copy") && !text.StartsWith(@"Sitecore.Clipboard.Cut"))
            {
                return false;
            }

            var item = context.SelectedItems.First() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            var items = GetItems(text);
            if (items == null)
            {
                return false;
            }

            foreach (var pasteItem in items)
            {
                if (pasteItem.ItemUri.Site.Name != item.ItemUri.Site.Name)
                {
                    return false;
                }

                if (pasteItem.ItemUri.DatabaseName.Name != item.ItemUri.DatabaseName.Name)
                {
                    return false;
                }
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

            if (!Clipboard.ContainsText())
            {
                return;
            }

            var item = context.SelectedItems.First() as ItemTreeViewItem;
            if (item == null)
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

            var items = GetItems(text);
            if (items == null)
            {
                return;
            }

            if (IsCopy)
            {
                var pipeline = PipelineManager.GetPipeline<DragCopyPipeline>();

                pipeline.KeyStates = DragDropKeyStates.None;
                pipeline.Target = item;
                pipeline.Items = items;
                pipeline.Position = ControlDragAdornerPosition.Over;
                pipeline.Confirm = false;

                pipeline.Start();
            }
            else
            {
                var pipeline = PipelineManager.GetPipeline<DragMovePipeline>();

                pipeline.KeyStates = DragDropKeyStates.None;
                pipeline.Target = item;
                pipeline.Items = items;
                pipeline.Position = ControlDragAdornerPosition.Over;
                pipeline.Confirm = false;

                pipeline.Start();

                Clipboard.Clear();
            }
        }

        [CanBeNull]
        public List<PasteItem> GetItems([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            if (text.StartsWith(@"Sitecore.Clipboard.Copy:"))
            {
                IsCopy = true;
                text = text.Mid(24);
            }
            else
            {
                IsCopy = false;
                text = text.Mid(23);
            }

            var result = new List<PasteItem>();

            var parts = text.Split(';');

            foreach (var part in parts)
            {
                var p = part.Split('/');

                var site = SiteManager.GetSite(p[0]);
                if (site == null)
                {
                    return null;
                }

                var databaseUri = new DatabaseUri(site, new DatabaseName(p[1]));

                Guid guid;
                if (!Guid.TryParse(p[2], out guid))
                {
                    return null;
                }

                var itemUri = new ItemUri(databaseUri, new ItemId(guid));

                var name = p[3].Replace(@"&slash", @"/");
                var icon = p[4].Replace(@"&slash", @"/");

                var pasteItem = new PasteItem
                {
                    ItemUri = itemUri,
                    Name = name,
                    Icon = new Icon(site, icon)
                };

                result.Add(pasteItem);
            }

            return result;
        }

        public class PasteItem : IItem
        {
            public Icon Icon { get; set; }

            public ItemUri ItemUri { get; set; }

            public string Name { get; set; }
        }
    }
}
