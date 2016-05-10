// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.EditRenderingDialogs
{
    public partial class EditRenderingDialog
    {
        private object source;

        public EditRenderingDialog([NotNull] object source)
        {
            Assert.ArgumentNotNull(source, nameof(source));

            InitializeComponent();
            this.InitializeDialog();

            Source = source;
        }

        [NotNull]
        public object Source
        {
            get { return source; }

            private set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                source = value;
                EditRendering.Source = source;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }

        private void Update([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            EditRendering.Update();
        }
    }
}
