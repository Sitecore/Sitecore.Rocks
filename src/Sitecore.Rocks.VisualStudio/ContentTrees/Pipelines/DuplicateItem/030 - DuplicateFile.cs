// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.FileItems;

namespace Sitecore.Rocks.ContentTrees.Pipelines.DuplicateItem
{
    [Pipeline(typeof(DuplicateItemPipeline), 3000)]
    public class DuplicateFile : PipelineProcessor<DuplicateItemPipeline>
    {
        protected override void Process(DuplicateItemPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.NewItemUri == ItemUri.Empty)
            {
                return;
            }

            var projectFileItem = ProjectManager.GetProjectFileItem(pipeline.ItemUri);
            if (projectFileItem == null)
            {
                return;
            }

            var project = projectFileItem.Project;

            var source = projectFileItem.AbsoluteFileName;
            var sourceFolder = Path.GetDirectoryName(source) ?? string.Empty;
            var extension = Path.GetExtension(source);
            var target = Path.Combine(sourceFolder, pipeline.NewName) + extension;

            if (!AppHost.Files.FileExists(target))
            {
                AppHost.Files.Copy(source, target, false);
            }

            EnvDTE.ProjectItem newProjectItem;
            try
            {
                newProjectItem = SitecorePackage.Instance.Dte.ItemOperations.AddExistingItem(target);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
                return;
            }

            var fileName = project.GetProjectItemFileName(newProjectItem);
            var projectItem = ProjectFileItem.Load(project, fileName);
            projectItem.Items.Add(pipeline.NewItemUri);
            project.Add(projectItem);
            project.Save();

            var fileItemHandler = FileItemManager.GetFileItemHandler(source);
            if (fileItemHandler != null)
            {
                fileItemHandler.UpdateItemPath(pipeline.NewItemUri, projectItem.Path);
            }
        }
    }
}
