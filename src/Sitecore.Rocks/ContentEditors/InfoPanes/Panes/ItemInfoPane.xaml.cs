// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Commands.Tasks;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.ContentEditors.InfoPanes.Panes
{
    [InfoPane("Item", 1000)]
    public partial class ItemInfoPane : IInfoPane
    {
        public ItemInfoPane()
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

            if (contentModel.IsEmpty)
            {
                return false;
            }

            return true;
        }

        public object GetHeader()
        {
            var icon = new Icon("Resources/16x16/home.png");

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

                if (contentModel.IsMultiple)
                {
                    RenderMultipleItems(contentModel);
                    return this;
                }

                RenderItem(contentModel.Items.First());

                return this;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void IconMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            var context = new ContentEditorContext(ContentEditor);

            var command = new SetIcon();
            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void NavigateCloneSource([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ContentEditor.ContentModel.IsEmpty)
            {
                return;
            }

            var item = ContentEditor.ContentModel.FirstItem;

            var source = item.Source;
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            var parts = source.Split('|');
            var databaseUri = new DatabaseUri(item.ItemUri.Site, new DatabaseName(parts[0]));
            var itemId = new ItemId(new Guid(parts[1]));

            var itemUri = new ItemUri(databaseUri, itemId);

            AppHost.OpenContentEditor(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Data.Version.Latest));
        }

        private void NavigateTemplate([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ContentEditor.ContentModel.IsEmpty)
            {
                return;
            }

            var item = ContentEditor.ContentModel.FirstItem;

            AppHost.Windows.Factory.OpenTemplateDesigner(new ItemUri(item.ItemUri.DatabaseUri, item.TemplateId));
        }

        private void RenderItem([NotNull] Item item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            ItemId.Value = item.ItemUri.ItemId.ToString();
            ItemName.Text = item.Name;
            Path.Value = item.GetPath();
            QuickInfoIcon.Source = item.Icon.GetSource();

            DefaultTemplateId.Value = item.TemplateId.ToString();
            DefaultTemplateName.Value = item.TemplateName;

            var source = item.Source;
            var n = source.IndexOf("|", StringComparison.Ordinal);
            if (n >= 0)
            {
                var databaseName = source.Left(n);
                if (databaseName == item.ItemUri.DatabaseName.ToString())
                {
                    source = source.Mid(n + 1);
                }
                else
                {
                    source = source.Replace("|", ": ");
                }
            }

            if (string.IsNullOrEmpty(source))
            {
                source = "N/A";
            }

            CloneSource.Value = source;
        }

        private void RenderMultipleItems([NotNull] ContentModel contentModel)
        {
            Debug.ArgumentNotNull(contentModel, nameof(contentModel));

            ItemId.Value = string.Empty;
            ItemName.Text = string.Empty;
            DefaultTemplateId.Value = string.Empty;
            DefaultTemplateName.Value = string.Empty;
            CloneSource.Value = string.Empty;

            var item = contentModel.Items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var templateName = item.TemplateName;
            if (!contentModel.Items.Select(i => i.TemplateName).All(t => t != null && t == templateName))
            {
                return;
            }

            DefaultTemplateId.Value = item.TemplateId.ToString();
            DefaultTemplateName.Value = item.TemplateName;
        }
    }
}
