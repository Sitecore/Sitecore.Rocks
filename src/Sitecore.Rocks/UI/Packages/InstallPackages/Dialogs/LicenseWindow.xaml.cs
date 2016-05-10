// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.Packages.InstallPackages.Dialogs
{
    public partial class LicenseWindow
    {
        public LicenseWindow()
        {
            InitializeComponent();
            this.InitializeDialog();
        }

        [NotNull]
        protected string License { get; set; }

        public void Initialize([NotNull] string license)
        {
            Assert.ArgumentNotNull(license, nameof(license));

            Frame.NavigateToString(license);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
