// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Windows;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioFilesHost : FilesHost
    {
        public override void OpenFile(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            AccessFile(fileName, FileAccess.Read);

            try
            {
                SitecorePackage.Instance.Dte.Application.ItemOperations.OpenFile(fileName);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(string.Format("An error occurred while opening the file.\n\n{0}", ex.Message), Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
