// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.Data
{
    [Flags]
    public enum DataServiceCapabilities
    {
        GetChildrenTemplateId = 1,

        GetItemFieldsValueList = 2
    }
}
