// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.TransferItems
{
    public partial class TransferDialog
    {
        public TransferDialog([NotNull] ItemTreeViewItem target, [NotNull] IEnumerable<IItem> items, bool deep, bool changeIds)
        {
            Assert.ArgumentNotNull(target, nameof(target));
            Assert.ArgumentNotNull(items, nameof(items));

            InitializeComponent();
            this.InitializeDialog();

            Target = target;
            Items = items;
            Deep = deep;
            ChangeIds = changeIds;
            Index = 0;

            ProgressBar.Maximum = items.Count();

            Loaded += ControlLoaded;
        }

        public bool ChangeIds { get; }

        public int Index { get; set; }

        protected bool Deep { get; set; }

        protected bool IsCancelled { get; set; }

        [NotNull]
        protected IEnumerable<IItem> Items { get; set; }

        [NotNull]
        protected ItemTreeViewItem Target { get; set; }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            IsCancelled = true;

            this.Close(true);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            CopyNext();
        }

        private void CopyNext()
        {
            if (IsCancelled || Index >= Items.Count())
            {
                Target.Refresh();
                this.Close(true);
                return;
            }

            var item = Items.ElementAt(Index);

            GetValueCompleted<string> completed = delegate(string value)
            {
                Target.ItemUri.Site.DataService.PasteXml(Target.ItemUri, value, ChangeIds);
                Index++;

                CopyNext();
            };

            item.ItemUri.Site.DataService.GetItemXmlAsync(item.ItemUri, Deep, completed);
        }
    }
}
