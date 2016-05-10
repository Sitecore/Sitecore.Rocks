// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.UI.XpathBuilder
{
    public class Evaluate
    {
        [NotNull]
        public string Execute([NotNull] string path, [NotNull] string databaseName, [NotNull] string expression, [NotNull] string mode)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(expression, nameof(expression));
            Assert.ArgumentNotNull(mode, nameof(mode));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return "Database not found";
            }

            var currentItem = database.GetItem(path);
            if (currentItem == null)
            {
                return "Item not found";
            }

            try
            {
                IEnumerable<Item> items;

                var timer = new HighResTimer(true);

                if (mode == "real")
                {
                    items = EvaluateRealXPath(currentItem, expression);
                }
                else
                {
                    items = EvaluateSitecoreXPath(currentItem, expression);
                }

                var elapsed = timer.Elapsed();

                if (items == null)
                {
                    return string.Empty;
                }

                return FormatItems(items, elapsed);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        [NotNull]
        private IEnumerable<Item> EvaluateRealXPath([NotNull] Item item, [NotNull] string expression)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(expression, nameof(expression));

            var result = new List<Item>();

            var nav = Factory.CreateItemNavigator(item);
            var iterator = nav.Select(expression);

            var count = 0;

            while (iterator.MoveNext() && count < Settings.Query.MaxItems)
            {
                var n = iterator.Current;
                if (n == null)
                {
                    continue;
                }

                var id = n.GetAttribute("id", string.Empty);

                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var i = item.Database.GetItem(id);
                if (i == null)
                {
                    continue;
                }

                result.Add(i);
                count++;
            }

            return result;
        }

        [CanBeNull]
        private IEnumerable<Item> EvaluateSitecoreXPath([NotNull] Item contextNode, [NotNull] string xpath)
        {
            Debug.ArgumentNotNull(contextNode, nameof(contextNode));
            Debug.ArgumentNotNull(xpath, nameof(xpath));

            if (xpath.StartsWith("fast:", StringComparison.OrdinalIgnoreCase))
            {
                return contextNode.Database.SelectItems(xpath);
            }

            return Data.Query.Query.SelectItems(xpath, contextNode);
        }

        [NotNull]
        private string FormatItems([NotNull] IEnumerable<Item> items, double elapsed)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            var list = items.ToList();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("items");
            output.WriteAttributeString("elapsed", elapsed.ToString("#,##0.00"));
            output.WriteAttributeString("count", list.Count().ToString());

            foreach (var item in list)
            {
                output.WriteStartElement("item");

                output.WriteAttributeString("id", item.ID.ToString());
                output.WriteAttributeString("name", item.Name);
                output.WriteAttributeString("path", item.Paths.Path);
                output.WriteAttributeString("icon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
