// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Shell.Applications.ContentEditor.Gutters;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests
{
    public class GetGutterItems
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string items, bool subitems)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(items, nameof(items));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var method = typeof(GutterRenderer).GetMethod("GetIconDescriptor", BindingFlags.Instance | BindingFlags.NonPublic);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("gutters");

            foreach (var id in items.Split('|'))
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                var item = database.GetItem(id);
                if (item == null)
                {
                    continue;
                }

                WiteGutters(output, item, method, subitems);
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        public void WiteGutters([NotNull] XmlTextWriter output, [NotNull] Item item, [NotNull] MethodInfo method, bool subitems)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(item, nameof(item));
            Assert.ArgumentNotNull(method, nameof(method));

            WriteGutter(output, item, method);

            if (!subitems)
            {
                return;
            }

            foreach (Item child in item.Children)
            {
                WiteGutters(output, child, method, true);
            }
        }

        private void WriteGutter([NotNull] XmlTextWriter output, [NotNull] Item item, [NotNull] MethodInfo method)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(method, nameof(method));

            var instance = new object[]
            {
                item
            };

            var list = new List<GutterIconDescriptor>();

            foreach (var gutter in GutterManager.GetRenderers())
            {
                GutterIconDescriptor descriptor;
                try
                {
                    descriptor = method.Invoke(gutter, instance) as GutterIconDescriptor;
                }
                catch
                {
                    descriptor = null;
                }

                if (descriptor == null)
                {
                    continue;
                }

                list.Add(descriptor);
            }

            if (list.Count == 0)
            {
                return;
            }

            output.WriteStartElement("item");

            output.WriteAttributeString("id", item.ID.ToString());

            foreach (var descriptor in list)
            {
                output.WriteStartElement("gutter");

                output.WriteAttributeString("icon", Images.GetThemedImageSource(descriptor.Icon, ImageDimension.id16x16));
                output.WriteAttributeString("tooltip", descriptor.Tooltip);

                output.WriteEndElement();
            }

            output.WriteEndElement();
        }
    }
}
