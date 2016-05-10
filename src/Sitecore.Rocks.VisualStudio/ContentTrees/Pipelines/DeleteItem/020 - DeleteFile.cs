// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem
{
    [Pipeline(typeof(DeleteItemPipeline), 2000)]
    public class DeleteFile : PipelineProcessor<DeleteItemPipeline>
    {
        protected override void Process([NotNull] DeleteItemPipeline pipeline)
        {
            Assert.ArgumentNotNull(pipeline, nameof(pipeline));

            if (!pipeline.IsDeleted)
            {
                return;
            }

            var projectFile = ProjectManager.GetProjectFileItem(pipeline.ItemUri);
            if (projectFile == null)
            {
                return;
            }

            projectFile.Project.Remove(projectFile);

            if (!pipeline.DeleteFiles)
            {
                return;
            }

            var projectItem = SitecorePackage.Instance.Dte.Solution.FindProjectItem(projectFile.Path);
            if (projectItem != null)
            {
                projectItem.Delete();
            }
        }
    }
}
