// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectRenderingsDialogs.Filters
{
    public abstract class RenderingSelectorFilterBase : IRenderingSelectorFilter
    {
        [Localizable(true)]
        public string Name { get; protected set; }

        public double Priority { get; protected set; }

        public virtual void AddToRecent(ItemHeader itemHeader)
        {
            Assert.ArgumentNotNull(itemHeader, nameof(itemHeader));
        }

        public abstract void GetRenderings(RenderingSelectorFilterParameters parameters, GetItemsCompleted<ItemHeader> completed);

        public abstract void SetRenderings(IEnumerable<ItemHeader> renderings);
    }
}
