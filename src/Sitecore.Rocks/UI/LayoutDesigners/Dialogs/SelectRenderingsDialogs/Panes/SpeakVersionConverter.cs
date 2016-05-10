// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Panes
{
    public sealed class SpeakVersionConverter : IValueConverter
    {
        [CanBeNull]
        public object Convert([CanBeNull] object value, [CanBeNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            var item = value as IItemData;
            return item?.GetData("ex.speakversion");
        }

        [NotNull]
        public object ConvertBack([CanBeNull] object value, [CanBeNull] Type targetType, [CanBeNull] object parameter, [CanBeNull] CultureInfo culture)
        {
            throw new NotSupportedException("MethodToValueConverter can only be used for one way conversion.");
        }
    }
}
