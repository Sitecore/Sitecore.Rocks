// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders
{
    public class PlaceHolderKeyConverter : TypeConverter
    {
        public override bool CanConvertFrom([CanBeNull] ITypeDescriptorContext context, Type sourceType)
        {
            Assert.ArgumentNotNull(sourceType, nameof(sourceType));

            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, object value)
        {
            if (value is string)
            {
                return new PlaceHolderKey((string)value);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, [CanBeNull] object value, Type destinationType)
        {
            Assert.ArgumentNotNull(destinationType, nameof(destinationType));

            if (destinationType == typeof(string))
            {
                var placeHolderKey = value as PlaceHolderKey;
                if (placeHolderKey != null)
                {
                    return placeHolderKey.Key;
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
            return false;
        }

        private void GetValues([CanBeNull] ITypeDescriptorContext context, [NotNull] List<string> values)
        {
            Debug.ArgumentNotNull(values, nameof(values));

            if (context == null)
            {
                return;
            }

            var renderingItem = context.Instance as RenderingItem;
            if (renderingItem == null)
            {
                return;
            }

            var renderingContainer = renderingItem.RenderingContainer;
            if (renderingContainer == null)
            {
                return;
            }

            // TODO: get values from layout item
            values.Add(@"Page.Stylesheets");
            values.Add(@"Page.Scripts");
            values.Add(@"Page.Code");
            values.Add(@"Page.Body");

            foreach (var rendering in renderingContainer.Renderings)
            {
                var placeHolders = rendering.PlaceHolders;
                if (string.IsNullOrEmpty(placeHolders))
                {
                    continue;
                }

                foreach (var placeHolder in placeHolders.Split(','))
                {
                    var value = placeHolder.Trim();

                    foreach (var dynamicProperty in rendering.DynamicProperties)
                    {
                        var v = dynamicProperty.Value != null ? dynamicProperty.Value.ToString() : string.Empty;

                        value = value.Replace(@"$" + dynamicProperty.Name, v);
                    }

                    values.Add(value);
                }
            }

            values.Sort();
        }
    }
}
