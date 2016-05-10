// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class PasteItemXmlDialog
    {
        public PasteItemXmlDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        private void LoadClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new OpenFileDialog
            {
                Title = Rocks.Resources.PasteItemXmlDialog_LoadClick_Load_Item_Xml_from_File,
                CheckFileExists = true,
                DefaultExt = @".xml",
                Filter = @"Xml documents|*.xml|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using (var sw = new StreamReader(dialog.FileName))
            {
                Xml.Text = sw.ReadToEnd();
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!string.IsNullOrEmpty(Xml.Text))
            {
                ItemUri.Site.DataService.PasteXml(ItemUri, Xml.Text, ChangeIds.IsChecked == true);

                Notifications.RaiseItemlXmlPasted(this, ItemUri, Xml.Text, ChangeIds.IsChecked == true);
            }

            Close();
        }

        private void PasteClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

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
                text = string.Empty;
            }

            Xml.Text = text;
        }
    }
}
