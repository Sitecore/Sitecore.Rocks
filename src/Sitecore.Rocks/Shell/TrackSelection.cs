// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell
{
    public static class TrackSelection
    {
        public delegate void SelectChangedEventHandler([NotNull] IPane pane, [CanBeNull] IEnumerable<object> objects);

        [CanBeNull]
        public static IPane CurrentPane { get; private set; }

        [CanBeNull]
        public static IEnumerable<object> CurrentSelection { get; private set; }

        public static event SelectChangedEventHandler SelectChanged;

        public static void SelectObjects([NotNull] IPane pane, [CanBeNull] IEnumerable<object> objects)
        {
            Assert.ArgumentNotNull(pane, nameof(pane));

            if (objects == null)
            {
                CurrentSelection = null;
                CurrentPane = null;
            }
            else
            {
                CurrentSelection = objects.ToList();
                CurrentPane = pane;
            }

            var selectChanged = SelectChanged;
            if (selectChanged != null)
            {
                selectChanged(pane, CurrentSelection);
            }
        }
    }
}
