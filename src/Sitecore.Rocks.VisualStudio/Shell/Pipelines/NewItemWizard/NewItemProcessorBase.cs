// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectHostExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    public abstract class NewItemProcessorBase : PipelineProcessor<NewItemWizardPipeline>
    {
        [CanBeNull]
        protected Project GetProject([NotNull] NewItemWizardPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.HasAskedForConnection)
            {
                return null;
            }

            pipeline.HasAskedForConnection = true;

            if (AppHost.MessageBox("The project is not connected to Sitecore.\n\nDo you want to connect now?", "Confirmation", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return null;
            }

            return AppHost.Projects.ConnectProjectToSitecore(pipeline.Item.ContainingProject);
        }
    }
}
