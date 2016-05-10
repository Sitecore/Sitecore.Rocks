// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.FileItems;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DragCopy
{
    [Pipeline(typeof(DragCopyPipeline), 3000)]
    public class CopyFiles : PipelineProcessor<DragCopyPipeline>
    {
        protected override void Process(DragCopyPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var projects = new List<ProjectBase>();

            foreach (var item in pipeline.NewItems)
            {
                CopyFile(projects, item);
            }

            foreach (var project in projects)
            {
                project.Save();
            }
        }

        private void CopyFile([NotNull] List<ProjectBase> projects, [NotNull] DragCopyPipeline.NewItem item)
        {
            Debug.ArgumentNotNull(projects, nameof(projects));
            Debug.ArgumentNotNull(item, nameof(item));

            var projectFileItem = ProjectManager.GetProjectFileItem(item.Item.ItemUri);
            if (projectFileItem == null)
            {
                return;
            }

            var project = projectFileItem.Project;

            var source = projectFileItem.AbsoluteFileName;
            var sourceFolder = Path.GetDirectoryName(source) ?? string.Empty;
            var extension = Path.GetExtension(source);
            var target = Path.Combine(sourceFolder, item.NewName) + extension;

            AppHost.Files.Copy(source, target, false);

            var newProjectItem = SitecorePackage.Instance.Dte.ItemOperations.AddExistingItem(target);

            var fileName = project.GetProjectItemFileName(newProjectItem);
            var projectItem = ProjectFileItem.Load(project, fileName);
            projectItem.Items.Add(item.NewItemUri);
            project.Add(projectItem);

            if (!projects.Contains(project))
            {
                projects.Add(project);
            }

            var fileItemHandler = FileItemManager.GetFileItemHandler(source);
            if (fileItemHandler == null)
            {
                return;
            }

            fileItemHandler.UpdateItemPath(item.NewItemUri, projectItem.Path);
        }
    }
}
