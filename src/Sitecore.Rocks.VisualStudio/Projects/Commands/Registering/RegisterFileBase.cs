// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.Projects.Commands.Registering
{
    public abstract class RegisterFileBase : SolutionCommand
    {
        protected RegisterFileBase()
        {
            FileExtension = string.Empty;
            DefaultItemPath = string.Empty;
            DialogTitle = string.Empty;
            TemplateItemPath = string.Empty;
            PathFieldName = "Path";
        }

        [NotNull]
        protected string DefaultItemPath { get; set; }

        [NotNull]
        protected string DialogTitle { get; set; }

        [NotNull]
        protected string FileExtension { get; set; }

        [NotNull]
        protected string PathFieldName { get; set; }

        [NotNull]
        protected string TemplateItemPath { get; set; }

        public override bool CanExecute(object parameter)
        {
            IsVisible = false;

            if (!(parameter is ShellContext))
            {
                return false;
            }

            var selectedItems = GetSelectedItems();
            if (!selectedItems.Any())
            {
                return false;
            }

            var items = GetProjectItems(selectedItems);
            if (!items.Any())
            {
                return false;
            }

            if (!items.All(i => i.ContainingProject.IsValidProjectKind()))
            {
                return false;
            }

            if (AnyItem(items, item => ProjectManager.GetProject(item) == null))
            {
                return false;
            }

            IsVisible = AnyItem(items, IsFileItem);

            return IsVisible;
        }

        public override void Execute(object parameter)
        {
            var selectedItems = GetSelectedItems();
            if (!selectedItems.Any())
            {
                return;
            }

            var items = GetProjectItems(selectedItems);

            var project = ProjectManager.GetProject(items[0]);
            if (project == null)
            {
                return;
            }

            var site = project.Site;
            if (site == null)
            {
                return;
            }

            Register(site, items);
        }

        [NotNull]
        protected virtual List<ItemUri> CreateItems([NotNull] ProjectBase project, [NotNull] List<EnvDTE.ProjectItem> items, [NotNull] ItemUri parentItemUri, [NotNull] ItemUri templateUri)
        {
            Debug.ArgumentNotNull(project, nameof(project));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(parentItemUri, nameof(parentItemUri));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));

            var result = new List<ItemUri>();

            foreach (var projectItem in items)
            {
                var fileName = project.GetRelativeFileName(projectItem.GetFileName());

                var itemUri = AppHost.Server.AddFromTemplate(parentItemUri, templateUri, Path.GetFileNameWithoutExtension(fileName));
                if (itemUri == ItemUri.Empty)
                {
                    continue;
                }

                UpdateFields(projectItem, itemUri, fileName);

                result.Add(itemUri);

                AppHost.Projects.LinkItemAndFile(project, itemUri, fileName, false);
            }

            project.Save();

            return result;
        }

        [NotNull]
        protected string GetFirstClass([NotNull] EnvDTE.ProjectItem projectItem)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));

            var fileCodeModel = projectItem.FileCodeModel as FileCodeModel2;
            if (fileCodeModel == null)
            {
                return string.Empty;
            }

            var result = GetFirstClass(fileCodeModel.CodeElements);
            if (string.IsNullOrEmpty(result))
            {
                return string.Empty;
            }

            var project = ProjectManager.GetProject(projectItem);
            if (project != null)
            {
                var outputFileName = project.OutputFileName;
                result += ", " + Path.GetFileNameWithoutExtension(outputFileName);
            }

            return result;
        }

        [NotNull]
        protected string GetFirstClass([NotNull] CodeElements codeElements)
        {
            Debug.ArgumentNotNull(codeElements, nameof(codeElements));

            foreach (var element in codeElements)
            {
                var classCodeElement = element as CodeClass;
                if (classCodeElement != null)
                {
                    var result = classCodeElement.Name;

                    if (classCodeElement.Namespace != null)
                    {
                        result = classCodeElement.Namespace.Name + "." + result;
                    }

                    return result;
                }

                var namespaceCodeElement = element as CodeNamespace;
                if (namespaceCodeElement != null)
                {
                    var result = GetFirstClass(namespaceCodeElement.Members);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }

                var codeElement = element as CodeElement;
                if (codeElement != null)
                {
                    var result = GetFirstClass(codeElement.Children);
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                }
            }

            return string.Empty;
        }

        protected virtual bool IsFileItem([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return false;
            }

            var projectFile = project.GetProjectItem(item) as ProjectFileItem;
            if (projectFile != null)
            {
                var itemIds = projectFile.Items;
                if (itemIds.Count > 0)
                {
                    return false;
                }
            }

            var fileName = item.GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return false;
            }

            return string.Compare(Path.GetExtension(fileName), FileExtension, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        protected virtual void Register([NotNull] Site site, [NotNull] List<EnvDTE.ProjectItem> items)
        {
            Debug.ArgumentNotNull(site, nameof(site));
            Debug.ArgumentNotNull(items, nameof(items));

            var databaseUri = new DatabaseUri(site, new DatabaseName("master"));

            var dialog = new SelectItemDialog();
            dialog.Initialize(DialogTitle, databaseUri, DefaultItemPath);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return;
            }

            var selectedItemUri = dialog.SelectedItemUri;
            ItemUri templateUri;

            Guid guid;
            if (Guid.TryParse(TemplateItemPath, out guid))
            {
                templateUri = new ItemUri(databaseUri, new ItemId(guid));
            }
            else
            {
                templateUri = new ItemUri(databaseUri, IdManager.GetItemId(TemplateItemPath));
            }

            var item = items.FirstOrDefault();
            if (item == null)
            {
                return;
            }

            var project = ProjectManager.GetProject(item);
            if (project == null)
            {
                return;
            }

            var itemUris = CreateItems(project, items, selectedItemUri, templateUri);
            var itemUri = itemUris.FirstOrDefault();
            if (itemUri == null)
            {
                return;
            }

            if (AppHost.CurrentContentTree != null)
            {
                AppHost.CurrentContentTree.Locate(itemUri);
            }

            AppHost.OpenContentEditor(itemUri);
        }

        protected virtual void UpdateFields([NotNull] EnvDTE.ProjectItem projectItem, [NotNull] ItemUri itemUri, [NotNull] string fileName)
        {
            Debug.ArgumentNotNull(projectItem, nameof(projectItem));
            Debug.ArgumentNotNull(itemUri, nameof(itemUri));
            Debug.ArgumentNotNull(fileName, nameof(fileName));

            if (!string.IsNullOrEmpty(PathFieldName))
            {
                ItemModifier.Edit(itemUri, PathFieldName, "/" + fileName.Replace('\\', '/'));
            }
        }
    }
}
