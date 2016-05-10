// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using NuGet;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Builders
{
    public abstract class NuGetPackageBuilderBase : BasePackageBuilder
    {
        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(Site.WebRootPath))
            {
                AppHost.MessageBox("Building NuGet packages requires physical access to the web site.\n\nPlease specify a Web Root Path in the Sitecore Explorer connection.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(PackageName))
            {
                AppHost.MessageBox("Package must have a name.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(Author))
            {
                AppHost.MessageBox("The Author field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(Version))
            {
                AppHost.MessageBox("The Version field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            SemanticVersion semanticVersion;
            if (!SemanticVersion.TryParse(Version, out semanticVersion))
            {
                AppHost.MessageBox("The Version field is not a valid Semantic Version.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (string.IsNullOrEmpty(Readme))
            {
                AppHost.MessageBox("The Readme field must have a value.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            return base.IsValid();
        }

        protected override void Execute(ExecuteCompleted completed, StringBuilder items, StringBuilder files)
        {
            Debug.ArgumentNotNull(files, nameof(files));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(completed, nameof(completed));

            var nugetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "NuGet");

            Site.DataService.ExecuteAsync(RequestTypeName, completed, items.ToString(), files.ToString(), PackageName, Author, Version, Publisher, License, Comment, Readme, TargetFileFolder, nugetPath);
        }

        protected override void ProcessCompleted(string url, string targetFileName, Action<string> completed)
        {
            Debug.ArgumentNotNull(completed, nameof(completed));
            Debug.ArgumentNotNull(targetFileName, nameof(targetFileName));
            Debug.ArgumentNotNull(url, nameof(url));

            targetFileName = Path.ChangeExtension(targetFileName, "nuspec");

            var client = new WebClient();
            try
            {
                client.DownloadFile(url, targetFileName);
            }
            catch (WebException ex)
            {
                if (AppHost.MessageBox(string.Format("Failed to download the package file: {0}\n\nDo you want to report this error?\n\n{1}\n{2}", url, ex.Message, ex.StackTrace), "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    AppHost.Shell.HandleException(ex);
                }

                completed(string.Empty);
                return;
            }

            var nuspecFileName = targetFileName;
            var nupkgFileName = Path.ChangeExtension(nuspecFileName, string.Empty) + Version + ".nupkg";

            try
            {
                using (var nuspec = new FileStream(nuspecFileName, FileMode.Open, FileAccess.Read))
                {
                    var packageBuilder = new NuGet.PackageBuilder(nuspec, Path.GetDirectoryName(targetFileName));

                    using (var nupkg = new FileStream(nupkgFileName, FileMode.Create))
                    {
                        packageBuilder.Save(nupkg);
                    }
                }
            }
            catch (Exception ex)
            {
                if (AppHost.MessageBox(string.Format("Failed to create the NuGet package file: {0}\n\nDo you want to report this error?\n\n{1}", url, ex.Message), "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                {
                    AppHost.Shell.HandleException(ex);
                }

                completed(string.Empty);
                return;
            }

            completed(nupkgFileName);
        }
    }
}
