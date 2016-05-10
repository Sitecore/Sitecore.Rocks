// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Filters
{
    public interface IRenderingSelectorFilter
    {
        [NotNull]
        string Name { get; }

        double Priority { get; }

        void AddToRecent([NotNull] ItemHeader itemHeader);

        void GetRenderings([NotNull] RenderingSelectorFilterParameters parameters, [NotNull] GetItemsCompleted<ItemHeader> completed);

        void SetRenderings([NotNull] IEnumerable<ItemHeader> renderings);
    }
}
