// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns
{
    public class DropDownValueConverter : TypeConverter
    {
        public DropDownValueConverter()
        {
            Values = new List<Tuple<string, string>>();
        }

        public DropDownValueConverter([NotNull] List<Tuple<string, string>> values)
        {
            Assert.ArgumentNotNull(values, nameof(values));

            Values = values;
        }

        [NotNull]
        public List<Tuple<string, string>> Values { get; }

        public override bool CanConvertFrom([CanBeNull] ITypeDescriptorContext context, Type sourceType)
        {
            Assert.ArgumentNotNull(sourceType, nameof(sourceType));

            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, object value)
        {
            var text = value as string;
            if (text != null)
            {
                var item = Values.FirstOrDefault(i => i.Item1 == text || i.Item2 == text) ?? new Tuple<string, string>(text, text);

                return new DropDownValue(item);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, [CanBeNull] object value, Type destinationType)
        {
            Assert.ArgumentNotNull(destinationType, nameof(destinationType));

            if (destinationType == typeof(string))
            {
                var dropDownValue = value as DropDownValue;
                if (dropDownValue != null)
                {
                    if (dropDownValue.Value != null)
                    {
                        return dropDownValue.Value.Item1;
                    }
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        [NotNull]
        public override StandardValuesCollection GetStandardValues([CanBeNull] ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Values.Select(i => i.Item1).ToList());
        }

        public override bool GetStandardValuesSupported([CanBeNull] ITypeDescriptorContext context)
        {
            return true;
        }
    }
}
