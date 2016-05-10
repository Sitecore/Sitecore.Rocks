// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class ClipboardHost
    {
        public void SetText([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            try
            {
                Clipboard.SetText(text);
            }
            catch (COMException)
            {
                AppHost.MessageBox("Oh, come on, Clipboard, why can't you be opened!\n\nSorry about this, can you try again?", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
