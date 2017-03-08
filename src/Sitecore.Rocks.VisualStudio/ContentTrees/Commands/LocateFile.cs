// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using EnvDTE;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.ContentTrees.Commands
{
    [Command]
    public class LocateFile : CommandBase
    {
        public LocateFile()
        {
            Text = Resources.LocateFile_LocateFile_Locate_File_in_Solution_Explorer;
            Group = "Layout";
            SortingValue = 10;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return false;
            }

            return ProjectManager.GetProjectFileItem(item.ItemUri) != null;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.SelectedItems;
            if (selectedItems.Count() != 1)
            {
                return;
            }

            var item = selectedItems.FirstOrDefault() as ItemTreeViewItem;
            if (item == null)
            {
                return;
            }

            var projectFile = ProjectManager.GetProjectFileItem(item.ItemUri);
            if (projectFile == null)
            {
                return;
            }

            var dte = SitecorePackage.Instance.Dte;

            var i = dte.Solution.FindProjectItem(projectFile.Path);
            if (i == null)
            {
                return;
            }

            var solutionExplorerPath = GetSolutionExplorerPath(projectFile.AbsoluteFileName);
            if (string.IsNullOrEmpty(solutionExplorerPath))
            {
                return;
            }

            // dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
            dte.Windows.Item("{3AE79031-E1BC-11D0-8F78-00A0C9110057}").Activate();

            var hierarchyItem = dte.ToolWindows.SolutionExplorer.GetItem(solutionExplorerPath);
            if (hierarchyItem == null)
            {
                return;
            }

            hierarchyItem.Select(vsUISelectionType.vsUISelectionTypeSelect);
        }

        [CanBeNull]
        private string GetSolutionExplorerPath([NotNull] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            var dte = SitecorePackage.Instance.Dte;
            var hierarchyItems = dte.ToolWindows.SolutionExplorer.UIHierarchyItems;

            return GetSolutionExplorerPath(hierarchyItems, path);
        }

        [CanBeNull]
        private string GetSolutionExplorerPath([NotNull] UIHierarchyItems hierarchyItems, [NotNull] string path)
        {
            Debug.ArgumentNotNull(hierarchyItems, nameof(hierarchyItems));
            Debug.ArgumentNotNull(path, nameof(path));

            foreach (var item in hierarchyItems)
            {
                var hierarchyItem = item as UIHierarchyItem;
                if (hierarchyItem == null)
                {
                    continue;
                }

                var projectItem = hierarchyItem.Object as EnvDTE.ProjectItem;

                if (projectItem != null)
                {
                    var fileName = projectItem.GetFileName();
                    if (string.Compare(fileName, path, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        return hierarchyItem.Name;
                    }

                    for (short n = 0; n < projectItem.FileCount; n++)
                    {
                        try
                        {
                            if (string.Compare(projectItem.FileNames[n], path, StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                return hierarchyItem.Name;
                            }
                        }

                            // ReSharper disable once EmptyGeneralCatchClause
                        catch
                        {
                            // silent
                        }
                    }
                }

                var child = GetSolutionExplorerPath(hierarchyItem.UIHierarchyItems, path);
                if (child != null)
                {
                    return hierarchyItem.Name + @"\" + child;
                }
            }

            return null;
        }
    }
}
