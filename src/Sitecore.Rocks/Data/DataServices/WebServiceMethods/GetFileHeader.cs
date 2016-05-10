// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XmlNodeExtensions;

namespace Sitecore.Rocks.Data.DataServices.WebServiceMethods
{
    public static class GetFileHeader
    {
        [NotNull]
        public static ItemHeader Call([NotNull] XmlNode node, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(node, nameof(node));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var name = node.GetAttributeValue("name");
            var path = node.GetAttributeValue("path");
            var type = node.GetAttributeValue("type");

            return new ItemHeader
            {
                Name = name,
                Path = path,
                ItemUri = new ItemUri(databaseUri, ItemId.Empty),
                HasChildren = type == @"folder"
            };
        }
    }
}
