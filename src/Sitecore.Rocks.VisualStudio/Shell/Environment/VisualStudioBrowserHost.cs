// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioBrowserHost : BrowserHost
    {
        public override void NavigateInternalBrowser(string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            var service = Package.GetGlobalService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            if (service == null)
            {
                Navigate(url);
                return;
            }

            IVsWindowFrame frame;
            service.Navigate(url, 0, out frame);
        }
    }
}
