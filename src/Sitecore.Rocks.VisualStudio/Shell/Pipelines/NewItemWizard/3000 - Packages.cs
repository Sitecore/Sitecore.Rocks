// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 3000)]
    public class Packages : PipelineProcessor<NewItemWizardPipeline>
    {
        protected override void Process(NewItemWizardPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var project = ProjectManager.GetProject(pipeline.Item);
            if (project == null)
            {
                return;
            }

            var extension = Path.GetExtension(pipeline.Item.GetFileName()) ?? string.Empty;
            if (string.Compare(extension, ".package", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                return;
            }

            var fileName = project.GetProjectItemFileName(pipeline.Item);

            var projectItem = ProjectFileItem.Load(project, fileName);

            project.Add(projectItem);
        }
    }
}
