// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Projects.FileItems.Handlers
{
    [FileItemHandler]
    public class CshtmlFileItemHandler : PathBasedFileItemHandler
    {
        public CshtmlFileItemHandler()
        {
            TemplateId = new ItemId(new Guid("{99F8905D-4A87-4EB8-9F8B-A9BEBFB3ADD6}"));
            FieldId = new FieldId(new Guid("{51B435BC-F7B9-478A-9C51-52916AF96FF5}"));
        }

        public override bool CanHandle(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var extension = Path.GetExtension(fileName);

            return string.Compare(extension, ".cshtml", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override string GetItemPath(string itemPath)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            if (itemPath.StartsWith(@"Views\", StringComparison.InvariantCultureIgnoreCase))
            {
                itemPath = itemPath.Mid(6);
            }

            return @"/sitecore/layout/renderings/" + itemPath.Replace('\\', '/');
        }

        public override ItemId GetRootItemId()
        {
            return IdManager.GetItemId("/sitecore/layout/Renderings");
        }

        public override string GetTemplateName()
        {
            return "View rendering";
        }
    }
}
