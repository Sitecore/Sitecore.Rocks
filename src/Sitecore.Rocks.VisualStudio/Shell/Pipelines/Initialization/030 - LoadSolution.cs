// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    [Pipeline(typeof(InitializationPipeline), 3000)]
    public class LoadSolution : PipelineProcessor<InitializationPipeline>
    {
        protected override void Process(InitializationPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsStartUp)
            {
                return;
            }

            if (SitecorePackage.Instance.Dte.Solution != null)
            {
                ProjectManager.LoadProjects();
            }
        }
    }
}
