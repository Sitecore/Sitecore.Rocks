// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.ObjectModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Libraries
{
    public class ObservableItemCollection : ObservableCollection<IItem>
    {
        public ObservableItemCollection([NotNull] IDynamicLibrary library)
        {
            Assert.ArgumentNotNull(library, nameof(library));

            Library = library;
        }

        [NotNull]
        protected IDynamicLibrary Library { get; }

        public void Refresh()
        {
            Library.Refresh();
        }
    }
}
