// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Projects.FileItems.Handlers
{
    [FileItemHandler]
    public class LayoutFileItemHandler : PathBasedFileItemHandler
    {
        public LayoutFileItemHandler()
        {
            TemplateId = IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutLayout);
            FieldId = new FieldId(IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutLayoutDataPath).ToGuid());
        }

        public override bool CanHandle(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return string.Compare(Path.GetExtension(fileName), ".aspx", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        [Localizable(false)]
        public override string GetItemPath(string itemPath)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            if (itemPath.StartsWith("Layouts\\", StringComparison.InvariantCultureIgnoreCase))
            {
                itemPath = itemPath.Mid(8);
            }

            return "/sitecore/layout/layouts/" + itemPath.Replace('\\', '/');
        }

        public override ItemId GetRootItemId()
        {
            return IdManager.GetItemId("/sitecore/layout/Layouts");
        }

        [Localizable(false)]
        public override string GetTemplateName()
        {
            return @"Layout";
        }
    }
}
