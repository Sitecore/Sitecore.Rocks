// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Requests.Templates;

namespace Sitecore.Rocks.Server.Requests.CodeGeneration
{
    public class GetTemplatesWithFields : GetTemplateBase
    {
        private List<List<string>> scope;

        [NotNull]
        public string Execute([NotNull] string databaseName)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("templates");

            scope = new List<List<string>>
            {
                new List<string>()
            };

            var templates = TemplateManager.GetTemplates(database);

            foreach (var template in templates.Values)
            {
                output.WriteStartElement("template");

                output.WriteAttributeString("id", template.ID.ToString());
                output.WriteAttributeString("name", template.Name);
                output.WriteAttributeString("safename", GetName(scope, template.Name));

                scope.Add(new List<string>());

                var sections = new List<TemplateSection>(template.GetSections());
                sections.Sort(this);
                foreach (var section in sections)
                {
                    WriteSection(output, database, section, template, true, false);
                }

                scope.RemoveAt(scope.Count - 1);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        protected override void WriteField(XmlTextWriter output, [NotNull] Database database, TemplateField field, Template template, bool includeInheritedFields, bool newId)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(field, nameof(field));
            Debug.ArgumentNotNull(template, nameof(template));

            var id = newId ? ID.NewID.ToString() : field.ID.ToString();

            output.WriteStartElement("field");

            output.WriteAttributeString("name", field.Name);
            output.WriteAttributeString("safename", GetName(scope, field.Name));
            output.WriteAttributeString("id", id);
            output.WriteAttributeString("type", field.Type);
            output.WriteAttributeString("inherited", field.Template.ID != template.ID ? "true" : "false");

            output.WriteEndElement();
        }

        protected override void WriteSection(XmlTextWriter output, [NotNull] Database database, TemplateSection section, Template template, bool includeInheritedFields, bool newId)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(section, nameof(section));
            Debug.ArgumentNotNull(template, nameof(template));

            var fields = new List<TemplateField>(section.GetFields());
            fields.Sort(this);

            foreach (var field in fields)
            {
                WriteField(output, database, field, template, includeInheritedFields, newId);
            }
        }

        [NotNull]
        private static string GetName([NotNull] List<List<string>> scope, [NotNull] string name)
        {
            Debug.ArgumentNotNull(scope, nameof(scope));
            Debug.ArgumentNotNull(name, nameof(name));

            var regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

            var safeName = regex.Replace(name, string.Empty);
            if (!char.IsLetter(safeName, 0) || !CodeDomProvider.CreateProvider("C#").IsValidIdentifier(safeName))
            {
                safeName = string.Concat("_", safeName);
            }

            var result = safeName;
            var index = 1;
            while (HasName(scope, result))
            {
                index++;
                result = safeName + index;
            }

            scope[scope.Count - 1].Add(result);

            return result;
        }

        private static bool HasName([NotNull] List<List<string>> scope, [NotNull] string result)
        {
            Debug.ArgumentNotNull(scope, nameof(scope));
            Debug.ArgumentNotNull(result, nameof(result));

            for (var index = scope.Count - 1; index >= 0; index--)
            {
                var list = scope[index];
                if (list.Contains(result))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
