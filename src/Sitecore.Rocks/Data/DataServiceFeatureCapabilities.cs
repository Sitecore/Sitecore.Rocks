// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.Data
{
    [Flags]
    public enum DataServiceFeatureCapabilities
    {
        Publish = 1,

        RebuildSearchIndex = 2,

        RebuildLinkDatabase = 4,

        EditTemplate = 8,

        CopyPasteItemXml = 16,

        InsertOptions = 32,

        Projects = 128,

        EditLayouts = 256,

        Jobs = 512,

        Logs = 1024,

        Execute = 2048
    }
}
