// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using TaskDialogInterop;

namespace Sitecore.Rocks.ContentTrees.Commands.Tasks
{
    public abstract class DesignLayoutBase : CommandBase
    {
        protected bool CanDesign([NotNull] IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            var project = AppHost.Projects.GetProjectContainingLinkedItem(item.ItemUri);
            if (project == null)
            {
                return true;
            }

            var fileName = project.GetLinkedFileName(item.ItemUri);
            if (string.IsNullOrEmpty(fileName))
            {
                return true;
            }

            var options = new TaskDialogOptions
            {
                Title = "The Item is linked to a Layout File",
                CommonButtons = TaskDialogCommonButtons.None,
                MainInstruction = "The item is already linked to a Layout File.",
                MainIcon = VistaTaskDialogIcon.Warning,
                Content = "Using the designer to change the layout will not update the Layout File and the Layout File will become out of date.",
                DefaultButtonIndex = 0,
                AllowDialogCancellation = true,
                CommandButtons = new[]
                {
                    "Edit the Layout File",
                    "Design the layout"
                }
            };

            var r = TaskDialog.Show(options).CommandButtonResult;
            if (r == null)
            {
                return false;
            }

            if (r != 0)
            {
                return true;
            }

            AppHost.Files.OpenFile(project.MakeAbsoluteFileName(fileName));
            return false;
        }
    }
}
