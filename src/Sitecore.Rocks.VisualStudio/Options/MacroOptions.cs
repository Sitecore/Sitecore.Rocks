// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.Macros.Dialogs;

namespace Sitecore.Rocks.Options
{
    [Guid(@"72d87665-6e7f-458c-a67f-f06a11ebc09c")]
    public class MacroOptions : DialogPage
    {
        [NotNull]
        protected override IWin32Window Window
        {
            get
            {
                var page = new MacroEditor();

                var host = new ElementHost
                {
                    Child = page
                };

                return host;
            }
        }
    }
}
