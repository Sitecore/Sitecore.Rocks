// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters.BindModeDialogs
{
    public partial class BindModeDialog
    {
        public BindModeDialog([NotNull] string bindMode)
        {
            Assert.ArgumentNotNull(bindMode, nameof(bindMode));

            InitializeComponent();
            this.InitializeDialog();

            BindMode = bindMode;

            switch (BindMode.ToLowerInvariant())
            {
                case "readwrite":
                    ReadWriteRadionButton.IsChecked = true;
                    break;
                case "read":
                    ReadRadionButton.IsChecked = true;
                    break;
                case "write":
                    WriteRadionButton.IsChecked = true;
                    break;
                case "server":
                    ServerRadionButton.IsChecked = true;
                    break;
            }
        }

        [NotNull]
        public string BindMode { get; set; }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (ReadWriteRadionButton.IsChecked == true)
            {
                BindMode = "readwrite";
            }
            else if (ReadRadionButton.IsChecked == true)
            {
                BindMode = "read";
            }
            else if (WriteRadionButton.IsChecked == true)
            {
                BindMode = "write";
            }
            else if (ServerRadionButton.IsChecked == true)
            {
                BindMode = "server";
            }

            this.Close(true);
        }
    }
}
