// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Sites
{
    public static class SiteHelper
    {
        [CanBeNull]
        public static string BrowseWebRootPath([NotNull] string webSiteRoot, [NotNull] string siteName)
        {
            Assert.ArgumentNotNull(webSiteRoot, nameof(webSiteRoot));
            Assert.ArgumentNotNull(siteName, nameof(siteName));

            var selectedPath = webSiteRoot;
            if (string.IsNullOrEmpty(selectedPath))
            {
                selectedPath = AppHost.Settings.Get("Web Site Location", "Last Selected Path", string.Empty) as string ?? string.Empty;
            }

            var retry = true;

            while (retry)
            {
                DialogResult dialogResult;

                using (var d = new FolderBrowserDialog
                {
                    ShowNewFolderButton = false,
                    SelectedPath = selectedPath
                })
                {
                    if (!string.IsNullOrEmpty(siteName))
                    {
                        d.Description = string.Format(Resources.SiteHelper_BrowseWebRootPath_Select_the_root_of_the_web_site___0___, siteName);
                    }
                    else
                    {
                        d.Description = Resources.SiteEditor_Browse_Select_the_root_of_the_web_site_;
                    }

                    dialogResult = d.ShowDialog();
                    selectedPath = d.SelectedPath;
                }

                if (dialogResult != DialogResult.OK)
                {
                    return null;
                }

                if (IsValidWebRootPath(selectedPath))
                {
                    break;
                }

                switch (AppHost.MessageBox(Resources.SiteHelper_BrowseWebRootPath_, Resources.Information, MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        retry = false;
                        break;

                    case MessageBoxResult.No:

                        // retry = true;
                        break;

                    default:
                        return null;
                }
            }

            if (string.IsNullOrEmpty(selectedPath))
            {
                return null;
            }

            AppHost.Settings.Set("Web Site Location", "Last Selected Path", selectedPath);

            return selectedPath;
        }

        private static bool IsValidWebRootPath([NotNull] string selectedPath)
        {
            Debug.ArgumentNotNull(selectedPath, nameof(selectedPath));

            if (!Directory.Exists(Path.Combine(selectedPath, @"bin")))
            {
                return false;
            }

            return Directory.Exists(Path.Combine(selectedPath, @"sitecore"));
        }
    }
}
