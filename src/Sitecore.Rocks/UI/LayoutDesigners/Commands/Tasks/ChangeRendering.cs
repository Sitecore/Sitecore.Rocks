// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands.Tasks
{
    [Command(Submenu = TasksSubmenu.Name), ToolbarElement(typeof(LayoutDesignerContext), 4000, "Rendering", "Tasks", Icon = "Resources/32x32/Export-XML.png", Text = "Change Rendering")]
    public class ChangeRendering : CommandBase, IToolbarElement
    {
        public ChangeRendering()
        {
            Text = "Change Rendering...";
            Group = "Renderings";
            SortingValue = 1000;
        }

        public override bool CanExecute([CanBeNull] object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.SelectedItems.Count() == 1 && context.SelectedItems.All(r => r is RenderingItem);
        }

        public override void Execute([CanBeNull] object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return;
            }

            var renderingItem = context.SelectedItems.FirstOrDefault() as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var dialog = new SelectRenderingsDialog
            {
                DatabaseUri = renderingItem.ItemUri.DatabaseUri,
                RenderingContainer = renderingItem.RenderingContainer,
                AllowMultipleRenderings = false,
                SpeakCoreVersionId = context.LayoutDesigner.SpeakCoreVersionId
            };

            if (!dialog.ShowDialog())
            {
                return;
            }

            Action<IEnumerable<RenderingItem>> getSelectedRenderings = delegate(IEnumerable<RenderingItem> selectedRenderings)
            {
                if (selectedRenderings.Count() != 1)
                {
                    return;
                }

                Action completed = () => context.LayoutDesigner.LayoutDesignerView.UpdateTracking();

                renderingItem.ChangeRendering(selectedRenderings.First(), completed);

                context.LayoutDesigner.Modified = true;
            };

            dialog.GetSelectedRenderings(getSelectedRenderings);
        }
    }
}
