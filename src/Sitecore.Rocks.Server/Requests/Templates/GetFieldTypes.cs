// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class GetFieldTypes
    {
        [NotNull]
        public string Execute(string databaseName)
        {
            var database = Factory.GetDatabase(Constants.CoreDatabaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("templates");

            WriteFieldTypes(output, database);

            database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            WriteFieldValidations(output, database);

            output.WriteEndElement();

            return writer.ToString();
        }

        private static void WriteFieldTypes(XmlTextWriter output, Database database)
        {
            output.WriteStartElement("fieldtypes");

            var fieldTypes = database.GetItem("/sitecore/system/field types");

            foreach (Item category in fieldTypes.Children)
            {
                var section = category.Name;

                foreach (Item fieldType in category.Children)
                {
                    var path = fieldType.Paths.Path;

                    output.WriteStartElement("fieldtype");

                    output.WriteAttributeString("name", fieldType.Name);
                    output.WriteAttributeString("id", fieldType.ID.ToString());
                    output.WriteAttributeString("icon", Images.GetThemedImageSource(fieldType.Appearance.Icon, ImageDimension.id16x16));
                    output.WriteAttributeString("section", section);
                    output.WriteAttributeString("path", path);

                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();
        }

        private static void WriteFieldValidations(XmlTextWriter output, Database database)
        {
            output.WriteStartElement("validations");

            var validations = database.GetItem("/sitecore/system/Settings/Validation Rules/Field Rules");
            if (validations != null)
            {
                WriteFieldValidations(output, validations);
            }

            output.WriteEndElement();
        }

        private static void WriteFieldValidations(XmlTextWriter output, Item parent)
        {
            foreach (Item item in parent.Children)
            {
                if (item.TemplateID == TemplateIDs.Folder)
                {
                    WriteFieldValidations(output, item);
                }
                else
                {
                    var section = item.Parent.Name;

                    output.WriteStartElement("validation");

                    output.WriteAttributeString("name", item["Title"] + " - " + item["Description"]);
                    output.WriteAttributeString("id", item.ID.ToString());
                    output.WriteAttributeString("section", section);

                    output.WriteEndElement();
                }
            }
        }
    }
}
