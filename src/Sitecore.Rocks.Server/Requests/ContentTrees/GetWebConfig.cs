// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Configuration;

namespace Sitecore.Rocks.Server.Requests.ContentTrees
{
    public class GetWebConfig
    {
        [NotNull]
        public string Execute()
        {
            var writer = new StringWriter();

            Factory.GetConfiguration().Save(writer);

            return writer.ToString();
        }
    }
}
