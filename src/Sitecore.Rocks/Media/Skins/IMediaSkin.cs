// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media.Skins
{
    public interface IMediaSkin
    {
        [NotNull]
        MediaViewer MediaViewer { get; }

        [CanBeNull]
        Site Site { get; set; }

        void Clear();

        void Deleted([NotNull] ItemHeader itemHeader);

        [NotNull]
        Control GetControl();

        [NotNull]
        IEnumerable<ItemHeader> GetSelectedItems();

        void Initialize([NotNull] MediaViewer mediaViewer);

        void Load([NotNull] List<ItemHeader> items);

        void Renamed([NotNull] ItemHeader itemHeader, [NotNull] string newName);
    }
}
