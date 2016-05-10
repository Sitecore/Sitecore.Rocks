// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects.FileItems
{
    public static class FileItemDropManager
    {
        public static void Initialize()
        {
            Notifications.ItemTreeViewDragOver += DragOver;
            Notifications.ItemTreeViewDrop += Drop;
        }

        private static void DragOver([NotNull] object sender, [NotNull] ItemTreeViewItem item, [NotNull] DragEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(args, nameof(args));

            if (!args.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS"))
            {
                return;
            }

            args.Effects = DragDropEffects.None;

            if ((item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) != DataServiceFeatureCapabilities.Execute)
            {
                return;
            }

            var fileName = args.Data.GetData(@"Text") as string ?? string.Empty;
            try
            {
                if (!File.Exists(fileName))
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            var fileItemHandler = FileItemManager.GetFileItemHandler(fileName);
            if (fileItemHandler == null)
            {
                return;
            }

            Project project = null;
            string relativeFileName = null;

            foreach (var proj in ProjectManager.Projects)
            {
                relativeFileName = proj.GetRelativeFileName(fileName);
                if (relativeFileName == fileName)
                {
                    continue;
                }

                project = proj;
                break;
            }

            if (project == null)
            {
                return;
            }

            if (project.Site != item.ItemUri.Site)
            {
                return;
            }

            var projectItem = project.GetProjectItem(relativeFileName);
            if (projectItem != null)
            {
                return;
            }

            args.Effects = DragDropEffects.Copy;
        }

        private static void Drop([NotNull] object sender, [NotNull] ItemTreeViewItem item, [NotNull] DragEventArgs args)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(args, nameof(args));

            if (!args.Data.GetDataPresent(@"CF_VSSTGPROJECTITEMS"))
            {
                return;
            }

            var fileName = args.Data.GetData(@"Text") as string ?? string.Empty;
            try
            {
                if (!File.Exists(fileName))
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            var fileItemHandler = FileItemManager.GetFileItemHandler(fileName);
            if (fileItemHandler == null)
            {
                return;
            }

            Project project = null;
            string relativeFileName = null;

            foreach (var proj in ProjectManager.Projects)
            {
                relativeFileName = proj.GetRelativeFileName(fileName);
                if (relativeFileName == fileName)
                {
                    continue;
                }

                if (proj.Site != item.ItemUri.Site)
                {
                    continue;
                }

                project = proj;
                break;
            }

            if (project == null)
            {
                return;
            }

            var projectItem = project.GetProjectItem(relativeFileName) as ProjectFileItem;
            if (projectItem != null)
            {
                if (projectItem.Items.Any())
                {
                    return;
                }
            }

            projectItem = ProjectFileItem.Load(project, fileName);
            project.Add(projectItem);
            project.Save();

            var itemPath = Path.Combine(item.GetPath(), Path.GetFileNameWithoutExtension(fileName) ?? string.Empty);

            fileItemHandler.Handle(item.ItemUri.DatabaseName, projectItem, itemPath, (s, e) => item.Refresh());
        }
    }
}
