// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.DataBindings
{
    public class DataBindingConverter : TypeConverter
    {
        public DataBindingConverter()
        {
        }

        public DataBindingConverter([CanBeNull] List<Tuple<string, string>> values)
        {
            Values = values;
        }

        [CanBeNull]
        public List<Tuple<string, string>> Values { get; }

        public override bool CanConvertFrom([CanBeNull] ITypeDescriptorContext context, Type sourceType)
        {
            Assert.ArgumentNotNull(sourceType, nameof(sourceType));

            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new DataBinding((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, [CanBeNull] object value, Type destinationType)
        {
            Assert.ArgumentNotNull(destinationType, nameof(destinationType));

            if (destinationType == typeof(string))
            {
                var binding = value as DataBinding;
                if (binding != null)
                {
                    return binding.Binding;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        [NotNull]
        public override StandardValuesCollection GetStandardValues([CanBeNull] ITypeDescriptorContext context)
        {
            var values = new List<string>();

            GetValues(context, values);

            return new StandardValuesCollection(values);
        }

        public override bool GetStandardValuesSupported([CanBeNull] ITypeDescriptorContext context)
        {
            return true;
        }

        private void GetValues([CanBeNull] ITypeDescriptorContext context, [NotNull] List<string> values)
        {
            Debug.ArgumentNotNull(values, nameof(values));

            if (context == null)
            {
                return;
            }

            var descriptor = context.PropertyDescriptor as DynamicPropertyDescriptor;
            if (descriptor == null)
            {
                return;
            }

            var renderingItem = descriptor.DynamicProperty.Tag as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var renderingContainer = renderingItem.RenderingContainer;
            if (renderingContainer == null)
            {
                return;
            }

            renderingContainer.GetDataBindingValues(renderingItem, descriptor.DynamicProperty, values);

            if (string.Compare(descriptor.DynamicProperty.TypeName, @"checkbox", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                values.Insert(0, @"False");
                values.Insert(0, @"True");
            }

            if (string.Compare(descriptor.DynamicProperty.TypeName, @"tristate", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                values.Insert(0, @"False");
                values.Insert(0, @"True");
                values.Insert(0, @"Indetermined");
            }

            if (Values != null)
            {
                foreach (var value in Values.Select(i => i.Item1).Reverse().ToList())
                {
                    values.Insert(0, value);
                }
            }
        }
    }
}
