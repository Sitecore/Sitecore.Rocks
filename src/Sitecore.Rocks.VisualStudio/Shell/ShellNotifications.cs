// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell
{
    public static class ShellNotifications
    {
        public delegate void BuildDoneEventHandler(object sender, vsBuildScope scope, vsBuildAction action);

        public delegate void ProjectEventHandler(object sender, Project project);

        public delegate void ProjectItemMovedEventHandler(object sender, string newName, string oldName);

        public delegate void ProjectItemRemovedEventHandler(object sender, ProjectItem projectItem);

        public delegate void ProjectItemRenamedEventHandler(object sender, ProjectItem projectItem, string oldName);

        public delegate void ProjectRenamedEventHandler(object sender, Project project, [NotNull] string oldName);

        public delegate void SolutionEventHandler(object sender);

        public static event BuildDoneEventHandler BuildDone;

        public static event ProjectEventHandler ProjectAdded;

        public static event ProjectItemMovedEventHandler ProjectItemMoved;

        public static event ProjectItemRemovedEventHandler ProjectItemRemoved;

        public static event ProjectItemRenamedEventHandler ProjectItemRenamed;

        public static event ProjectRenamedEventHandler ProjectRenamed;

        public static event ProjectEventHandler ProjectUnloaded;

        public static void RaiseBuildDone([NotNull] object sender, vsBuildScope scope, vsBuildAction action)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var buildDone = BuildDone;
            if (buildDone != null)
            {
                buildDone(sender, scope, action);
            }
        }

        public static void RaiseProjectAdded([NotNull] object sender, [NotNull] Project project)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(project, nameof(project));

            var projectAdded = ProjectAdded;
            if (projectAdded != null)
            {
                projectAdded(sender, project);
            }
        }

        public static void RaiseProjectItemMoved([NotNull] object sender, [NotNull] string newName, [NotNull] string oldName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(newName, nameof(newName));
            Assert.ArgumentNotNull(oldName, nameof(oldName));

            var projectItemMoved = ProjectItemMoved;
            if (projectItemMoved != null)
            {
                projectItemMoved(sender, newName, oldName);
            }
        }

        public static void RaiseProjectItemRemoved([NotNull] object sender, [NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var projectItemRemoved = ProjectItemRemoved;
            if (projectItemRemoved != null)
            {
                projectItemRemoved(sender, projectItem);
            }
        }

        public static void RaiseProjectItemRenamed([NotNull] object sender, [NotNull] ProjectItem projectItem, [NotNull] string oldName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));
            Assert.ArgumentNotNull(oldName, nameof(oldName));

            var projectItemRemoved = ProjectItemRenamed;
            if (projectItemRemoved != null)
            {
                projectItemRemoved(sender, projectItem, oldName);
            }
        }

        public static void RaiseProjectRenamed([NotNull] object sender, [NotNull] Project project, [NotNull] string oldName)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(project, nameof(project));
            Assert.ArgumentNotNull(oldName, nameof(oldName));

            var projectItemMoved = ProjectRenamed;
            if (projectItemMoved != null)
            {
                projectItemMoved(sender, project, oldName);
            }
        }

        public static void RaiseProjectUnloaded([NotNull] object sender, [NotNull] Project project)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(project, nameof(project));

            var projectUnloaded = ProjectUnloaded;
            if (projectUnloaded != null)
            {
                projectUnloaded(sender, project);
            }
        }

        public static void RaiseSolutionLoaded([NotNull] object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var solutionLoaded = SolutionLoaded;
            if (solutionLoaded != null)
            {
                solutionLoaded(sender);
            }
        }

        public static void RaiseSolutionUnloaded([NotNull] object sender)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));

            var solutionUnloaded = SolutionUnloaded;
            if (solutionUnloaded != null)
            {
                solutionUnloaded(sender);
            }
        }

        public static event SolutionEventHandler SolutionLoaded;

        public static event SolutionEventHandler SolutionUnloaded;
    }
}
