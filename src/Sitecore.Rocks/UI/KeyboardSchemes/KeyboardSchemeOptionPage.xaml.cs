// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.KeyboardSchemes
{
    public partial class KeyboardSchemeOptionPage
    {
        public KeyboardSchemeOptionPage()
        {
            InitializeComponent();
        }

        private void OpenKeyboardSchemeDialog([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            var dialog = new KeyboardSchemeDialog();
            dialog.ShowDialog();
        }
    }
}
