// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites.Connections;

namespace Sitecore.Rocks.Sites.Dialogs
{
    public partial class EditConnectionDialog
    {
        private Connection connection;

        public EditConnectionDialog([NotNull] Connection connection)
        {
            Assert.ArgumentNotNull(connection, nameof(connection));

            InitializeComponent();
            this.InitializeDialog();

            Connection = connection;
        }

        [NotNull]
        public Connection Connection
        {
            get { return connection; }

            private set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                connection = value;
                EditRendering.Connection = connection;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
