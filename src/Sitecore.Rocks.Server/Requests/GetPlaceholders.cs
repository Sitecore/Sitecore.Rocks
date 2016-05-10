// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.Rocks.Server.Layouts;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetPlaceholders
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, [NotNull] string renderings)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(renderings, nameof(renderings));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var result = new StringWriter();

            var item = database.GetItem(id);
            if (item != null)
            {
                FindPlaceHolders(result, item, new Dictionary<string, string>());
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(renderings);
            }
            catch
            {
                return result.ToString();
            }

            var root = doc.Root;
            if (root == null)
            {
                return result.ToString();
            }

            foreach (var renderingElement in root.Elements())
            {
                var renderingId = renderingElement.GetAttributeValue("id");
                if (string.IsNullOrEmpty(renderingId))
                {
                    continue;
                }

                var rendering = database.GetItem(renderingId);
                if (rendering == null)
                {
                    continue;
                }

                var parameters = new Dictionary<string, string>();

                foreach (var parameterElement in renderingElement.Elements())
                {
                    var name = parameterElement.GetAttributeValue("name");
                    var value = parameterElement.Value;

                    parameters[name] = value;
                }

                FindPlaceHolders(result, rendering, parameters);
            }

            return result.ToString();
        }

        private void FindPlaceHolders([NotNull] StringWriter result, [NotNull] Item item, Dictionary<string, string> parameters)
        {
            var placeholders = item["Place Holders"];
            if (!string.IsNullOrEmpty(placeholders))
            {
                var parts = placeholders.Split(',');
                foreach (var part in parts)
                {
                    var value = part.Trim();

                    foreach (var pair in parameters)
                    {
                        if (string.Compare(pair.Key, "Id", StringComparison.InvariantCultureIgnoreCase) == 0 && string.IsNullOrEmpty(pair.Value))
                        {
                            continue;
                        }

                        value = value.Replace("$" + pair.Key, pair.Value);
                    }

                    result.WriteLine(value);
                }

                return;
            }

            var placeHolders = PlaceHolderAnalyzer.Analyze(item);
            foreach (var placeHolder in placeHolders)
            {
                result.WriteLine(placeHolder);
            }
        }
    }
}
