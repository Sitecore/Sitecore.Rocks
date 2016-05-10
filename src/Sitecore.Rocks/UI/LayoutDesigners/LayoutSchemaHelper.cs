// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using TaskDialogInterop;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    public static class LayoutSchemaHelper
    {
        public static void GenerateSchema([NotNull] ItemUri itemUri, [NotNull] IItem selectedItem, [NotNull] Action<ItemUri, IItem> completed)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(selectedItem, nameof(selectedItem));
            Assert.ArgumentNotNull(completed, nameof(completed));

            GenerateSchema(string.Empty, itemUri, selectedItem, (fileName, uri, item) => completed(uri, item));
        }

        public static void GenerateSchema([NotNull] string fileName, [NotNull] ItemUri itemUri, [NotNull] IItem selectedItem, [NotNull] Action<string, ItemUri, IItem> completed)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(selectedItem, nameof(selectedItem));
            Assert.ArgumentNotNull(completed, nameof(completed));

            if (string.IsNullOrEmpty(AppHost.Shell.VisualStudioLocation))
            {
                completed(fileName, itemUri, selectedItem);
                return;
            }

            var dontAsk = Assembly.GetExecutingAssembly().GetFileVersion().ToString();
            if (AppHost.Settings.GetString("Schemas", "DontAsk", string.Empty) == dontAsk)
            {
                return;
            }

            var xsdFileName = "Sitecore.Speak." + itemUri.DatabaseUri.Site.Name.GetSafeCodeIdentifier() + "." + itemUri.DatabaseUri.DatabaseName + ".xsd";
            var schemaFileName = Path.Combine(AppHost.User.UserFolder, "Xml\\Schemas\\" + xsdFileName);

            AppHost.Files.CreateDirectory(Path.GetDirectoryName(schemaFileName) ?? string.Empty);

            if (AppHost.Files.FileExists(schemaFileName))
            {
                completed(fileName, itemUri, selectedItem);
                return;
            }

            GenerateSchema(itemUri.DatabaseUri, true, () => completed(fileName, itemUri, selectedItem));
        }

        public static void GenerateSchema([NotNull] DatabaseUri databaseUri, bool showDontAsk, [NotNull] Action completed)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            var xsdFileName = "Sitecore.Speak." + databaseUri.Site.Name.GetSafeCodeIdentifier() + "." + databaseUri.DatabaseName + ".xsd";
            var schemaFileName = Path.Combine(AppHost.User.UserFolder, "Xml\\Schemas\\" + xsdFileName);

            var dontAskAgain = AppHost.Settings.GetString("Schemas", "DontAskAgain", string.Empty);
            switch (dontAskAgain)
            {
                case "Yes":
                    GenerateSchema(databaseUri, schemaFileName, completed);
                    return;
                case "No":
                    completed();
                    return;
            }

            AppHost.Files.CreateDirectory(Path.GetDirectoryName(schemaFileName) ?? string.Empty);

            var options = new TaskDialogOptions
            {
                Title = "Update IntelliSense for Renderings",
                CommonButtons = TaskDialogCommonButtons.YesNo,
                MainInstruction = "Do you want to update IntelliSense for rendering?",
                Content = "Updating the schema file for all MVC View renderings in this database will enable IntelliSense and validation in Layout Files.\n\nDepending on your security settings, the Windows UAC dialog may appear.",
                MainIcon = VistaTaskDialogIcon.Information,
                DefaultButtonIndex = 0,
                AllowDialogCancellation = true,
                VerificationText = "Do not show again"
            };

            if (showDontAsk)
            {
                options.VerificationText = "Do not show this dialog again";
                options.VerificationByDefault = false;
            }

            var r = TaskDialog.Show(options);

            if (showDontAsk && r.VerificationChecked == true)
            {
                AppHost.Settings.SetString("Schemas", "DontAskAgain", r.Result == TaskDialogSimpleResult.Yes ? "Yes" : "No");
            }

            if (r.Result != TaskDialogSimpleResult.Yes)
            {
                completed();
                return;
            }

            GenerateSchema(databaseUri, schemaFileName, completed);
        }

        private static void GenerateSchema([NotNull] DatabaseUri databaseUri, [NotNull] string schemaFileName, [NotNull] Action completed)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(completed, nameof(completed));
            Debug.ArgumentNotNull(schemaFileName, nameof(schemaFileName));

            ExecuteCompleted saveSchema = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    completed();
                    return;
                }

                var targetFolder = AppHost.Settings.GetString("Schemas", "Folder", string.Empty);
                if (string.IsNullOrEmpty(targetFolder))
                {
                    targetFolder = Path.Combine(AppHost.Shell.VisualStudioLocation, "Xml\\Schemas");
                }

                if (!AppHost.Files.FolderExists(targetFolder))
                {
                    using (var d = new FolderBrowserDialog())
                    {
                        d.ShowNewFolderButton = true;
                        d.Description = "Select the target folder for the schema file (usually %Visual Studio Install Dir%\\Xml\\Schemas):";
                        d.SelectedPath = targetFolder;

                        if (d.ShowDialog() != DialogResult.OK)
                        {
                            completed();
                            return;
                        }

                        targetFolder = d.SelectedPath;
                    }
                }

                AppHost.Settings.SetString("Schemas", "Folder", targetFolder);

                AppHost.Files.WriteAllText(schemaFileName, response, Encoding.UTF8);
                AppHost.Files.CopyWithElevatedRights(schemaFileName, targetFolder);

                completed();
            };

            databaseUri.Site.DataService.ExecuteAsync("XmlLayouts.GenerateSchema", saveSchema, databaseUri.Site.Name.GetSafeCodeIdentifier(), databaseUri.DatabaseName.ToString());
        }
    }
}
