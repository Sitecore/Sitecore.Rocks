// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Items.Fields
{
    public class FieldWriterFactory
    {
        private readonly KeyValuePair<string, Type>[] fieldTypes;

        private readonly XmlWriter writer;

        public FieldWriterFactory([NotNull] XmlWriter writer)
        {
            Assert.ArgumentNotNull(writer, nameof(writer));

            this.writer = writer;

            fieldTypes = new[]
            {
                new KeyValuePair<string, Type>("Rules", typeof(RuleSetFieldWriter)),
                new KeyValuePair<string, Type>("layout", typeof(LayoutFieldWriter)),
                new KeyValuePair<string, Type>("security", typeof(SecurityFieldWriter)),
                new KeyValuePair<string, Type>("general link", typeof(GeneralLinkFieldWriter)),
                new KeyValuePair<string, Type>("link", typeof(GeneralLinkFieldWriter)),
                new KeyValuePair<string, Type>("droptree", typeof(DropTreeFieldWriter)),
                new KeyValuePair<string, Type>("reference", typeof(DropTreeFieldWriter))
            };
        }

        [CanBeNull]
        public FieldWriterBase GetFieldWriter([NotNull] string type)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            return (from KeyValuePair<string, Type> fieldType in fieldTypes
                where string.Compare(fieldType.Key, type, StringComparison.InvariantCultureIgnoreCase) == 0
                select Activator.CreateInstance(fieldType.Value, writer)).OfType<FieldWriterBase>().FirstOrDefault();
        }
    }
}
