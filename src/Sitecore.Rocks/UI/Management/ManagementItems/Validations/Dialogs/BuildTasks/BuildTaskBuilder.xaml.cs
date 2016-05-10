// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Validations.Dialogs.BuildTasks
{
    public partial class BuildTaskBuilder
    {
        private static readonly char[] SplitterChars =
        {
            ']',
            '['
        };

        public BuildTaskBuilder()
        {
            InitializeComponent();
        }

        [NotNull]
        public string DatabasesAndLanguages { get; private set; }

        [NotNull]
        public string InactiveValidations { get; private set; }

        [NotNull]
        public string ProfileName { get; private set; }

        [NotNull]
        public Site Site { get; private set; }

        public void Initialize([NotNull] Site site, [NotNull] string profileName)
        {
            Assert.ArgumentNotNull(profileName, nameof(profileName));
            Assert.ArgumentNotNull(site, nameof(site));

            ProfileName = profileName;
            Site = site;

            LoadProfiles();
            LoadProjects();

            UpdateProfile();
            UpdateConfigFile();
            UpdateBuildTask();

            EnableButtons();
        }

        public void Initialize([NotNull] Site site, [NotNull] ProjectBase project)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(project, nameof(project));

            ProjectComboBox.IsEnabled = false;

            ProfileName = string.Empty;
            Site = site;

            LoadProfiles();
            LoadProjects(project);

            UpdateProfile();
            UpdateConfigFile();
            UpdateBuildTask();

            EnableButtons();
        }

        private void CopyConfigFile([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var fileName = Path.Combine(GetProjectFolder(), "Sitecore.Rocks.Validation.Runner.config.xml");

            AppHost.Files.CreateDirectory(GetProjectFolder());
            AppHost.Files.WriteAllText(fileName, ConfigFileTextBox.Text, Encoding.UTF8);

            CopyConfigTextBlock.Visibility = Visibility.Visible;
        }

        private void CopyRunner([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var destFolder = GetProjectFolder();
            var dest = Path.Combine(destFolder, "Sitecore.Rocks.Validation.Runner.exe");
            var source = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, "Sitecore.Rocks.Validation.Runner.exe");

            AppHost.Files.CreateDirectory(destFolder);
            AppHost.Files.Copy(source, dest, true);

            CopyRunnerTextBlock.Visibility = Visibility.Visible;
        }

        private void EnableButtons()
        {
            var project = GetProject();
            CopyConfigButton.IsEnabled = project != null;
            CopyRunnerButton.IsEnabled = project != null;
        }

        [CanBeNull]
        private ProjectBase GetProject()
        {
            var comboBoxItem = ProjectComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return null;
            }

            return comboBoxItem.Tag as ProjectBase;
        }

        [NotNull]
        private string GetProjectFolder()
        {
            var result = string.Empty;

            var project = GetProject();
            if (project == null)
            {
                result += "<ProjectFolder>";
            }
            else
            {
                result += project.FolderName;
            }

            result += "\\sitecorerocks\\";

            return result;
        }

        private void HandleProfile([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var comboBoxItem = ProfileComboBox.SelectedItem as ComboBoxItem;
            if (comboBoxItem == null)
            {
                return;
            }

            ProfileName = comboBoxItem.Content as string ?? string.Empty;

            UpdateProfile();
            UpdateConfigFile();
            UpdateBuildTask();

            EnableButtons();
        }

        private void HandleProject([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var projectFolder = GetProjectFolder();
            ProjectFolderTextBlock.Text = projectFolder;

            UpdateConfigFile();
            UpdateBuildTask();

            EnableButtons();
        }

        private void LoadProfiles()
        {
            var keys = Storage.GetKeys(ValidationViewer.ValidationSiteProfiles);

            ProfileComboBox.Items.Clear();

            foreach (var key in keys)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = key
                };

                ProfileComboBox.Items.Add(comboBoxItem);

                if (string.Compare(key, ProfileName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    comboBoxItem.IsSelected = true;
                }
            }

            var all = new ComboBoxItem
            {
                Content = "All"
            };

            ProfileComboBox.Items.Add(all);

            if (ProfileComboBox.SelectedItem == null)
            {
                ProfileComboBox.SelectedItem = all;
            }
        }

        private void LoadProjects([CanBeNull] ProjectBase selectedProject = null)
        {
            var projects = AppHost.Projects.Where(proj => proj.Site == Site);

            foreach (var project in projects)
            {
                var comboBoxItem = new ComboBoxItem
                {
                    Content = project.Name,
                    Tag = project
                };

                ProjectComboBox.Items.Add(comboBoxItem);

                if (ProjectComboBox.SelectedItem == null)
                {
                    ProjectComboBox.SelectedItem = comboBoxItem;
                }

                if (project == selectedProject)
                {
                    ProjectComboBox.SelectedItem = comboBoxItem;
                }
            }
        }

        private void UpdateBuildTask()
        {
            BuildTaskTextBox.Text = "<Target Name=\"AfterBuild\">\r\n  <Exec Command=\"$(MSBuildProjectDirectory)\\sitecorerocks\\Sitecore.Rocks.Validation.Runner.exe\" WorkingDirectory=\"$(MSBuildProjectDirectory)\\sitecorerocks\\\"/>\r\n</Target>";
        }

        private void UpdateConfigFile()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            var ignore = new List<string>();
            if (!string.IsNullOrEmpty(InactiveValidations))
            {
                ignore.AddRange(InactiveValidations.Split(SplitterChars, StringSplitOptions.RemoveEmptyEntries));
            }

            output.WriteStartElement("SitecoreRocksValidation");

            Site.Connection.Save(output);

            var databases = DatabasesAndLanguages.Split('|');
            foreach (var database in databases)
            {
                var parts = database.Split('^');
                var databaseName = parts[0];
                var languages = string.Join(",", parts.Skip(1));

                output.WriteStartElement("Validate");

                output.WriteAttributeString("DatabaseName", databaseName);
                output.WriteAttributeString("Languages", languages);
                output.WriteAttributeString("Context", "Site");

                if (ignore.Any())
                {
                    foreach (var validation in ignore)
                    {
                        output.WriteStartElement("Ignore");
                        output.WriteValue(validation);
                        output.WriteEndElement();
                    }
                }

                output.WriteEndElement();
            }

            output.WriteEndElement();

            ConfigFileTextBox.Text = writer.ToString();
        }

        private void UpdateProfile()
        {
            InactiveValidations = AppHost.Settings.Get(ValidationViewer.ValidationSiteProfiles, ProfileName, string.Empty) as string ?? string.Empty;
            DatabasesAndLanguages = AppHost.Settings.GetString(ValidationViewer.ValidationSiteDatabasesAndLanguages, Site.Name, "master^en");
        }
    }
}
