// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.FrameworkElementExtensions
{
    public static class FrameworkElementExtensions
    {
        [CanBeNull]
        public static T FindChild<T>([NotNull] this DependencyObject parent, [NotNull] string childName) where T : FrameworkElement
        {
            Assert.ArgumentNotNull(parent, nameof(parent));
            Assert.ArgumentNotNull(childName, nameof(childName));

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);

            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var frameworkElement = child as FrameworkElement;

                if (frameworkElement != null && frameworkElement.GetType() == typeof(T) && frameworkElement.Name == childName)
                {
                    return (T)frameworkElement;
                }

                var result = FindChild<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return default(T);
        }

        [NotNull]
        public static IEnumerable<T> FindChildren<T>([CanBeNull] this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (var c in FindChildren<T>(child))
                {
                    yield return c;
                }
            }
        }

        [CanBeNull]
        public static T GetAncestor<T>([NotNull] this FrameworkElement element) where T : class
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var e = VisualTreeHelper.GetParent(element) as FrameworkElement;
            if (e == null)
            {
                return null;
            }

            return e.GetAncestorOrSelf<T>();
        }

        [CanBeNull]
        public static T GetAncestorOrSelf<T>([NotNull] this FrameworkElement element) where T : class
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var e = element;

            while (e != null)
            {
                object o = e as T;
                if (o != null)
                {
                    return (T)o;
                }

                e = VisualTreeHelper.GetParent(e) as FrameworkElement;
            }

            return default(T);
        }

        public static T GetInterfaceAncestorOrSelf<T>([NotNull] this FrameworkElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            var e = element;

            while (e != null)
            {
                if (e is T)
                {
                    object o = e;
                    return (T)o;
                }

                e = VisualTreeHelper.GetParent(e) as FrameworkElement;
            }

            return default(T);
        }
    }
}
