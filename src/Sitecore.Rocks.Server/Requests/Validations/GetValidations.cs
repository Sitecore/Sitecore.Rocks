// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Validations;

namespace Sitecore.Rocks.Server.Requests.Validations
{
    public class GetValidations
    {
        [NotNull]
        public string Execute([NotNull] string contextName)
        {
            Assert.ArgumentNotNull(contextName, nameof(contextName));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validations");

            foreach (var definition in ValidationManager.Validations)
            {
                var instance = definition.GetInstance();
                if (instance == null || !instance.CanCheck(contextName))
                {
                    continue;
                }

                output.WriteStartElement("validation");

                output.WriteAttributeString("name", definition.Attribute.Name);
                output.WriteAttributeString("category", definition.Attribute.Category);

                output.WriteEndElement();
            }

            foreach (var definition in ValidationManager.ItemValidations)
            {
                output.WriteStartElement("validation");

                output.WriteAttributeString("name", definition.Attribute.Name);
                output.WriteAttributeString("category", definition.Attribute.Category);

                output.WriteEndElement();
            }

            foreach (var definition in ValidationManager.RenderingValidations)
            {
                output.WriteStartElement("validation");

                output.WriteAttributeString("name", definition.Attribute.Name);
                output.WriteAttributeString("category", definition.Attribute.Category);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
