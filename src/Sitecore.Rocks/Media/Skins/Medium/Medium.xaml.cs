// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Media.Skins.Medium
{
    [MediaSkin("Medium Icons", 300)]
    public partial class Medium : IMediaSkin
    {
        public Medium()
        {
            InitializeComponent();
            ListBox.Initialize(this, GetHeader);
        }

        public MediaViewer MediaViewer { get; set; }

        [CanBeNull]
        public Site Site
        {
            get { return ListBox.Site; }

            set { ListBox.Site = value; }
        }

        protected List<ItemHeader> Items { get; set; }

        public void Clear()
        {
            ListBox.Clear();
        }

        public Control GetControl()
        {
            return this;
        }

        public IEnumerable<ItemHeader> GetSelectedItems()
        {
            return ListBox.GetSelectedItems();
        }

        public void Initialize(MediaViewer mediaViewer)
        {
            Assert.ArgumentNotNull(mediaViewer, nameof(mediaViewer));

            MediaViewer = mediaViewer;
        }

        public void Load(List<ItemHeader> items)
        {
            Assert.ArgumentNotNull(items, nameof(items));

            Items = items;

            ListBox.RenderItems(items);
        }

        public void Renamed(ItemHeader itemHeader, string newName)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));
            Assert.ArgumentNotNull(newName, nameof(newName));

            ListBox.Renamed(itemHeader, newName);
        }

        void IMediaSkin.Deleted(ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));

            ListBox.Deleted(itemHeader);
        }

        [NotNull]
        private UserControl GetHeader([NotNull] ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var result = new MediaIconsHeader();

            result.Initialize(itemHeader, 45, 34);

            return result;
        }
    }
}
