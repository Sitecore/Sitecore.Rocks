// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.XmlFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 9000, "XML", "Editing", ElementType = RibbonElementType.SmallButton), Command]
    public class FormatXml : XmlOperation, IToolbarElement
    {
        public FormatXml()
        {
            Text = "Format Xml";
            Group = "Xml";
            SortingValue = 2595;
        }

        protected override string ProcessXml(string xml)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));

            var value = "<d.d>" + xml + "</d.d>";

            if (Parse(value) == null)
            {
                return xml;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 2
            };

            var doc = new XmlDocument
            {
                PreserveWhitespace = false
            };

            doc.LoadXml(value);

            var reader = new XmlNodeReader(doc);

            output.WriteNode(reader, true);

            output.Flush();

            value = writer.ToString().Replace("<d.d>", string.Empty).Replace("</d.d>", string.Empty).Trim();
            value = value.Replace("\n  ", "\n");

            return value;
        }
    }
}
