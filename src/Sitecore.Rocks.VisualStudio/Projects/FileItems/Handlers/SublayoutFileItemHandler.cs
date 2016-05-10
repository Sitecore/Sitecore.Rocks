// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Projects.FileItems.Handlers
{
    [FileItemHandler]
    public class SublayoutFileItemHandler : PathBasedFileItemHandler
    {
        public SublayoutFileItemHandler()
        {
            TemplateId = IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutRenderingsSublayout);
            FieldId = new FieldId(IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutRenderingsSublayoutDataPath).ToGuid());
        }

        public override bool CanHandle(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            return string.Compare(Path.GetExtension(fileName), ".ascx", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override string GetItemPath(string itemPath)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            if (itemPath.StartsWith(@"Layouts\", StringComparison.InvariantCultureIgnoreCase))
            {
                itemPath = itemPath.Mid(8);
            }

            return @"/sitecore/layout/sublayouts/" + itemPath.Replace('\\', '/');
        }

        public override ItemId GetRootItemId()
        {
            return IdManager.GetItemId("/sitecore/layout/Sublayouts");
        }

        public override string GetTemplateName()
        {
            return @"Sublayout";
        }
    }
}
