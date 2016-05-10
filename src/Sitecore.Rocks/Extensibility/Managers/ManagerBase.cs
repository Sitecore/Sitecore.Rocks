// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Extensibility.Managers
{
    [Manager]
    public abstract class ManagerBase : IManager
    {
        protected virtual void Initialize()
        {
        }

        void IManager.Initialize()
        {
            Initialize();
        }
    }
}
