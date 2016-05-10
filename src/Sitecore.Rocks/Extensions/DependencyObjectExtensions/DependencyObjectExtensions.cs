// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.DependencyObjectExtensions
{
    public static class DependencyObjectExtensions
    {
        public static bool IsContainedIn([NotNull] this DependencyObject obj, [NotNull] object container)
        {
            Assert.ArgumentNotNull(obj, nameof(obj));
            Assert.ArgumentNotNull(container, nameof(container));

            while (obj != null)
            {
                if (obj == container)
                {
                    return true;
                }

                obj = VisualTreeHelper.GetParent(obj);
            }

            return false;
        }
    }
}
