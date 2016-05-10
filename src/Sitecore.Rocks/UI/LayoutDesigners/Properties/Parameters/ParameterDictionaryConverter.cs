// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.Parameters
{
    public class ParameterDictionaryConverter : TypeConverter
    {
        public override bool CanConvertFrom([CanBeNull] ITypeDescriptorContext context, Type sourceType)
        {
            Assert.ArgumentNotNull(sourceType, nameof(sourceType));

            return sourceType == typeof(string) || sourceType == typeof(Dictionary<string, string>) || sourceType == typeof(ParameterDictionary) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, object value)
        {
            var s = value as string;
            if (s != null)
            {
                return new ParameterDictionary(s);
            }

            var dictionary = value as Dictionary<string, string>;
            if (dictionary != null)
            {
                return new ParameterDictionary(dictionary);
            }

            var parameterDictionary = value as ParameterDictionary;
            if (parameterDictionary != null)
            {
                return new ParameterDictionary(parameterDictionary.Parameters);
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo([CanBeNull] ITypeDescriptorContext context, [CanBeNull] CultureInfo culture, [CanBeNull] object value, Type destinationType)
        {
            Assert.ArgumentNotNull(destinationType, nameof(destinationType));

            if (destinationType == typeof(string))
            {
                var parameterDictionary = value as ParameterDictionary;
                if (parameterDictionary != null)
                {
                    return parameterDictionary.ToString();
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
