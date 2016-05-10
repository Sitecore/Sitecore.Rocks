// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.DropDowns
{
    [TypeConverter(typeof(DropDownValueConverter))]
    public class DropDownValue
    {
        public DropDownValue([CanBeNull] Tuple<string, string> value)
        {
            Value = value;
        }

        [CanBeNull]
        public Tuple<string, string> Value { get; set; }

        public override string ToString()
        {
            if (Value == null)
            {
                return string.Empty;
            }

            return Value.Item1 ?? string.Empty;
        }
    }
}
