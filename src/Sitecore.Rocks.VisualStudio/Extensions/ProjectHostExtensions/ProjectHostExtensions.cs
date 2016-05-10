// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Shell.Environment;
using Sitecore.Rocks.UI.Dialogs.SelectSiteDialogs;

namespace Sitecore.Rocks.Extensions.ProjectHostExtensions
{
    public static class ProjectHostExtensions
    {
        [CanBeNull]
        public static Project ConnectProjectToSitecore([NotNull] this ProjectHost projectHost, [NotNull] EnvDTE.Project project)
        {
            Assert.ArgumentNotNull(projectHost, nameof(projectHost));
            Assert.ArgumentNotNull(project, nameof(project));

            var d = new SelectSiteDialog();

            var hostName = GetServer(project);

            d.Select(hostName, string.Empty);
            d.ShowNewConnectionButton = true;

            if (AppHost.Shell.ShowDialog(d) != true)
            {
                return null;
            }

            var site = d.Site;
            if (site == null)
            {
                return null;
            }

            var newProject = ProjectManager.CreateBlankProject(project.FileName);

            newProject.HostName = site.HostName;
            newProject.UserName = site.UserName;
            newProject.IsRemoteSitecore = projectHost.GetIsRemoteSitecore(project.FileName);

            newProject.Save();
            ProjectManager.LoadProject(newProject);

            return newProject;
        }

        [NotNull]
        public static Project CreateBlankProject([NotNull] this ProjectHost projectHost, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(projectHost, nameof(projectHost));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var newProject = ProjectManager.CreateBlankProject(fileName);
            ProjectManager.LoadProject(newProject);
            return newProject;
        }

        [CanBeNull]
        public static Project FindProjectFromProjectItemFileName([NotNull] this ProjectHost projectHost, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(projectHost, nameof(projectHost));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return ProjectManager.FindProjectFromProjectItemFileName(fileName);
        }

        public static bool GetIsRemoteSitecore([NotNull] this ProjectHost projectHost, [NotNull] string fileName)
        {
            Assert.ArgumentNotNull(projectHost, nameof(projectHost));
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var path = Path.GetDirectoryName(fileName);

            path += @"\sitecore\shell\WebService\service.asmx";

            return !File.Exists(path);
        }

        private static T GetProperty<T>([NotNull] EnvDTE.Project project, [NotNull] string name)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(name, nameof(name));

            try
            {
                var property = project.Properties.Item(name);

                if (property == null)
                {
                    return default(T);
                }

                return (T)property.Value;
            }
            catch
            {
                return default(T);
            }
        }

        [NotNull]
        private static string GetServer([NotNull] EnvDTE.Project project)
        {
            Debug.ArgumentNotNull(project, nameof(project));

            /*
      var s = string.Empty;
      for (var n = 1; n < item.Properties.Count; n++)
      {
        try
        {
          s += item.Properties.Item(n).Name + " = " + item.Properties.Item(n).Value +  "\n";
        }
        catch
        {
          s += item.Properties.Item(n).Name + "\n";
        }
      }
      */

            string server;
            var useIis = GetProperty<bool>(project, @"WebApplication.UseIIS");
            if (useIis)
            {
                server = GetProperty<string>(project, @"WebApplication.IISUrl");
            }
            else
            {
                server = @"localhost";

                var port = GetProperty<int>(project, @"WebApplication.DevelopmentServerPort");
                if (port != 80)
                {
                    server += @":" + port;
                }

                var virtualPath = GetProperty<string>(project, @"WebApplication.DevelopmentServerVPath");

                if (virtualPath != @"/")
                {
                    server += virtualPath;
                }
            }

            return server;
        }
    }
}
