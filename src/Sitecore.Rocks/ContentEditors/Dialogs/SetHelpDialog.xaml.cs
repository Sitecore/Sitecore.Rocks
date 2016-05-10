// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentEditors.Dialogs
{
    public partial class SetHelpDialog
    {
        public SetHelpDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        public void Initialize([NotNull] string shortHelp, [NotNull] string longHelp)
        {
            Assert.ArgumentNotNull(shortHelp, nameof(shortHelp));
            Assert.ArgumentNotNull(longHelp, nameof(longHelp));

            ShortHelp.Text = shortHelp;
            LongHelp.Text = longHelp;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;
            Keyboard.Focus(ShortHelp);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(true);
        }
    }
}
