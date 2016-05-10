// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectFileDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.PageCodeDesigners
{
    public partial class PageCodeDesignerControl : IDesigner
    {
        private bool activated;

        public PageCodeDesignerControl([NotNull] PageModel pageModel, [NotNull] RenderingItem rendering)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));
            Assert.ArgumentNotNull(rendering, nameof(rendering));

            InitializeComponent();

            PageModel = pageModel;
            Rendering = rendering;

            PageCodeTextBox.Text = Rendering.GetParameterValue("PageCodeScriptFileName");

            Rendering.PropertyChanged += UpdatePageCode;
        }

        [NotNull]
        public PageModel PageModel { get; private set; }

        [NotNull]
        public RenderingItem Rendering { get; }

        public void Activate()
        {
            if (activated)
            {
                return;
            }

            activated = true;

            UpdatePageCode();
        }

        public void Close()
        {
            Rendering.PropertyChanged -= UpdatePageCode;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectFileDialog
            {
                Site = Rendering.ItemUri.Site
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            var fileName = dialog.SelectedFilePath.Replace("\\", "/");
            Rendering.SetParameterValue("PageCodeScriptFileName", fileName);
            PageCodeTextBox.Text = fileName;
        }

        private void Create([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fileName = "PageCode.js";

            var folder = Rendering.ItemUri.Site.WebRootPath;
            var dialog = new SaveFileDialog
            {
                Title = "Create Page Code",
                CheckPathExists = true,
                OverwritePrompt = true,
                InitialDirectory = folder,
                FileName = fileName,
                Filter = "JavaScript|*.js|All files|*.*"
            };

            bool retry;
            do
            {
                retry = false;

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (!dialog.FileName.StartsWith(folder))
                {
                    AppHost.MessageBox("The PageCode must be located inside the web site.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    retry = true;
                }
            }
            while (retry);

            fileName = dialog.FileName;

            var text = string.Empty;
            using (var stream = typeof(ItemUri).Assembly.GetManifestResourceStream(@"Sitecore.Rocks.Resources.PageCode.js"))
            {
                if (stream != null)
                {
                    var reader = new StreamReader(stream);
                    text = reader.ReadToEnd();
                }
            }

            AppHost.Files.WriteAllText(fileName, text, Encoding.UTF8);

            fileName = fileName.Mid(folder.Length).Replace('\\', '/');
            Rendering.SetParameterValue("PageCodeScriptFileName", fileName);
            PageCodeTextBox.Text = fileName;
        }

        private void Edit([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            AppHost.Files.OpenFile(fileName);
        }

        private void EnableButtons()
        {
            var fileName = GetFileName();
            var exists = !string.IsNullOrEmpty(fileName) && File.Exists(fileName);

            EditPageCode.IsEnabled = exists;
            CreatePageCode.IsEnabled = !exists;
        }

        [NotNull]
        private string GetFileName()
        {
            var path = Rendering.GetParameterValue("PageCodeScriptFileName");
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = IO.File.Normalize(path);

            if (path.StartsWith("\\"))
            {
                path = path.Mid(1);
            }

            return Path.Combine(Rendering.ItemUri.Site.WebRootPath, path);
        }

        private void SetPageCode([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Rendering.SetParameterValue("PageCodeScriptFileName", PageCodeTextBox.Text);
        }

        private void UpdatePageCode([NotNull] object sender, [NotNull] PropertyChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.PropertyName != "PageCodeScriptFileName")
            {
                return;
            }

            UpdatePageCode();
        }

        private void UpdatePageCode()
        {
            PageCodeTextBox.Text = Rendering.GetParameterValue("PageCodeScriptFileName");
            EnableButtons();
        }
    }
}
