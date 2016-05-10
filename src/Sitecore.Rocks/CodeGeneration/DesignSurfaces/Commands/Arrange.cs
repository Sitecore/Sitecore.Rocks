// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Commands;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces.Commands
{
    [Command]
    public class Arrange : CommandBase
    {
        public Arrange()
        {
            Text = Resources.Arrange_Arrange_Arrange;
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

            double x = 16;
            double y = 16;
            var bottom = y;

            foreach (var shape in context.DesignSurface.Shapes)
            {
                var frameworkElement = shape as FrameworkElement;
                if (frameworkElement == null)
                {
                    continue;
                }

                var w = frameworkElement.ActualWidth;
                var h = frameworkElement.ActualHeight;

                if (x + w + 16 > context.DesignSurface.ActualWidth)
                {
                    x = 16;
                    y = bottom + 16;
                    bottom = y;
                }

                shape.SetPosition(new Point(x, y));

                x += w + 16;

                if (bottom < y + h)
                {
                    bottom = y + h;
                }
            }

            context.DesignSurface.SetModifiedFlag(true);
        }
    }
}
