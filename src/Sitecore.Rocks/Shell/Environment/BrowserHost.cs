// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using System.Web;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Environment
{
    public class BrowserHost
    {
        [NotNull]
        public virtual string GetUrl([NotNull] Site site, [NotNull] string path)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(path, nameof(path));

            /*
            if (!path.Contains(@"://"))
            {
                path = site.GetHost() + path;
            }
            */

            var url = site.GetHost() + @"/sitecore/shell/WebService/browse.aspx";

            url += @"?u=" + HttpUtility.UrlEncode(site.Credentials.UserName);
            url += @"&p=" + HttpUtility.UrlEncode(site.Credentials.Password);
            url += @"&r=" + HttpUtility.UrlEncode(path);

            return url;
        }

        public virtual void Navigate([NotNull] Site site, [NotNull] string webPath)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(webPath, nameof(webPath));

            var url = GetUrl(site, webPath);

            Navigate(url);
        }

        public virtual void Navigate([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            try
            {
                Process.Start(url);
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 1155 || ex.NativeErrorCode == 1156 || ex.Message.Contains("Class not registered") || ex.Message.Contains("Klasse nicht registriert") || ex.Message.Contains("Attempt to access invalid address") || ex.Message.Contains("No application is associated with the specified file for this operation"))
                {
                    if (AppHost.MessageBox("The default browser could not be opened.\n\nDo you want to open Internet Explorer?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                    {
                        var startInfo = new ProcessStartInfo("explorer.exe", url);
                        Process.Start(startInfo);
                    }

                    return;
                }

                throw;
            }
        }

        public virtual void NavigateInternalBrowser([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            Navigate(url);
        }
    }
}
