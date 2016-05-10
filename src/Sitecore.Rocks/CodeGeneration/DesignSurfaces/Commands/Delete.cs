// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class Delete : CommandBase
    {
        public Delete()
        {
            Text = Resources.Delete_Delete_Delete;
            Group = "Editing";
            SortingValue = 3000;
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

            return true;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return;
            }

            var list = new List<IShape>(context.DesignSurface.SelectedItems);
            if (list.Count == 0)
            {
                return;
            }

            for (var index = list.Count - 1; index >= 0; index--)
            {
                var shape = list[index];

                context.DesignSurface.Delete(shape);
            }

            context.DesignSurface.SetModifiedFlag(true);
        }
    }
}
