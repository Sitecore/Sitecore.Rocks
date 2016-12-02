// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;

namespace Sitecore.Rocks.ContentTrees.Commands.Helix.Dialogs
{
    public partial class CreateHelixSolutionDialog
    {
        public CreateHelixSolutionDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            LocationTextBox.Text = AppHost.Settings.GetString("CreateHelixSolution", "Location", string.Empty);
            IgnoreGitCheckBox.IsChecked = AppHost.Settings.GetBool("CreateHelixSolution", "IgnoreGitIgnore", false);
        }

        [NotNull]
        public string Description => DescriptionTextBox.Text ?? string.Empty;

        public bool IgnoreGitIgnoreFile => IgnoreGitCheckBox.IsChecked == true;

        [NotNull]
        public string Location => LocationTextBox.Text ?? string.Empty;

        [NotNull]
        public string SolutionName => NameTextBox.Text ?? string.Empty;

        private void Browse([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            using (var d = new FolderBrowserDialog())
            {
                d.ShowNewFolderButton = true;
                d.Description = "Select a location for the Visual Studio project:";
                d.SelectedPath = LocationTextBox.Text;

                if (d.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                LocationTextBox.Text = d.SelectedPath;

                if (string.IsNullOrEmpty(NameTextBox.Text))
                {
                    NameTextBox.Text = Path.GetFileName(d.SelectedPath);
                }
            }
        }

        private void EnableButtons(object sender, TextChangedEventArgs e)
        {
            OkButton.IsEnabled = !string.IsNullOrEmpty(LocationTextBox.Text) && !string.IsNullOrEmpty(NameTextBox.Text);
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            AppHost.Settings.SetString("CreateHelixSolution", "Location", LocationTextBox.Text);
            AppHost.Settings.SetBool("CreateHelixSolution", "IgnoreGitIgnore", IgnoreGitCheckBox.IsChecked == true);

            this.Close(true);
        }
    }
}
