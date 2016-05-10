// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.ProjectItemExtensions;

namespace Sitecore.Rocks.Projects.Commands
{
    public abstract class SolutionCommand : CommandBase
    {
        protected bool AnyItem([NotNull] IEnumerable<EnvDTE.ProjectItem> items, [NotNull] Predicate<EnvDTE.ProjectItem> predicate)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            if (!items.Any())
            {
                return false;
            }

            foreach (var item in items)
            {
                if (IsAnyItem(item, predicate))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool AnyProjectItem([NotNull] IEnumerable<EnvDTE.ProjectItem> items, [NotNull] Predicate<ProjectItem> predicate)
        {
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            if (!items.Any())
            {
                return false;
            }

            foreach (var item in items)
            {
                var project = ProjectManager.GetProject(item);
                if (project == null)
                {
                    continue;
                }

                var path = project.GetProjectItemFileName(item);

                foreach (var projectItem in project.ProjectItems.OfType<ProjectItem>())
                {
                    if (!projectItem.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }

                    if (predicate(projectItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        [NotNull]
        protected List<EnvDTE.ProjectItem> GetProjectItems([NotNull] List<SelectedItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var result = new List<EnvDTE.ProjectItem>();

            if (!selectedItems.Any())
            {
                return result;
            }

            foreach (var selectedItem in selectedItems)
            {
                object value;

                selectedItem.Hierarchy.GetProperty(selectedItem.ItemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out value);

                var projectItem = value as EnvDTE.ProjectItem;
                if (projectItem != null)
                {
                    result.Add(projectItem);
                }

                var project = value as EnvDTE.Project;
                if (project == null)
                {
                    continue;
                }

                foreach (var item in project.ProjectItems)
                {
                    var i = item as EnvDTE.ProjectItem;
                    if (i != null)
                    {
                        result.Add(i);
                    }
                }
            }

            return result;
        }

        [NotNull]
        protected List<EnvDTE.Project> GetProjects([NotNull] List<SelectedItem> selectedItems)
        {
            Debug.ArgumentNotNull(selectedItems, nameof(selectedItems));

            var result = new List<EnvDTE.Project>();

            if (selectedItems.Count == 0)
            {
                return result;
            }

            foreach (var selectedItem in selectedItems)
            {
                object value;

                ErrorHandler.ThrowOnFailure(selectedItem.Hierarchy.GetProperty(selectedItem.ItemId, (int)__VSHPROPID.VSHPROPID_ExtObject, out value));

                var project = value as EnvDTE.Project;
                if (project != null)
                {
                    result.Add(project);
                }
            }

            return result;
        }

        [NotNull]
        protected List<SelectedItem> GetSelectedItems()
        {
            var result = new List<SelectedItem>();

            var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (monitorSelection == null)
            {
                throw Exceptions.InvalidOperation();
            }

            var hierarchyPtr = IntPtr.Zero;
            var selectionContainer = IntPtr.Zero;

            try
            {
                IVsMultiItemSelect multiItemSelect;
                uint itemId;

                ErrorHandler.ThrowOnFailure(monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemId, out multiItemSelect, out selectionContainer));

                if (itemId == VSConstants.VSITEMID_SELECTION)
                {
                    uint count;
                    int isSingleHierarchy;

                    multiItemSelect.GetSelectionInfo(out count, out isSingleHierarchy);

                    var items = new VSITEMSELECTION[count];
                    multiItemSelect.GetSelectedItems(0, count, items);

                    foreach (var item in items)
                    {
                        result.Add(new SelectedItem(item.pHier, item.itemid));
                    }
                }
                else
                {
                    if (hierarchyPtr == IntPtr.Zero)
                    {
                        return result;
                    }

                    var hierarchy = (IVsHierarchy)Marshal.GetObjectForIUnknown(hierarchyPtr);

                    result.Add(new SelectedItem(hierarchy, itemId));
                }
            }
            finally
            {
                if (hierarchyPtr != IntPtr.Zero)
                {
                    Marshal.Release(hierarchyPtr);
                }

                if (selectionContainer != IntPtr.Zero)
                {
                    Marshal.Release(selectionContainer);
                }
            }

            return result;
        }

        protected bool HasProject([NotNull] IEnumerable<EnvDTE.ProjectItem> items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items)
            {
                var project = ProjectManager.GetProject(item.ContainingProject.FileName);
                if (project == null)
                {
                    return false;
                }
            }

            return true;
        }

        protected bool IsAnyItem([NotNull] EnvDTE.ProjectItem item, [NotNull] Predicate<EnvDTE.ProjectItem> predicate)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(predicate, nameof(predicate));

            if (!IsFolder(item))
            {
                if (predicate.Invoke(item))
                {
                    return true;
                }
            }

            foreach (var subProjectItem in item.ProjectItems)
            {
                var subitem = subProjectItem as EnvDTE.ProjectItem;
                if (subitem == null)
                {
                    continue;
                }

                if (IsAnyItem(subitem, predicate))
                {
                    return true;
                }
            }

            return false;
        }

        protected bool IsFolder([NotNull] EnvDTE.ProjectItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return item.GetFileName().EndsWith(@"\");
        }

        protected class SelectedItem
        {
            public SelectedItem([NotNull] IVsHierarchy hierarchy, uint itemId)
            {
                Assert.ArgumentNotNull(hierarchy, nameof(hierarchy));

                Hierarchy = hierarchy;
                ItemId = itemId;
            }

            [NotNull]
            public IVsHierarchy Hierarchy { get; }

            public uint ItemId { get; }
        }
    }
}
