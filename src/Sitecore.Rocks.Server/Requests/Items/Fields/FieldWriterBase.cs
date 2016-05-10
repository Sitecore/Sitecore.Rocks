// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public abstract class FieldWriterBase
    {
        protected FieldWriterBase([NotNull] XmlTextWriter writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));

            Writer = writer;
        }

        protected XmlTextWriter Writer { get; set; }

        public abstract void WriteField(Item item, Field field);
    }
}
