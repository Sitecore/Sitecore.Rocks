// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Toolbars;

namespace Sitecore.Rocks.ContentEditors.Commands.Fields.XmlFields
{
    [ToolbarElement(typeof(ContentEditorFieldContext), 9010, "XML", "Editing", Text = "Validate", ElementType = RibbonElementType.SmallButton), Command]
    public class ValidateXml : XmlOperation, IToolbarElement
    {
        public ValidateXml()
        {
            Text = "Validate Xml...";
            Group = "Xml";
            SortingValue = 2590;
        }

        protected override string ProcessXml(string xml)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));

            var value = "<d>" + xml + "</d>";

            try
            {
                var doc = XDocument.Parse(value);

                if (doc.Root != null)
                {
                    AppHost.MessageBox("All good.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return xml;
                }

                AppHost.MessageBox("Hmm... No root element.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppHost.MessageBox(string.Format("Nope, sorry, got an exception while parsing.\n\n{0}", ex.Message), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return xml;
        }
    }
}
