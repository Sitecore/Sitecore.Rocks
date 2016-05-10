// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Reflection;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public static class DependencyHelpers
    {
        [CanBeNull]
        public static DependencyProperty GetDependencyProperty([CanBeNull] Type type, [CanBeNull] string propertyName)
        {
            DependencyProperty prop = null;

            if (type != null)
            {
                var fieldInfo = type.GetField(propertyName + "Property", BindingFlags.Static | BindingFlags.Public);

                if (fieldInfo != null)
                {
                    prop = fieldInfo.GetValue(null) as DependencyProperty;
                }
            }

            return prop;
        }

        [CanBeNull]
        public static DependencyProperty GetDependencyProperty([CanBeNull] this DependencyObject o, [CanBeNull] string propertyName)
        {
            DependencyProperty prop = null;

            if (o != null)
            {
                prop = GetDependencyProperty(o.GetType(), propertyName);
            }

            return prop;
        }

        public static bool SetIfDefault<T>([NotNull] this DependencyObject o, [CanBeNull] DependencyProperty property, T value)
        {
            Assert.ArgumentNotNull(o, nameof(o));

            if (DependencyPropertyHelper.GetValueSource(o, property).BaseValueSource == BaseValueSource.Default)
            {
                o.SetValue(property, value);

                return true;
            }

            return false;
        }
    }
}
