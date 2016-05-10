// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors.Skins.Default
{
    public partial class Section : IContextProvider, IItemUri
    {
        public const string RegistryPath = "ContentEditor\\Sections";

        private Icon icon = Icon.Empty;

        public Section()
        {
            InitializeComponent();
            ExpandedByDefault = true;

            Notifications.RegisterFieldEvents(this, FieldChanged);
        }

        [NotNull]
        public UIElementCollection Children => FieldPanel.Children;

        [NotNull]
        public ContentEditor ContentEditor { get; set; }

        public bool ExpandedByDefault { get; set; }

        [NotNull]
        public Icon Icon
        {
            get { return icon; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                icon = value;
                IconImage.Source = value.GetSource();
            }
        }

        public ItemUri ItemUri { get; set; }

        [NotNull]
        public string Text
        {
            get { return HeaderTextBlock.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                HeaderTextBlock.Text = value;
                Expander.IsExpanded = GetExpanderState();
            }
        }

        [NotNull]
        public object GetContext()
        {
            var itemUri = ItemUri;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (itemUri == null)
            {
                itemUri = ItemUri.Empty;
            }

            return new ContentEditorSectionContext(ContentEditor, itemUri, Text, Icon);
        }

        private void FieldChanged([NotNull] object sender, [NotNull] FieldUri fieldUri, [CanBeNull] string newValue)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(fieldUri, nameof(fieldUri));

            if (fieldUri.ItemVersionUri.ItemUri != ItemUri)
            {
                return;
            }

            if (fieldUri.FieldId == FieldIds.Icon && !string.IsNullOrEmpty(newValue))
            {
                var path = @"/sitecore/shell/~/icon/" + newValue;
                Icon = new Icon(fieldUri.Site, path);
            }
        }

        private bool GetExpanderState()
        {
            var state = (string)AppHost.Settings.Get(RegistryPath, Text, @"2");

            if (state == "2" || state == null)
            {
                return ExpandedByDefault;
            }

            return state == @"1";
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ExpanderBorder.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void SetExpanderState([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Set(RegistryPath, Text, Expander.IsExpanded ? @"1" : @"0");
            Focus();
            e.Handled = true;
        }
    }
}
