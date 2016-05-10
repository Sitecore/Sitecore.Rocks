// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.LayoutDesigners.Items;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.UI.LayoutDesigners.Commands
{
    [Command, ToolbarElement(typeof(LayoutDesignerContext), 1000, "Home", "Editing", Icon = "Resources/32x32/Edit.png")]
    public class Edit : CommandBase, IToolbarElement
    {
        public Edit()
        {
            Text = "Edit";
            Group = "Edit";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as LayoutDesignerContext;
            if (context == null)
            {
                return false;
            }

            return context.SelectedItems.Count() == 1 && context.SelectedItems.All(r => r is RenderingItem);
        }

        public override void Execute(object parameter)
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

            AppHost.Windows.OpenPropertyWindow();

            /*
            try
            {
                var dialog = new EditRenderingDialog(renderingItem);
                AppHost.Shell.ShowDialog(dialog);

                context.LayoutDesigner.LayoutDesignerView.UpdateTracking();
            }
            catch (Exception ex)
            {
                AppHost.Shell.HandleException(ex);
            }
            */
        }
    }
}
