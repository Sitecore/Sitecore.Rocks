// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class Redo : CommandBase
    {
        public Redo()
        {
            Text = "Redo";
            Group = "Undo";
            SortingValue = 1100;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return false;
            }

            if (!context.DesignSurface.Journal.CanGoForward)
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

            if (!context.DesignSurface.Journal.CanGoForward)
            {
                return;
            }

            var root = context.DesignSurface.Journal.GoForward().ToXElement();
            if (root == null)
            {
                return;
            }

            context.DesignSurface.Canvas.Children.Clear();
            context.DesignSurface.Owner.LoadState(root);
        }
    }
}
