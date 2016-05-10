// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs.BuildTasks
{
    public partial class BuildTaskDialog
    {
        public BuildTaskDialog([NotNull] Site site, [NotNull] string profileName)
        {
            Assert.ArgumentNotNull(profileName, nameof(profileName));
            Assert.ArgumentNotNull(site, nameof(site));

            InitializeComponent();
            this.InitializeDialog();

            Builder.Initialize(site, profileName);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
