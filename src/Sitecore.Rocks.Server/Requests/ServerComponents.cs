// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests
{
    public class ServerComponents
    {
        [NotNull]
        public string Execute()
        {
            var path = FileUtil.MapPath("/bin");

            var doc = new XDocument();
            var root = new XElement("components");
            doc.Add(root);

            foreach (var file in Directory.GetFiles(path, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(file) ?? string.Empty;
                if (!name.StartsWith("Sitecore.Rocks", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var element = new XElement("component");

                element.SetAttributeValue("name", name);

                var info = FileVersionInfo.GetVersionInfo(file);
                var version = new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);

                element.SetAttributeValue("version", version.ToString());

                root.Add(element);
            }

            return doc.ToString();
        }
    }
}
