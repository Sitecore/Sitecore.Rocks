// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Server.Requests.Serialization
{
    public class RevertItem : UpdateItem
    {
        public RevertItem()
        {
            ForceUpdate = true;
        }
    }
}
