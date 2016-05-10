// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class LoadManyItemsDialog
    {
        public LoadManyItemsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        public int Count { get; set; }

        public static int Execute(int count)
        {
            var dialog = new LoadManyItemsDialog();
            dialog.Initialize(count);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return -1;
            }

            return dialog.Count;
        }

        public void Initialize(int count)
        {
            Count = count;

            Message.Text = string.Format("Hey, you are about to load {0} items.\n\nVisual Studio will get grumpy and unresponsive, if you load all.\n\nAre you sure you want to continue?", count);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var source = sender as FrameworkElement;
            if (source != null)
            {
                var c = int.Parse(source.Tag as string);
                if (c > 0)
                {
                    Count = c;
                }
            }

            this.Close(true);
        }
    }
}
