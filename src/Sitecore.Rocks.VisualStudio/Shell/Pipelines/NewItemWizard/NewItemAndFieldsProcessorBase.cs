// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Projects;
using Sitecore.Rocks.Shell.Dialogs;

namespace Sitecore.Rocks.Shell.Pipelines.NewItemWizard
{
    public abstract class NewItemAndFieldsProcessorBase : NewItemProcessorBase
    {
        protected NewItemAndFieldsProcessorBase()
        {
            FileType = string.Empty;
            TemplateName = string.Empty;
            TemplateId = ItemId.Empty;
        }

        [NotNull]
        public string FileType { get; protected set; }

        [NotNull]
        public ItemId TemplateId { get; protected set; }

        [NotNull]
        public string TemplateName { get; protected set; }

        [NotNull]
        protected string GetFileName([NotNull] NewItemWizardPipeline pipeline, [NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            return "/" + projectItem.Path.Replace("\\", "/");
        }

        [NotNull]
        protected string GetTypeName([NotNull] NewItemWizardPipeline pipeline, [NotNull] ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            var result = pipeline.Tokens["$rootnamespace$"] + "." + pipeline.Tokens["$safeitemrootname$"] + ", " + projectItem.Project.OutputFileName;
            return result;
        }

        protected virtual void Handle([NotNull] NewItemWizardPipeline pipeline, [NotNull] DatabaseName databaseName, [NotNull] ProjectItem projectItem, [NotNull] string itemPath, [NotNull] ProcessedEventHandler callback)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(databaseName, nameof(databaseName));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemPath, nameof(itemPath));
            Debug.ArgumentNotNull(callback, nameof(callback));

            var projectFile = (ProjectFileItem)projectItem;

            var site = projectItem.Project.Site;
            if (site == null)
            {
                return;
            }

            var databaseUri = new DatabaseUri(site, databaseName);

            var name = Path.GetFileNameWithoutExtension(projectItem.Path);

            GetItemsCompleted<ItemPath> completed = delegate(IEnumerable<ItemPath> items)
            {
                if (!items.Any())
                {
                    callback(this, new ProcessedEventArgs(Resources.PathBasedFileItemHandler_Create_failed, Resources.PathBasedFileItemHandler_Create_Path_was_not_created));
                    return;
                }

                // create item
                var parentUri = items.First().ItemUri;
                var templateuri = new ItemUri(databaseUri, TemplateId);

                var itemUri = site.DataService.AddFromTemplate(parentUri, templateuri, name);

                UpdateFields(pipeline, projectItem, itemUri);

                // done
                projectFile.Items.Add(itemUri);

                callback(this, new ProcessedEventArgs(Resources.PathBasedFileItemHandler_Create_created, Path.GetDirectoryName(itemPath) + @"/" + name));

                Notifications.RaiseItemAdded(this, new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest), parentUri);

                if (AppHost.CurrentContentTree != null)
                {
                    AppHost.CurrentContentTree.Locate(itemUri);
                }
            };

            site.DataService.CreateItemPath(databaseUri, (Path.GetDirectoryName(itemPath) ?? string.Empty).Replace('\\', '/'), completed);
        }

        protected override void Process(NewItemWizardPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            string fileType;
            if (!pipeline.Tokens.TryGetValue("$filetype$", out fileType))
            {
                return;
            }

            if (fileType != FileType)
            {
                return;
            }

            pipeline.Abort();

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

            var itemPath = string.Empty;

            if (pipeline.DatabaseName == null)
            {
                var d = new NewItemWizardDialog();
                d.Initialize(site, TemplateName);

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

                itemPath = d.SelectedPath ?? string.Empty;
                if (!string.IsNullOrEmpty(itemPath))
                {
                    itemPath += "/" + Path.GetFileNameWithoutExtension(pipeline.Item.Name);
                }
            }

            var fileName = project.GetProjectItemFileName(pipeline.Item);
            var projectItem = ProjectFileItem.Load(project, fileName);
            project.Add(projectItem);

            var busy = true;

            Handle(pipeline, pipeline.DatabaseName, projectItem, itemPath, (sender, args) => busy = false);

            AppHost.DoEvents(ref busy);

            pipeline.ProjectItem = projectItem;
        }

        protected virtual void UpdateFields([NotNull] NewItemWizardPipeline pipeline, [NotNull] ProjectItem projectItem, [NotNull] ItemUri itemUri)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
        }
    }
}
