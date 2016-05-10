// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    public interface ISearcher
    {
        void Search([NotNull] CommandyContext context);

        void SetActiveMode([NotNull] IMode mode);
    }
}
