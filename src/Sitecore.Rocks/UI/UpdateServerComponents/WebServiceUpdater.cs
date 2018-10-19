// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    [ServerComponentsUpdater, ServerComponentsRemover]
    public class WebServiceUpdater : IServerComponentUpdater, IServerComponentRemover
    {
        public bool CanRemove([NotNull] RemoveServerComponentOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));

            return !string.IsNullOrEmpty(options.WebSiteRootFolder);
        }

        public bool CanUpdate(UpdateServerComponentOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));

            return options.PluginName == Constants.SitecoreRocksServer;
        }

        public bool Remove([NotNull] RemoveServerComponentOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));

            var webServiceFolder = Path.Combine(options.WebSiteRootFolder, @"sitecore/shell/WebService");

            try
            {
                File.Delete(Path.Combine(webServiceFolder, @"Service2.asmx"));
            }
            catch (Exception ex)
            {
                options.Output.WriteLine(@"/sitecore/shell/WebService/Service2.asmx: " + ex.Message);
            }

            try
            {
                File.Delete(Path.Combine(webServiceFolder, @"Browse.aspx"));
            }
            catch (Exception ex)
            {
                options.Output.WriteLine(@"/sitecore/shell/WebService/Browse.aspx: " + ex.Message);
            }

            try
            {
                File.Delete(Path.Combine(webServiceFolder, @"Sitecore.Rocks.Validation.ashx"));
            }
            catch (Exception ex)
            {
                options.Output.WriteLine(@"/sitecore/shell/WebService/Sitecore.Rocks.Validation.ashx: " + ex.Message);
            }

            try
            {
                File.Delete(Path.Combine(webServiceFolder, @"Web.config"));
            }
            catch (Exception ex)
            {
                options.Output.WriteLine(@"/sitecore/shell/WebService/Web.config: " + ex.Message);
            }

            return true;
        }

        public bool Update(UpdateServerComponentOptions options)
        {
            Assert.ArgumentNotNull(options, nameof(options));

            var source = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            var webServiceFolder = Path.Combine(options.WebSiteRootFolder, @"sitecore/shell/WebService");

            try
            {
                AppHost.Files.CreateDirectory(webServiceFolder);

                IO.File.CopyFile(webServiceFolder, Path.Combine(source, @"WebService\\Service2.asmx"));
                IO.File.CopyFile(webServiceFolder, Path.Combine(source, @"WebService\\Browse.aspx"));
                IO.File.CopyFile(webServiceFolder, Path.Combine(source, @"WebService\\Sitecore.Rocks.Validation.ashx"));
                IO.File.CopyFile(webServiceFolder, Path.Combine(source, @"WebService\\Web.config"));
            }
            catch (UnauthorizedAccessException)
            {
                AppHost.MessageBox(string.Format(Resources.WebServiceUpdater_Update_, webServiceFolder), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }
    }
}
