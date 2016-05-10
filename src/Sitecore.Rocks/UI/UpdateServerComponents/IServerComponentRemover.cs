// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    public interface IServerComponentRemover
    {
        bool CanRemove(RemoveServerComponentOptions options);

        bool Remove(RemoveServerComponentOptions options);
    }
}
