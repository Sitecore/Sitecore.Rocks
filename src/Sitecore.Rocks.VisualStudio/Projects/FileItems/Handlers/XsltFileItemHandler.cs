// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Projects.FileItems.Handlers
{
    [FileItemHandler]
    public class XsltFileItemHandler : PathBasedFileItemHandler
    {
        public XsltFileItemHandler()
        {
            TemplateId = IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutRenderingsXslRendering);
            FieldId = new FieldId(IdManager.GetItemId(IdManager.SitecoreTemplatesSystemLayoutRenderingsXslRenderingDataPath).ToGuid());
        }

        public override bool CanHandle(string fileName)
        {
            Assert.ArgumentNotNull(fileName, nameof(fileName));

            var extension = Path.GetExtension(fileName);

            return string.Compare(extension, ".xsl", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(extension, ".xslt", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override string GetItemPath(string itemPath)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            if (itemPath.StartsWith(@"Xsl\", StringComparison.InvariantCultureIgnoreCase))
            {
                itemPath = itemPath.Mid(4);
            }

            return @"/sitecore/layout/renderings/" + itemPath.Replace('\\', '/');
        }

        public override ItemId GetRootItemId()
        {
            return IdManager.GetItemId("/sitecore/layout/Renderings");
        }

        public override string GetTemplateName()
        {
            return "Xsl Rendering";
        }
    }
}
