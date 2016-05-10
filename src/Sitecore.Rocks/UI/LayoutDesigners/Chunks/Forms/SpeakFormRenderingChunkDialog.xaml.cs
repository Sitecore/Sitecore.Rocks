// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks.Forms
{
    public partial class SpeakFormRenderingChunkDialog
    {
        public SpeakFormRenderingChunkDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            DatabaseUri = DatabaseUri.Empty;
            ItemTextBox.Text = AppHost.Settings.GetString("LayoutDesigners\\SpeakFormGenerator", "LastItem", string.Empty);
        }

        [NotNull]
        public DatabaseUri DatabaseUri { get; set; }

        [NotNull]
        public IRenderingContainer RenderingContainer { get; set; }

        [CanBeNull]
        public IEnumerable<RenderingItem> Renderings { get; private set; }

        [NotNull]
        private string SelectedItem => ItemTextBox.Text ?? string.Empty;

        public bool ShowModal()
        {
            Assert.IsFalse(DatabaseUri == DatabaseUri.Empty, "DatabaseUri cannot be Empty.");
            Assert.IsNotNull(DatabaseUri, "DatabaseUri cannot be null");
            Assert.IsNotNull(RenderingContainer, "RenderingContainer cannot be null");

            return AppHost.Shell.ShowDialog(this) == true;
        }

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var dialog = new SelectItemDialog()
            {
                DatabaseUri = DatabaseUri
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            dialog.GetSelectedItemPath(s => ItemTextBox.Text = s);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ExecuteCompleted getItemIdcompleted = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var renderings = new List<RenderingItem>();

                var root = response.ToXElement();
                if (root != null)
                {
                    var layoutElement = root.Element("layout");
                    if (layoutElement != null)
                    {
                        foreach (var deviceElement in layoutElement.Elements())
                        {
                            if (!deviceElement.HasElements)
                            {
                                continue;
                            }

                            foreach (var element in deviceElement.Elements())
                            {
                                renderings.Add(new RenderingItem(RenderingContainer, DatabaseUri, element));
                            }

                            break;
                        }
                    }
                }

                Renderings = renderings;

                this.Close(true);
            };

            AppHost.Server.Forms.GetFormRenderings(DatabaseUri, SelectedItem, string.Empty, getItemIdcompleted);

            IsEnabled = false;

            AppHost.Settings.SetString("LayoutDesigners\\SpeakFormGenerator", "LastItem", ItemTextBox.Text);
        }
    }
}
