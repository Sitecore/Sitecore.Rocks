// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.ProjectHostExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.ProjectOptions.PropertyPages
{
    [Export(typeof(IPropertyPage))]
    public partial class ConnectionPropertyPage : IPropertyPage, INotifyPropertyChanged
    {
        private Site selectedSite;

        public ConnectionPropertyPage()
        {
            InitializeComponent();

            Header = "Connection";
            DataContext = this;
        }

        public string Header { get; }

        [CanBeNull]
        public ProjectBase Project { get; private set; }

        [CanBeNull]
        public Site SelectedSite
        {
            get { return selectedSite; }

            set
            {
                if (value == selectedSite)
                {
                    return;
                }

                selectedSite = value;
                TestButton.IsEnabled = value != null;
                DisconnectButton.IsEnabled = value != null;

                if (value != null)
                {
                    UpdateProject();
                }

                OnPropertyChanged("SelectedSite");
            }
        }

        [NotNull]
        public IEnumerable<Site> Sites
        {
            get { return SiteManager.Sites; }
        }

        [NotNull]
        public object VisualStudioProject { get; private set; }

        public void LoadProject(ProjectBase project, object visualStudioProject)
        {
            Assert.ArgumentNotNull(visualStudioProject, nameof(visualStudioProject));

            Project = project;
            VisualStudioProject = visualStudioProject;

            if (Project != null)
            {
                SelectedSite = Project.Site;
            }

            TestButton.IsEnabled = SelectedSite != null;
            DisconnectButton.IsEnabled = SelectedSite != null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CanBeNull] string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void CreateNewConnection([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var site = SiteManager.NewConnection();
            if (site == null)
            {
                return;
            }

            var bindingExpression = ConnectionsComboBox.GetBindingExpression(ItemsControl.ItemsSourceProperty);
            if (bindingExpression != null)
            {
                bindingExpression.UpdateTarget();
            }

            SelectedSite = site;
        }

        private void Disconnect([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Project == null)
            {
                return;
            }

            if (AppHost.MessageBox("Are you sure you want to disconnect the Visual Studio project and the Sitecore website?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }

            ProjectManager.UnloadProject(Project);

            var project = Project as Project;
            if (project != null)
            {
                project.Delete();
            }

            Project = null;
            SelectedSite = null;
        }

        private void TestConnection(object sender, RoutedEventArgs e)
        {
            AppHost.DoEvents();

            var site = SelectedSite;
            if (site == null)
            {
                return;
            }

            try
            {
                if (site.DataService.TestConnection(string.Empty))
                {
                    AppHost.MessageBox(Rocks.Resources.ProjectSettingsDialog_TestClick_Yes__it_works_, Rocks.Resources.Test_Connection, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(Rocks.Resources.SiteEditor_TestClick_OK__it_does_not_work_ + @"\n\n" + ex.Message, Rocks.Resources.Test_Connection, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateProject()
        {
            var project = (EnvDTE.Project)VisualStudioProject;

            if (Project == null)
            {
                Project = AppHost.Projects.CreateBlankProject(project.FileName);
            }

            var site = SelectedSite;
            if (site != null)
            {
                Project.HostName = site.HostName;
                Project.UserName = site.UserName;
            }

            Project.Save();
        }
    }
}
