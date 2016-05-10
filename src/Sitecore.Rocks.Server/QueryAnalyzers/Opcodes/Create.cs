// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Web.UI;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using Sitecore.Rocks.Server.Extensions.QueryExtensions;
using Sitecore.Rocks.Server.QueryAnalyzers.Keywords;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class Create : Opcode
    {
        public Create([NotNull] string templateName, [NotNull] Opcode location, [NotNull] List<CreateKeyword.CreateColumn> createColumns)
        {
            Assert.ArgumentNotNull(templateName, nameof(templateName));
            Assert.ArgumentNotNull(location, nameof(location));
            Assert.ArgumentNotNull(createColumns, nameof(createColumns));

            TemplateName = templateName;
            Location = location;
            CreateColumns = createColumns;
        }

        protected List<CreateKeyword.CreateColumn> CreateColumns { get; set; }

        protected Opcode Location { get; set; }

        protected string TemplateName { get; set; }

        [NotNull]
        public override object Evaluate([NotNull] Query query, [NotNull] QueryContext contextNode)
        {
            Assert.ArgumentNotNull(query, nameof(query));
            Assert.ArgumentNotNull(contextNode, nameof(contextNode));

            var parent = query.GetItem(contextNode, Location);
            if (parent == null)
            {
                throw new QueryException("The 'In' clause does not evaluate to a single item");
            }

            var itemsAffected = 1;

            var templateItem = parent.Add(TemplateName, new TemplateID(TemplateIDs.Template));

            var sections = new Dictionary<string, Item>();

            foreach (var createColumn in CreateColumns)
            {
                var sectionName = createColumn.Section;
                if (string.IsNullOrEmpty(sectionName))
                {
                    sectionName = "Data";
                }

                Item sectionItem;
                if (!sections.TryGetValue(sectionName, out sectionItem))
                {
                    sectionItem = templateItem.Add(sectionName, new TemplateID(TemplateIDs.TemplateSection));

                    sections[sectionName] = sectionItem;

                    itemsAffected++;
                }

                var fieldItem = sectionItem.Add(createColumn.FieldName, new TemplateID(TemplateIDs.TemplateField));

                fieldItem.Editing.BeginEdit();

                fieldItem["Type"] = createColumn.FieldType;
                fieldItem["Shared"] = createColumn.Shared ? "1" : string.Empty;
                fieldItem["Unversioned"] = createColumn.Unversioned ? "1" : string.Empty;

                fieldItem.Editing.EndEdit();
                itemsAffected++;
            }

            if (itemsAffected == 1)
            {
                return "(1 item affected)";
            }

            return string.Format("({0} item affected)", itemsAffected);
        }

        public override void Print([NotNull] HtmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            PrintIndent(output);
            PrintLine(output, GetType().Name);
        }
    }
}
