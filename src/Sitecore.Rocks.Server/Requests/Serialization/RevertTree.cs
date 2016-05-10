// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Server.Requests.Serialization
{
    public class RevertTree : UpdateTree
    {
        public RevertTree()
        {
            ForceUpdate = true;
        }
    }
}
