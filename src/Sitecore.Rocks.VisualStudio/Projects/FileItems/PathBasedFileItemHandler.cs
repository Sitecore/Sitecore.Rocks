// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;

namespace Sitecore.Rocks.Projects.FileItems
{
    public abstract class PathBasedFileItemHandler : FileItemHandler
    {
        [NotNull]
        protected FieldId FieldId { get; set; }

        [NotNull]
        protected ItemId TemplateId { get; set; }

        public override string GetItemPath(ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            var path = projectItem.Path;

            return GetItemPath(path);
        }

        public override void Handle(DatabaseName databaseName, ProjectItem projectItem, string itemPath, ProcessedEventHandler callback)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));
            Assert.ArgumentNotNull(callback, nameof(callback));

            var projectFile = (ProjectFileItem)projectItem;
            var serverPath = @"/" + projectFile.Path.Replace('\\', '/');

            var site = projectItem.Project.Site;
            if (site == null)
            {
                return;
            }

            var databaseUri = new DatabaseUri(site, databaseName);
            if (string.IsNullOrEmpty(itemPath))
            {
                itemPath = GetItemPath(projectItem);
            }

            var name = Path.GetFileNameWithoutExtension(projectItem.Path) ?? string.Empty;

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

                // set "Path" field
                var pathField = new Field
                {
                    Value = serverPath,
                    HasValue = true
                };

                pathField.FieldUris.Add(new FieldUri(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest), FieldId));

                var fields = new List<Field>
                {
                    pathField
                };

                site.DataService.Save(databaseName, fields);

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

        public override void UpdateItemPath(ItemUri itemUri, string path)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(path, nameof(path));

            path = NormalizeItemPath(path);

            AppHost.Server.UpdateItem(itemUri, FieldId, path);
        }

        [NotNull]
        private static string NormalizeItemPath([NotNull] string path)
        {
            Debug.ArgumentNotNull(path, nameof(path));

            path = path.Replace(@"\", @"/");
            if (!path.StartsWith(@"/"))
            {
                path = @"/" + path;
            }

            return path;
        }
    }
}
