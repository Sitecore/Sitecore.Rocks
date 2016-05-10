// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Projects.FileItems;
using Sitecore.Rocks.Shell.Dialogs;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    [Pipeline(typeof(NewItemWizardPipeline), 1000)]
    public class FileItemHandler : NewItemProcessorBase
    {
        protected override void Process(NewItemWizardPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            var handler = FileItemManager.GetFileItemHandler(pipeline.Item.GetFileName());
            if (handler == null)
            {
                return;
            }

            var project = ProjectManager.GetProject(pipeline.Item) ?? GetProject(pipeline);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            var path = string.Empty;

            if (pipeline.DatabaseName == null)
            {
                var d = new NewItemWizardDialog();
                d.Initialize(site, handler.GetTemplateName());
                if (AppHost.Shell.ShowDialog(d) != true)
                {
                    return;
                }

                var itemUri = d.ItemUri;
                if (itemUri == null || itemUri == ItemUri.Empty)
                {
                    return;
                }

                pipeline.DatabaseName = itemUri.DatabaseName;

                path = d.SelectedPath ?? string.Empty;
                if (!string.IsNullOrEmpty(path))
                {
                    path += "/" + Path.GetFileNameWithoutExtension(pipeline.Item.Name);
                }
            }

            var fileName = project.GetProjectItemFileName(pipeline.Item);
            var projectItem = ProjectFileItem.Load(project, fileName);
            project.Add(projectItem);

            var busy = true;

            handler.Handle(pipeline.DatabaseName, projectItem, path, (sender, args) => busy = false);

            AppHost.DoEvents(ref busy);

            pipeline.ProjectItem = projectItem;
        }
    }
}
