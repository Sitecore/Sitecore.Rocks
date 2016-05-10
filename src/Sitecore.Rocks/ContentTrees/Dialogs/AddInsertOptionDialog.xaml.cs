// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class AddInsertOptionDialog
    {
        public AddInsertOptionDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string ItemName
        {
            get { return NewItemName.Text ?? string.Empty; }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();

            NewItemName.Focus();
            NewItemName.SelectAll();
            Keyboard.Focus(NewItemName);
        }

        private void EnableButtons()
        {
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
