// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell
{
    public class VsTrackSelection
    {
        public VsTrackSelection()
        {
            TrackSelection.SelectChanged += SelectChanged;
        }

        private void SelectChanged([NotNull] IPane pane, [CanBeNull] IEnumerable<object> objects)
        {
            Debug.ArgumentNotNull(pane, nameof(pane));

            var servicePane = pane as IServicePane;
            if (servicePane == null)
            {
                return;
            }

            var trackSelection = servicePane.GetVsService(typeof(STrackSelection)) as ITrackSelection;
            if (trackSelection == null)
            {
                return;
            }

            var list = new ArrayList();

            if (objects != null)
            {
                foreach (var o in objects)
                {
                    list.Add(o);
                }
            }

            var container = new SelectionContainer
            {
                SelectedObjects = list
            };

            trackSelection.OnSelectChange(container);
        }
    }
}
