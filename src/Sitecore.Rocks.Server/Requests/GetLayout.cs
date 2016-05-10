// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Layouts;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetLayout
    {
        [NotNull]
        public string Execute([NotNull] string layout, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(layout, nameof(layout));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented
            };

            var layoutWriter = new LayoutWriter();
            layoutWriter.Write(output, database, layout);

            return writer.ToString();
        }
    }
}
