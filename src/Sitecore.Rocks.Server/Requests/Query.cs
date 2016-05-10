// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Pipelines.Search;
using Sitecore.Rocks.Server.Extensions.XmlTextWriterExtensions;
using Sitecore.Search;

namespace Sitecore.Rocks.Server.Requests
{
    public class Query
    {
        [NotNull]
        public string Execute([NotNull] string queryText)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));

            var args = new SearchArgs(queryText)
            {
                Type = SearchType.ContentEditor,
                Limit = 30,
            };

            using (new LongRunningOperationWatcher(250, "Search pipeline from instant search for '{0} query", queryText))
            {
                CorePipeline.Run("search", args);
            }

            var results = args.Result;

            if (results.Count == 0)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("hits");

            foreach (var category in results.Categories)
            {
                foreach (var hit in category)
                {
                    var item = hit.GetObject<Item>();
                    if (item == null)
                    {
                        continue;
                    }

                    output.WriteItemHeader(item, category.Name);
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
