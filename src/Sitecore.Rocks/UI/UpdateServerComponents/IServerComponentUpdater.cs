// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    public interface IServerComponentUpdater
    {
        bool CanUpdate([NotNull] UpdateServerComponentOptions options);

        bool Update([NotNull] UpdateServerComponentOptions options);
    }
}
