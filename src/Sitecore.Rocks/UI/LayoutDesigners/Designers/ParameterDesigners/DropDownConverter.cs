// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers.ParameterDesigners
{
    public class DropDownConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert([CanBeNull] object value, [NotNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            Assert.ArgumentNotNull(targetType, nameof(targetType));

            if (value == null)
            {
                return null;
            }

            var converter = TypeDescriptor.GetConverter(targetType);

            try
            {
                if (converter.CanConvertFrom(value.GetType()))
                {
                    return converter.ConvertFrom(value);
                }

                return converter.ConvertFrom(value.ToString());
            }
            catch (Exception)
            {
                return value;
            }
        }

        [CanBeNull]
        public object ConvertBack([CanBeNull] object value, [NotNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            Assert.ArgumentNotNull(targetType, nameof(targetType));

            if (value == null)
            {
                return null;
            }

            if (targetType == typeof(DropDownValue))
            {
                return new DropDownValue(new Tuple<string, string>(value.ToString(), value.ToString()));
            }

            return null;
        }
    }
}
