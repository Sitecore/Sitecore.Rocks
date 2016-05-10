// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class Undo : CommandBase
    {
        public Undo()
        {
            Text = Resources.Undo_Undo_Undo;
            Group = "Undo";
            SortingValue = 1000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
            {
                return false;
            }

            if (!context.DesignSurface.Journal.CanGoBack)
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

            if (!context.DesignSurface.Journal.CanGoBack)
            {
                return;
            }

            var s = context.DesignSurface.Journal.GoBack();
            if (string.IsNullOrEmpty(s))
            {
                return;
            }

            var root = s.ToXElement();
            if (root == null)
            {
                return;
            }

            context.DesignSurface.Canvas.Children.Clear();
            context.DesignSurface.Owner.LoadState(root);
        }
    }
}
