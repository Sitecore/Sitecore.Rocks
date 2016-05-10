// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows.Media.Imaging;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface IIcon
    {
        [NotNull]
        BitmapImage GetSource();
    }
}
