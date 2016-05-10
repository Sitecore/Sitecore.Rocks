// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class SelectionHost
    {
        public void Track([NotNull] IPane pane, [CanBeNull] object selection)
        {
            Assert.ArgumentNotNull(pane, nameof(pane));

            var objects = new[]
            {
                selection
            };

            TrackSelection.SelectObjects(pane, objects);
        }

        public void Track([NotNull] IPane pane, [CanBeNull] IEnumerable<object> selection)
        {
            Assert.ArgumentNotNull(pane, nameof(pane));

            TrackSelection.SelectObjects(pane, selection);
        }
    }
}
