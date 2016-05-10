// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Publishing.Dialogs
{
    public partial class PublishDialog
    {
        public PublishDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            JobViewer.IsChecked = AppHost.Settings.Options.ShowJobViewer;
        }

        [NotNull]
        public string Caption
        {
            get { return Title ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Title = value;
            }
        }

        [NotNull]
        public string PublishingText
        {
            get { return Text.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Text.Text = value;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Options.ShowJobViewer = JobViewer.IsChecked == true;
            AppHost.Settings.Options.HidePublishingDialog = DontShowAgain.IsChecked == true;
            AppHost.Settings.Options.Save();

            this.Close(true);
        }
    }
}
