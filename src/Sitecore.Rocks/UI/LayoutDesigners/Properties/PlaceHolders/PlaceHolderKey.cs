// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.PlaceHolders
{
    [TypeConverter(typeof(PlaceHolderKeyConverter))]
    public class PlaceHolderKey
    {
        private static readonly PlaceHolderKey empty = new PlaceHolderKey("");

        public PlaceHolderKey([CanBeNull] string key)
        {
            Key = key;
        }

        [NotNull]
        public static PlaceHolderKey Empty => empty;

        [CanBeNull]
        public string Key { get; set; }

        public override string ToString()
        {
            return Key ?? string.Empty;
        }
    }
}
