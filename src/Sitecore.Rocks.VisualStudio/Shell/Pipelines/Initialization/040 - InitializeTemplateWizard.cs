// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Win32;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.AssemblyNameExtensions;
using TaskDialogInterop;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 4000)]
    public class InitializeTemplateWizard : PipelineProcessor<InitializationPipeline>
    {
        private const string RegistryPath = "Software\\Sitecore\\VisualStudio\\";

        protected override void Process(InitializationPipeline pipeline)
        {
            Diagnostics.Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            WriteAssemblyPath();
            CopyAssembly();

            InstallInGac();
        }

        private void InstallInGac()
        {
            var source = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"Sitecore.Rocks.TemplateWizard.dll");
            try
            {
                new System.EnterpriseServices.Internal.Publish().GacInstall(source);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }
        }

        private void CopyAssembly()
        {
            var installDir = AppHost.Settings.GetString("TemplateWizard", "InstallDir", string.Empty);
            if (string.IsNullOrEmpty(installDir))
            {
                var version = SitecorePackage.Instance.Dte.Version;

                var visualStudioKey = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\VisualStudio\\" + version);
                if (visualStudioKey != null)
                {
                    installDir = visualStudioKey.GetValue(@"InstallDir") as string;
                }

                if (installDir == null)
                {
                    installDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86), @"Microsoft Visual Studio " + version + @"\Common7\IDE");
                }

                installDir = Path.Combine(installDir, @"PublicAssemblies");
            }

            var source = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"Sitecore.Rocks.TemplateWizard.dll");
            var target = Path.Combine(installDir, @"Sitecore.Rocks.TemplateWizard.dll");

            if (File.Exists(target))
            {
                return;
            }

            var dontAsk = Assembly.GetExecutingAssembly().GetFileVersion().ToString();
            if (AppHost.Settings.GetString("TemplateWizard", "DontAsk", string.Empty) == dontAsk)
            {
                return;
            }

            if (!Directory.Exists(installDir))
            {
                var options = new TaskDialogOptions
                {
                    Title = "Install Template Wizard",
                    CommonButtons = TaskDialogCommonButtons.None,
                    MainInstruction = "The Sitecore Template Wizard is not installed.",
                    Content = "Adding new Visual Studio items which also creates Sitecore items will probably not work correctly.\n\nDo you want to install it now?\n\nDepending on your security settings, the Windows UAC dialog may appear.",
                    MainIcon = VistaTaskDialogIcon.Information,
                    DefaultButtonIndex = 0,
                    CommandButtons = new[]
                    {
                        "Select where to install the Template Wizard"
                    },
                    VerificationText = "Do not show this dialog again",
                    VerificationByDefault = false,
                    AllowDialogCancellation = true
                };

                var result = TaskDialog.Show(options);
                var r = result.CommandButtonResult;

                if (result.VerificationChecked == true)
                {
                    AppHost.Settings.SetString("TemplateWizard", "DontAsk", dontAsk);
                }

                if (r != 0)
                {
                    return;
                }

                using (var d = new FolderBrowserDialog())
                {
                    d.ShowNewFolderButton = true;
                    d.Description = "Select the target folder for the Template Wizard (usually %Visual Studio Install Dir%\\Common7\\IDE\\PublicAssemblies):";
                    d.SelectedPath = installDir;

                    if (d.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    installDir = d.SelectedPath;
                }
            }
            else
            {
                var options = new TaskDialogOptions
                {
                    Title = "Install Template Wizard",
                    CommonButtons = TaskDialogCommonButtons.OKCancel,
                    MainInstruction = "The Sitecore Template Wizard is not installed.",
                    Content = "Adding new Visual Studio items which also creates Sitecore items will probably not work correctly.\n\nDo you want to install it now?\n\nDepending on your security settings, the Windows UAC dialog may appear.",
                    MainIcon = VistaTaskDialogIcon.Information,
                    DefaultButtonIndex = 0,
                    VerificationText = "Do not show this dialog again",
                    VerificationByDefault = false,
                    AllowDialogCancellation = true
                };

                var result = TaskDialog.Show(options);

                if (result.VerificationChecked == true)
                {
                    AppHost.Settings.SetString("TemplateWizard", "DontAsk", dontAsk);
                }

                if (result.Result != TaskDialogSimpleResult.Ok)
                {
                    return;
                }
            }

            AppHost.Settings.SetString("TemplateWizard", "InstallDir", installDir);

            try
            {
                AppHost.Files.CopyWithElevatedRights(source, installDir);
            }
            catch (Exception ex)
            {
                AppHost.Output.Log("Failed to copy {0} to {1}: {2}", source, target, ex.Message);
                AppHost.MessageBox("Failed to install the Template Wizard:\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void WriteAssemblyPath()
        {
            var registryKey = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
            if (registryKey == null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey(RegistryPath);
            }

            if (registryKey == null)
            {
                return;
            }

            registryKey.SetValue(@"ExtensionAssemblyPath", Assembly.GetExecutingAssembly().Location);
        }
    }
}
