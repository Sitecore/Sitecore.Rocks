// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using EnvDTE80;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 2000)]
    public class RegisterEvents : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            var package = SitecorePackage.Instance;

            package.Events = (Events2)package.Dte.Events;

            package.WindowEvents = package.Events.WindowEvents;
            package.WindowEvents.WindowActivated += package.WindowActivated;
            package.WindowEvents.WindowClosing += package.WindowClosing;

            package.SolutionEvents = package.Events.SolutionEvents;
            package.SolutionEvents.Opened += () => ShellNotifications.RaiseSolutionLoaded(package);
            package.SolutionEvents.BeforeClosing += () => ShellNotifications.RaiseSolutionUnloaded(package);

            package.SolutionEvents.ProjectAdded += project => ShellNotifications.RaiseProjectAdded(package, project);
            package.SolutionEvents.ProjectRemoved += project => ShellNotifications.RaiseProjectUnloaded(package, project);
            package.SolutionEvents.ProjectRenamed += (project, oldName) => ShellNotifications.RaiseProjectRenamed(package, project, oldName);

            package.ProjectItemsEvents = package.Events.ProjectItemsEvents;
            package.ProjectItemsEvents.ItemRenamed += (projectItem, newName) => ShellNotifications.RaiseProjectItemRenamed(package, projectItem, newName);
            package.ProjectItemsEvents.ItemRemoved += item => ShellNotifications.RaiseProjectItemRemoved(package, item);

            package.BuildEvents = package.Events.BuildEvents;
            package.BuildEvents.OnBuildDone += (scope, action) => ShellNotifications.RaiseBuildDone(package, scope, action);

            package.VsRunningDocumentTable = new VsRunningDocumentTable();
            package.VsTrackProjectDocument = new VsTrackProjectDocuments();
            package.VsTrackSelection = new VsTrackSelection();
        }
    }
}
