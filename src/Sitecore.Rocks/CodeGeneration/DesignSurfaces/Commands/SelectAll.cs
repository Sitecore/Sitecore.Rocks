// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class SelectAll : CommandBase
    {
        public SelectAll()
        {
            Text = "Select All";
            Group = "Selecting";
            SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as DesignSurfaceContext;
            if (context == null)
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

            foreach (var shape in context.DesignSurface.Shapes)
            {
                context.DesignSurface.AddSelectedItem(shape);
            }
        }
    }
}
