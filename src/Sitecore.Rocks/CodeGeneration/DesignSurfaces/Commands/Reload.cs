// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class Reload : CommandBase
    {
        public Reload()
        {
            Text = "Reload from Server";
            Group = "Refresh";
            SortingValue = 9998;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return false;
            }

            if (!context.DesignSurface.SelectedItems.Any())
            {
                return false;
            }

            if (!context.DesignSurface.SelectedItems.All(i => i.ShapeContent is IReloadable))
            {
                return false;
            }

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return;
            }

            var shapes = context.DesignSurface.SelectedItems.ToList();
            for (var index = shapes.Count - 1; index >= 0; index--)
            {
                var content = shapes[index].ShapeContent as IReloadable;
                if (content != null)
                {
                    content.Reload();
                }
            }

            context.DesignSurface.SetModifiedFlag(true);
        }
    }
}
