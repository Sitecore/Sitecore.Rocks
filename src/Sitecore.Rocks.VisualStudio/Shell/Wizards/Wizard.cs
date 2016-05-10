// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.Win32;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.IO.Zip;
using Sitecore.Rocks.Shell.Pipelines.NewItemWizard;

namespace Sitecore.Rocks.Shell.Wizards
{
    [UsedImplicitly]
    public class Wizard : IWizard
    {
        private static readonly Version VisualStudioVersion11 = new Version(11, 0);

        protected string ChildExtension { get; set; }

        protected List<ProjectItem> Children { get; set; }

        protected List<string> IgnoreExtensions { get; set; }

        protected ProjectItem Parent { get; set; }

        protected string ParentExtension { get; set; }

        private string Template { get; set; }

        [NotNull]
        private Dictionary<string, string> Tokens { get; set; }

        public void BeforeOpeningFile([CanBeNull] ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating([CanBeNull] Project project)
        {
        }

        public void ProjectItemFinishedGenerating([CanBeNull] ProjectItem projectItem)
        {
            if (projectItem == null)
            {
                return;
            }

            HandleDependentFiles(projectItem);

            if (!IsIgnored(projectItem.Name))
            {
                NewItemWizardPipeline.Run().WithParameters(projectItem, Tokens, Template);
            }
        }

        public void RunFinished()
        {
            if (Parent != null && Children != null)
            {
                foreach (var item in Children)
                {
                    MakeChild(Parent, item);
                }
            }

            Parent = null;
            Children = null;
            ChildExtension = null;
            ParentExtension = null;
            Template = null;
            Tokens.Clear();
        }

        public void RunStarted([CanBeNull] object automationObject, [NotNull] Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, [CanBeNull] object[] customParams)
        {
            Assert.ArgumentNotNull(replacementsDictionary, nameof(replacementsDictionary));

            Tokens = replacementsDictionary;

            Template = GetVsTemplate(customParams);

            var project = GetProject();
            if (project != null)
            {
                var assemblyPath = GetAssemblyPath(project);

                replacementsDictionary[@"$assemblypath$"] = assemblyPath;
                replacementsDictionary[@"$assemblyname$"] = Path.GetFileNameWithoutExtension(assemblyPath);
                replacementsDictionary[@"$projectfolder$"] = Path.GetDirectoryName(project.FileName);

                if (Template.IndexOf(@"App_Config\include", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    IncludeFolder(project, @"App_Config\include");
                }
            }
            else
            {
                replacementsDictionary[@"$assemblypath$"] = string.Empty;
                replacementsDictionary[@"$assemblyname$"] = string.Empty;
                replacementsDictionary[@"$projectfolder$"] = string.Empty;
            }

            replacementsDictionary[@"$devenvexe$"] = GetVisualStudioExe();
            replacementsDictionary[@"$sitecorerockspluginfolder$"] = AppHost.Plugins.PluginFolder;
            replacementsDictionary[@"$sitecorerocksfolder$"] = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string safeRootName;
            if (!replacementsDictionary.TryGetValue(@"$rootname$", out safeRootName))
            {
                safeRootName = "a";
            }
            else
            {
                safeRootName = Path.GetFileNameWithoutExtension(safeRootName) ?? string.Empty;
            }

            replacementsDictionary[@"$saferootname$"] = safeRootName.GetSafeCodeIdentifier();

            IgnoreExtensions = new List<string>();

            string s;
            if (replacementsDictionary.TryGetValue(@"$parentextension$", out s))
            {
                ParentExtension = s;
            }

            if (replacementsDictionary.TryGetValue(@"$childextension$", out s))
            {
                ChildExtension = s;
            }

            if (replacementsDictionary.TryGetValue(@"$ignoreextension$", out s))
            {
                IgnoreExtensions.AddRange(s.Split(','));
            }
        }

        public bool ShouldAddProjectItem([CanBeNull] string filePath)
        {
            return true;
        }

        [NotNull]
        private string GetAssemblyPath([NotNull] Project project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            var properties = project.Properties;
            if (properties == null)
            {
                return string.Empty;
            }

            var fullPathProperty = properties.Item(@"FullPath");
            if (fullPathProperty == null)
            {
                return string.Empty;
            }

            var fullPath = (fullPathProperty.Value ?? string.Empty).ToString();

            var configurationManager = project.ConfigurationManager;
            if (configurationManager == null)
            {
                return string.Empty;
            }

            var activeConfiguration = configurationManager.ActiveConfiguration;
            if (activeConfiguration == null)
            {
                return string.Empty;
            }

            var configProperties = activeConfiguration.Properties;
            if (configProperties == null)
            {
                return string.Empty;
            }

            var outputPathProperty = configProperties.Item(@"OutputPath");
            if (outputPathProperty == null)
            {
                return string.Empty;
            }

            var outputPath = (outputPathProperty.Value ?? string.Empty).ToString();

            var outputDir = Path.Combine(fullPath, outputPath);

            var outputFileNameProperty = properties.Item(@"OutputFileName");
            if (outputFileNameProperty == null)
            {
                return string.Empty;
            }

            var outputFileName = (outputFileNameProperty.Value ?? string.Empty).ToString();

            return Path.Combine(outputDir, outputFileName);
        }

        [CanBeNull]
        private Project GetProject()
        {
            var projects = SitecorePackage.Instance.Dte.ActiveSolutionProjects as object[];

            if (projects == null)
            {
                return null;
            }

            if (projects.Length != 1)
            {
                return null;
            }

            return projects[0] as Project;
        }

        [NotNull]
        private string GetVisualStudioExe()
        {
            var key = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\VisualStudio\\" + SitecorePackage.Instance.Dte.Version);
            if (key == null)
            {
                return string.Empty;
            }

            var installDir = key.GetValue("InstallDir", string.Empty) as string ?? string.Empty;

            if (string.IsNullOrEmpty(installDir))
            {
                return string.Empty;
            }

            return Path.Combine(installDir, "devenv.exe");
        }

        [NotNull]
        private string GetVsTemplate([CanBeNull] object[] customParams)
        {
            if (customParams == null)
            {
                return string.Empty;
            }

            if (customParams.Length == 0)
            {
                return string.Empty;
            }

            var fileName = customParams[0] as string ?? string.Empty;
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }

            if (AppHost.Shell.VisualStudioVersion >= VisualStudioVersion11)
            {
                return AppHost.Files.ReadAllText(fileName);
            }

            fileName = fileName.Replace(@"~IC\", string.Empty);
            fileName = fileName.Replace(@"~PC\", string.Empty);

            var zipFileName = Path.GetDirectoryName(fileName);
            var templateFileName = Path.GetFileName(fileName);

            using (var reader = new ZipReader(zipFileName))
            {
                foreach (var entry in reader.Entries)
                {
                    if (string.Compare(entry.Name, templateFileName, StringComparison.InvariantCultureIgnoreCase) != 0)
                    {
                        continue;
                    }

                    var textReader = new StreamReader(entry.GetStream());
                    return textReader.ReadToEnd();
                }
            }

            return string.Empty;
        }

        private void HandleDependentFiles([NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            if (string.IsNullOrEmpty(ParentExtension))
            {
                return;
            }

            if (string.IsNullOrEmpty(ChildExtension))
            {
                return;
            }

            var name = projectItem.Name;
            if (name.EndsWith(ParentExtension, StringComparison.OrdinalIgnoreCase))
            {
                Parent = projectItem;
            }
            else if (name.EndsWith(ChildExtension, StringComparison.OrdinalIgnoreCase))
            {
                if (Parent != null)
                {
                    MakeChild(Parent, projectItem);
                }
                else
                {
                    if (Children == null)
                    {
                        Children = new List<ProjectItem>();
                    }

                    Children.Add(projectItem);
                }
            }
        }

        private void IncludeFolder([NotNull] Project project, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(folder, nameof(folder));

            var projectItems = project.ProjectItems;
            var currentFolder = Path.GetDirectoryName(project.FileName) ?? string.Empty;

            Directory.CreateDirectory(Path.Combine(currentFolder, folder));

            foreach (var folderName in folder.Split('\\'))
            {
                currentFolder = Path.Combine(currentFolder, folderName);

                ProjectItem item = null;

                for (var n = 1; n <= projectItems.Count; n++)
                {
                    var projectItem = projectItems.Item(n);

                    if (string.Compare(projectItem.Name, folderName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        item = projectItem;
                        break;
                    }
                }

                if (item == null)
                {
                    item = projectItems.AddFromDirectory(currentFolder);

                    for (var n = item.ProjectItems.Count; n >= 1; n--)
                    {
                        var i = item.ProjectItems.Item(n);
                        i.Remove();
                    }
                }

                projectItems = item.ProjectItems;
            }
        }

        private bool IsIgnored([NotNull] string name)
        {
            Debug.ArgumentNotNull(name, nameof(name));

            if (IgnoreExtensions.Count == 0)
            {
                return false;
            }

            return IgnoreExtensions.Any(extension => name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase));
        }

        private static void MakeChild([NotNull] ProjectItem parent, [NotNull] ProjectItem child)
        {
            Debug.ArgumentNotNull(parent, nameof(parent));
            Debug.ArgumentNotNull(child, nameof(child));

            var str = child.GetFileName();

            if (!string.IsNullOrEmpty(str))
            {
                parent.ProjectItems.AddFromFileCopy(str);
            }
        }
    }
}
