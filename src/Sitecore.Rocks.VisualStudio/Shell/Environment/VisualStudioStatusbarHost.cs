// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioStatusbarHost : StatusbarHost
    {
        public override void SetText(string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var statusBar = SitecorePackage.Instance.GetService<SVsStatusbar>() as IVsStatusbar;
            if (statusBar == null)
            {
                return;
            }

            int frozen;

            statusBar.IsFrozen(out frozen);

            if (frozen == 0)
            {
                statusBar.SetText(text);
            }
        }
    }
}
