// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    public abstract class SearcherBase : ISearcher
    {
        public abstract void Search(CommandyContext context);

        public virtual void SetActiveMode([NotNull] IMode mode)
        {
            Assert.ArgumentNotNull(mode, nameof(mode));
        }
    }
}
