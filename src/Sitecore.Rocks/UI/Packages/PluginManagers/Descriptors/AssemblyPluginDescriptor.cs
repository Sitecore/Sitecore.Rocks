// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;

namespace Sitecore.Rocks.UI.Packages.PluginManagers.Descriptors
{
    public class AssemblyPluginDescriptor : BasePluginDescriptor
    {
        public AssemblyPluginDescriptor([NotNull] Assembly assembly)
        {
            Assert.ArgumentNotNull(assembly, nameof(assembly));

            Assembly = assembly;

            Attribute[] attributes;
            try
            {
                attributes = Attribute.GetCustomAttributes(assembly);
            }
            catch
            {
                attributes = new Attribute[0];
            }

            foreach (var attribute in attributes)
            {
                var titleAttribute = attribute as AssemblyTitleAttribute;
                if (titleAttribute != null)
                {
                    Title = titleAttribute.Title ?? string.Empty;
                }

                var copyrightAttribute = attribute as AssemblyCopyrightAttribute;
                if (copyrightAttribute != null)
                {
                    Copyright = copyrightAttribute.Copyright ?? string.Empty;
                }

                var descriptiontAttribute = attribute as AssemblyDescriptionAttribute;
                if (descriptiontAttribute != null)
                {
                    Description = descriptiontAttribute.Description ?? string.Empty;
                }

                var companyAttribute = attribute as AssemblyCompanyAttribute;
                if (companyAttribute != null)
                {
                    Author = companyAttribute.Company ?? string.Empty;
                }
            }

            Version = assembly.GetFileVersion().ToString();
            Location = assembly.Location;

            if (string.IsNullOrEmpty(Summary))
            {
                Summary = Description;
            }

            if (string.IsNullOrEmpty(Description))
            {
                Description = Summary;
            }
        }

        [NotNull]
        public Assembly Assembly { get; }

        public override void Install(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            completed();
        }

        public override void Uninstall(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (AppHost.MessageBox(string.Format("Uninstalling this plugin will delete the file:\n\n{0}\n\nAre you sure you want to continue?", Assembly.Location), "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            var fileName = Assembly.Location;

            try
            {
                AppHost.Files.Delete(fileName);
            }
            catch
            {
                var deferred = AppHost.Plugins.PluginFolder;
                AppHost.Files.CreateDirectory(deferred);

                string target;
                var n = 0;
                do
                {
                    target = Path.Combine(deferred, n + @".uninstall");
                    n++;
                }
                while (File.Exists(target));

                File.WriteAllText(target, fileName);
            }

            AppHost.Extensibility.Reinitialize();
            AppHost.MessageBox("The plugin has been successfully uninstalled.", "Plugins", MessageBoxButton.OK, MessageBoxImage.Information);
            completed();
        }

        public override void Update(Action completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            completed();
        }
    }
}
