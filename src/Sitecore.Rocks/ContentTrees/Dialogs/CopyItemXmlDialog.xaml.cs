// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs
{
    public partial class CopyItemXmlDialog
    {
        public CopyItemXmlDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        public ItemUri ItemUri { get; set; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadXml();
            EnableButtons();
        }

        private void CopyClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Clipboard.SetText(Xml.Text);
        }

        private void EnableButtons()
        {
            Copy.IsEnabled = Xml.Visibility == Visibility.Visible;
            Save.IsEnabled = Xml.Visibility == Visibility.Visible;
        }

        private void GetItemXmlCallback([NotNull] string xml)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));

            Dispatcher.Invoke(new Action(delegate
            {
                Xml.Text = xml;
                Loading.Visibility = Visibility.Collapsed;
                Xml.Visibility = Visibility.Visible;
                EnableButtons();
            }));
        }

        private void IncludeSubItemsChecked([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!IncludeSubItems.IsLoaded)
            {
                return;
            }

            Loading.Visibility = Visibility.Visible;
            Xml.Visibility = Visibility.Collapsed;

            LoadXml();

            EnableButtons();
        }

        private void LoadXml()
        {
            ItemUri.Site.DataService.GetItemXmlAsync(ItemUri, IncludeSubItems.IsChecked == true, GetItemXmlCallback);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void SaveClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SaveFileDialog
            {
                Title = Rocks.Resources.CopyItemXmlDialog_SaveClick_Save_Item_Xml_to_File,
                CheckPathExists = true,
                OverwritePrompt = true,
                FileName = @"Item",
                DefaultExt = @".xml",
                Filter = string.Format(@"{0} (.xml)|*.xml|{1}|*.*", Rocks.Resources.CopyItemXmlDialog_SaveClick_Xml_documents, Rocks.Resources.All_files)
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            using (var sw = new StreamWriter(dialog.FileName))
            {
                sw.Write(Xml.Text);
            }
        }
    }
}
