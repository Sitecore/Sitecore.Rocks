// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public abstract class GetTemplateBase : IComparer<TemplateSection>, IComparer<TemplateField>
    {
        public int Compare([CanBeNull] TemplateField x, [CanBeNull] TemplateField y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (x.Sortorder != y.Sortorder)
            {
                return x.Sortorder - y.Sortorder;
            }

            return x.Name.CompareTo(y.Name);
        }

        public int Compare([CanBeNull] TemplateSection x, [CanBeNull] TemplateSection y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            if (x.Sortorder != y.Sortorder)
            {
                return x.Sortorder - y.Sortorder;
            }

            return x.Name.CompareTo(y.Name);
        }

        protected virtual void WriteField([NotNull] XmlTextWriter output, Database database, [NotNull] TemplateField field, [NotNull] Template template, bool includeInheritedFields, bool newId)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(template, nameof(template));

            if (!includeInheritedFields && field.Template.ID != template.ID)
            {
                return;
            }

            var id = newId ? ID.NewID.ToString() : field.ID.ToString();

            output.WriteStartElement("field");

            output.WriteAttributeString("name", field.Name);
            output.WriteAttributeString("id", id);
            output.WriteAttributeString("type", field.Type);
            output.WriteAttributeString("shared", field.IsShared ? "1" : "0");
            output.WriteAttributeString("unversioned", field.IsUnversioned ? "1" : "0");
            output.WriteAttributeString("sortorder", field.Sortorder.ToString());

            output.WriteAttributeString("title", field.Name);
            output.WriteAttributeString("source", field.Source);
            output.WriteAttributeString("system", field.Template.BaseIDs.Length == 0 ? "1" : "0");

            var item = database.GetItem(field.ID);
            if (item != null)
            {
                output.WriteAttributeString("validatorbar", item["Validator Bar"]);
            }

            output.WriteEndElement();
        }

        protected virtual void WriteSection([NotNull] XmlTextWriter output, Database database, [NotNull] TemplateSection section, [NotNull] Template template, bool includeInheritedFields, bool newId)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(section, nameof(section));
            Debug.ArgumentNotNull(template, nameof(template));

            var id = newId ? ID.NewID.ToString() : section.ID.ToString();

            output.WriteStartElement("section");
            output.WriteAttributeString("name", section.Name);
            output.WriteAttributeString("id", id);
            output.WriteAttributeString("sortorder", section.Sortorder.ToString());

            var fields = new List<TemplateField>(section.GetFields());
            fields.Sort(this);

            foreach (var field in fields)
            {
                WriteField(output, database, field, template, includeInheritedFields, newId);
            }

            output.WriteEndElement();
        }
    }
}
