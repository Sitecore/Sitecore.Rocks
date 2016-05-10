// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Schemas
{
    public static class SchemaManager
    {
        public static void InstallSchema([NotNull] string schemaFileName)
        {
            Assert.ArgumentNotNull(schemaFileName, nameof(schemaFileName));

            var location = Assembly.GetExecutingAssembly().Location;
            if (string.IsNullOrEmpty(location))
            {
                return;
            }

            var targetFolder = Path.Combine(GetProgramFilesFolder(), "Microsoft Visual Studio " + SitecorePackage.Instance.Dte.Version + "\\Xml\\Schemas");

            var target = Path.Combine(targetFolder, schemaFileName);

            if (File.Exists(target))
            {
                return;
            }

            if (AppHost.MessageBox(string.Format(Resources.SchemaManager_InstallSchema_Do_you_want_to_install_the_schema___0___, schemaFileName), Resources.SchemaManager_InstallSchema_Install_Schema, MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var sourceFolder = Path.Combine(Path.GetDirectoryName(location) ?? string.Empty, "Schemas");

            var source = Path.Combine(sourceFolder, schemaFileName);

            try
            {
                AppHost.Files.Copy(source, target, true);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(ex.Message, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [NotNull]
        private static string GetProgramFilesFolder()
        {
            if (IntPtr.Size == 8 || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)") ?? string.Empty;
            }

            return Environment.GetEnvironmentVariable("ProgramFiles") ?? string.Empty;
        }
    }
}
