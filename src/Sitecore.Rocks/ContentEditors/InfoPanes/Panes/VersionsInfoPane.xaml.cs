// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.InfoPanes.Panes
{
    [InfoPane("Versions", 1010)]
    public partial class VersionsInfoPane : IInfoPane
    {
        public VersionsInfoPane()
        {
            InitializeComponent();
        }

        [NotNull]
        protected ContentEditor ContentEditor { get; set; }

        protected bool IsLoading { get; set; }

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
            var icon = new Icon("Resources/16x16/versions.png");

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

            IsLoading = true;
            try
            {
                ContentEditor = contentEditor;

                var contentModel = contentEditor.ContentModel;

                RenderItem(contentModel.Items.First());

                return this;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void RenderItem([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            RenderLanguages(item);
            RenderVersions(item);
        }

        private void RenderLanguages([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var languages = item.Languages;

            foreach (var language in languages.OrderBy(l => l))
            {
                var listBoxItem = new ListBoxItem
                {
                    Content = language,
                    Tag = language,
                    IsSelected = language == item.Uri.Language.Name
                };

                Languages.Items.Add(listBoxItem);
            }
        }

        private void RenderVersions([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var versions = item.Versions;

            for (var index = versions.Count - 1; index >= 0; index--)
            {
                var version = versions[index];

                var listBoxItem = new ListBoxItem
                {
                    Content = version,
                    Tag = version,
                    IsSelected = version == item.Uri.Version.Number
                };

                Versions.Items.Add(listBoxItem);
            }
        }

        private void SetLanguage([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (IsLoading)
            {
                return;
            }

            var listBoxItem = Languages.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var language = listBoxItem.Tag as string;
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            var contentModel = ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            if (contentModel.IsMultiple)
            {
                return;
            }

            LanguageManager.CurrentLanguage = new Language(language);

            var list = new List<ItemVersionUri>
            {
                new ItemVersionUri(contentModel.FirstItem.Uri.ItemUri, new Language(language), Version.Latest)
            };

            ContentEditor.LoadItems(list, new LoadItemsOptions(true));
        }

        private void SetVersion([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (IsLoading)
            {
                return;
            }

            var listBoxItem = Versions.SelectedItem as ListBoxItem;
            if (listBoxItem == null)
            {
                return;
            }

            var contentModel = ContentEditor.ContentModel;
            if (contentModel.IsEmpty)
            {
                return;
            }

            if (contentModel.IsMultiple)
            {
                return;
            }

            var version = (int)listBoxItem.Tag;

            var list = new List<ItemVersionUri>
            {
                new ItemVersionUri(contentModel.FirstItem.Uri.ItemUri, LanguageManager.CurrentLanguage, new Version(version))
            };

            ContentEditor.LoadItems(list, new LoadItemsOptions(true));
        }
    }
}
