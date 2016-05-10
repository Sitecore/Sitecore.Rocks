// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.VisualStudio.Shell;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.KeyboardSchemes;

namespace Sitecore.Rocks.Options
{
    [Guid(@"155e39e9-3f13-4a97-a44f-b44b87bacbe3")]
    public class KeyboardSchemeOptions : DialogPage
    {
        [NotNull]
        protected override IWin32Window Window
        {
            get
            {
                var page = new KeyboardSchemeOptionPage();

                var host = new ElementHost
                {
                    Child = page
                };

                return host;
            }
        }
    }
}
