// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Projects.Pipelines.AddToProject;

namespace Sitecore.Rocks.Projects.AddToProject
{
    [Pipeline(typeof(AddToProjectPipeline), 1000)]
    public class AddToVisualStudioProject : PipelineProcessor<AddToProjectPipeline>
    {
        protected override void Process(AddToProjectPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (AppHost.Files.FolderExists(pipeline.ParentFileName))
            {
                foreach (var project in SitecorePackage.Instance.Dte.Solution.Projects.OfType<EnvDTE.Project>())
                {
                    if (string.IsNullOrEmpty(project.FileName))
                    {
                        continue;
                    }

                    var projectFolder = Path.GetDirectoryName(project.FileName);
                    if (string.IsNullOrEmpty(projectFolder))
                    {
                        continue;
                    }

                    if (!pipeline.NewFileName.StartsWith(projectFolder, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    project.ProjectItems.AddFromFile(pipeline.NewFileName);

                    var prj = project;
                    pipeline.Project = AppHost.Projects.OfType<Project>().FirstOrDefault(p => string.Compare(p.FileName, prj.FileName, StringComparison.InvariantCultureIgnoreCase) == 0);

                    break;
                }

                return;
            }

            var item = SitecorePackage.Instance.Dte.Solution.FindProjectItem(pipeline.ParentFileName);
            if (item == null)
            {
                return;
            }

            if (pipeline.DependOnParent)
            {
                item.ProjectItems.AddFromFile(pipeline.NewFileName);
                pipeline.Project = AppHost.Projects.OfType<Project>().FirstOrDefault(p => string.Compare(p.FileName, item.ContainingProject.FileName, StringComparison.InvariantCultureIgnoreCase) == 0);
            }
            else
            {
                var parent = item.Collection.Parent as EnvDTE.ProjectItem;
                if (parent != null)
                {
                    parent.ProjectItems.AddFromFile(pipeline.NewFileName);
                    pipeline.Project = AppHost.Projects.OfType<Project>().FirstOrDefault(p => string.Compare(p.FileName, item.ContainingProject.FileName, StringComparison.InvariantCultureIgnoreCase) == 0);
                }
            }
        }
    }
}
