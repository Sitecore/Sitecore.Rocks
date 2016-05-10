// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.ContentEditors.InfoPanes.Panes
{
    [InfoPane("Url", 2000), Feature(FeatureNames.AdvancedNavigation)]
    public partial class UrlInfoPane : IInfoPane, ICanActivateInfoPane
    {
        public UrlInfoPane()
        {
            InitializeComponent();
        }

        [NotNull]
        protected ContentEditor ContentEditor { get; set; }

        protected bool IsActivated { get; set; }

        public void Activate()
        {
            if (IsActivated)
            {
                return;
            }

            IsActivated = true;

            Refresh();
        }

        public bool CanRender(ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            var contentModel = contentEditor.ContentModel;
            if (!contentModel.IsSingle)
            {
                return false;
            }

            return true;
        }

        public object GetHeader()
        {
            var icon = new Icon("Resources/16x16/link.png");

            var image = new Image
            {
                Source = icon.GetSource(),
                SnapsToDevicePixels = true,
                Width = 16,
                Height = 16
            };

            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

            return image;
        }

        public FrameworkElement Render(ContentEditor contentEditor)
        {
            Assert.ArgumentNotNull(contentEditor, nameof(contentEditor));

            ContentEditor = contentEditor;

            return this;
        }

        private void Open([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var textBlock = sender as InfoPaneTextBlock;
            if (textBlock == null)
            {
                return;
            }

            if (ContentEditor.ContentModel.IsEmpty)
            {
                return;
            }

            var url = textBlock.Value;

            var item = ContentEditor.ContentModel.FirstItem;

            AppHost.Browsers.Navigate(item.ItemUri.Site, url);
        }

        private void Refresh()
        {
            if (ContentEditor.ContentModel.IsEmpty)
            {
                Links.Children.Clear();
                return;
            }

            Loading.ShowLoading(Links);

            var item = ContentEditor.ContentModel.FirstItem;

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                Loading.HideLoading(Links);

                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                RenderLinks(root);
            };

            item.ItemUri.Site.DataService.ExecuteAsync("Items.GetItemUrls", completed, item.ItemUri.DatabaseName.ToString(), item.ItemUri.ItemId.ToString());
        }

        private void RenderLinks([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            Links.Children.Clear();

            foreach (var element in root.Elements())
            {
                var text = element.Value;
                var url = element.GetAttributeValue("url");

                var textBlock = new InfoPaneTextBlock
                {
                    Header = text,
                    Value = url
                };

                textBlock.Click += Open;

                Links.Children.Add(textBlock);
            }
        }
    }
}
