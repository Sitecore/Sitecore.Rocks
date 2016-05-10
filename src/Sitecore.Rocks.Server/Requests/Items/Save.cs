// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Rocks.Server.Pipelines.SetFieldValue;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Requests.Items
{
    public class Save
    {
        [NotNull]
        public string Execute([NotNull] string xml, [NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(xml, nameof(xml));
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            if (string.IsNullOrEmpty(xml))
            {
                return string.Empty;
            }

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var items = new Hashtable();

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var nodeList = doc.SelectNodes("/sitecore/field");
            if (nodeList == null)
            {
                return string.Empty;
            }

            foreach (XmlNode node in nodeList)
            {
                var id = XmlUtil.GetAttribute("itemid", node);
                var language = XmlUtil.GetAttribute("language", node);
                var version = XmlUtil.GetAttribute("version", node);
                var fieldid = XmlUtil.GetAttribute("fieldid", node);
                var value = XmlUtil.GetChildValue("value", node);
                var reset = XmlUtil.GetAttribute("reset", node) == "1";

                var key = id + language + version;

                var item = items[key] as Item;
                if (item == null)
                {
                    item = database.Items[id, Language.Parse(language), Data.Version.Parse(version)];
                    if (item == null)
                    {
                        continue;
                    }

                    items[key] = item;

                    item.Editing.BeginEdit();
                }

                var field = item.Fields[ID.Parse(fieldid)];

                if (field == null)
                {
                    continue;
                }

                if (reset)
                {
                    field.Value = string.Empty;
                    field.Reset();
                    continue;
                }

                SetFieldValuePipeline.Run().WithParameters(field, value);
            }

            foreach (Item item in items.Values)
            {
                item.Editing.EndEdit();
            }

            return string.Empty;
        }
    }
}
