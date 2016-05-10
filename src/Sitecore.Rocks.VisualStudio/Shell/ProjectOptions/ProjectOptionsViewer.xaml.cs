// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Shell.ProjectOptions.PropertyPages;

namespace Sitecore.Rocks.Shell.ProjectOptions
{
    public partial class ProjectOptionsViewer
    {
        public ProjectOptionsViewer()
        {
            InitializeComponent();

            AppHost.Extensibility.ComposeParts(this);
        }

        [NotNull, ImportMany(typeof(IPropertyPage))]
        public IEnumerable<IPropertyPage> PropertyPages { get; protected set; }

        public void LoadProject([NotNull] EnvDTE.Project project)
        {
            Assert.ArgumentNotNull(project, nameof(project));

            var proj = ProjectManager.GetProject(project.FileName);

            foreach (var propertyPage in PropertyPages)
            {
                var tabItem = new TabItem
                {
                    Header = propertyPage.Header,
                    Content = propertyPage
                };

                propertyPage.LoadProject(proj, project);

                Tabs.Items.Add(tabItem);

                var handler = propertyPage as ConnectionPropertyPage;
                if (handler != null)
                {
                    handler.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == "SelectedSite")
                        {
                            ReloadProjects(project);
                        }
                    };
                }
            }
        }

        private void ReloadProjects(EnvDTE.Project project)
        {
            var proj = ProjectManager.GetProject(project.FileName);

            foreach (var propertyPage in PropertyPages.Where(p => !(p is ConnectionPropertyPage)))
            {
                propertyPage.LoadProject(proj, project);
            }
        }
    }
}
