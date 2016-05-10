// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    public partial class UpdateDialog
    {
        public UpdateDialog()
        {
            InitializeComponent();
            this.InitializeDialog();
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
        public string UpdateText
        {
            get { return Message.Text ?? string.Empty; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Message.Text = value;
            }
        }

        private void CancelClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.Options.HideUpdateDialog = DontShowAgain.IsChecked == true;
            AppHost.Settings.Options.Save();

            this.Close(true);
        }
    }
}
