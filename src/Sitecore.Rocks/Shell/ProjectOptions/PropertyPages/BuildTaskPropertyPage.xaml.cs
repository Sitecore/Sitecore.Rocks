// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.ProjectOptions.PropertyPages
{
    [Export(typeof(IPropertyPage), Priority = 1000)]
    public partial class BuildTaskPropertyPage : IPropertyPage
    {
        public BuildTaskPropertyPage()
        {
            InitializeComponent();

            Header = "Build Task";
        }

        public string Header { get; }

        [NotNull]
        public object VisualStudioProject { get; private set; }

        public void LoadProject(ProjectBase project, object visualStudioProject)
        {
            Assert.ArgumentNotNull(visualStudioProject, nameof(visualStudioProject));

            if (project == null)
            {
                Builder.Visibility = Visibility.Collapsed;
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                Builder.Visibility = Visibility.Collapsed;
                return;
            }

            Builder.Visibility = Visibility.Visible;
            Builder.Initialize(site, project);
        }
    }
}
