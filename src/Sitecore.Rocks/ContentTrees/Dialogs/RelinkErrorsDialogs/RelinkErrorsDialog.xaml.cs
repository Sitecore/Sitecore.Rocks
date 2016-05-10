// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Dialogs.RelinkErrorsDialogs
{
    public partial class RelinkErrorsDialog
    {
        public RelinkErrorsDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            DataContext = this;
        }

        [CanBeNull]
        public IEnumerable<XElement> Elements { get; private set; }

        public void Initialize([NotNull] string response)
        {
            Assert.ArgumentNotNull(response, nameof(response));

            var root = response.ToXElement();
            if (root == null)
            {
                return;
            }

            Elements = root.Elements();
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
