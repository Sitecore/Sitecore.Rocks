// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentEditors.Dialogs
{
    public partial class ChangeTemplateDialog
    {
        public ChangeTemplateDialog(DatabaseUri databaseUri, ItemId templateId, string templateName)
        {
            InitializeComponent();
            this.InitializeDialog();

            DatabaseUri = databaseUri;
            TemplateId = templateId;
            TemplateName = templateName;

            if (!string.IsNullOrEmpty(TemplateName))
            {
                TemplateNameTextBlock.Visibility = Visibility.Visible;
                TemplateNameTextBlock.Text = "Current Template: " + TemplateName;
            }

            TemplateSelector.InitialTemplateId = TemplateId;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public DatabaseUri DatabaseUri
        {
            get { return TemplateSelector.DatabaseUri; }

            private set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                TemplateSelector.DatabaseUri = value;
            }
        }

        [CanBeNull]
        public TemplateHeader SelectedTemplate
        {
            get { return TemplateSelector.SelectedTemplate; }
        }

        [NotNull]
        public ItemId TemplateId { get; }

        [NotNull]
        public string TemplateName { get; }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            EnableButtons();
        }

        private void EnableButtons()
        {
            OK.IsEnabled = TemplateSelector.SelectedTemplate != null;
        }

        private void HandleDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            OkClick(sender, e);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedTemplate = SelectedTemplate;
            if (selectedTemplate != null)
            {
                TemplateSelector.AddToRecent(selectedTemplate);
            }

            this.Close(true);
        }

        private void TemplateSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EnableButtons();
        }
    }
}
