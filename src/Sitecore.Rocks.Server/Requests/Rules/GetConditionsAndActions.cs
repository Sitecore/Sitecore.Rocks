// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.Rules.Taxonomy;
using Sitecore.Rules;
using Sitecore.Shell.Applications.Dialogs.RulesEditor;

namespace Sitecore.Rocks.Server.Requests.Rules
{
    public class GetConditionsAndActions
    {
        private static readonly ID RulesContextFolder = new ID("{DDA66314-03F3-4C89-84A9-39DFFB235B06}");

        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string dataSource)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                return string.Empty;
            }

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            Write(output, database, dataSource);

            return writer.ToString();
        }

        public void Write([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string dataSource)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(database, nameof(database));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            WriteRules(output, database, dataSource);
        }

        protected void WriteAction([NotNull] XmlTextWriter output, [NotNull] Item item, [NotNull] string category)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(category, nameof(category));

            output.WriteStartElement("action");

            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("category", category);
            output.WriteValue(item["Text"]);

            output.WriteEndElement();
        }

        protected void WriteCondition([NotNull] XmlTextWriter output, [NotNull] Item item, [NotNull] string category)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(category, nameof(category));

            output.WriteStartElement("condition");

            output.WriteAttributeString("id", item.ID.ToString());
            output.WriteAttributeString("category", category);
            output.WriteValue(item["Text"]);

            output.WriteEndElement();
        }

        protected virtual void WriteRules([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string dataSource)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(dataSource, nameof(dataSource));

            var options = new RulesEditorOptions
            {
                AllowMultiple = true,
                RulesPath = dataSource,
                IncludeCommon = true,
                HideActions = false,
            };

            var elements = GetGroupedElements(options);

            output.WriteStartElement("rules");

            WriteConditions(output, elements);
            WriteActions(output, elements);

            output.WriteEndElement();
        }

        [NotNull]
        private ICollection<Pair<string, IEnumerable<Item>>> GetGroupedElements([NotNull] RulesEditorOptions options)
        {
            Debug.ArgumentNotNull(options, nameof(options));
            Assert.ArgumentNotNull(options, nameof(options));

            var args = new RuleElementsPipelineArgs
            {
                RulesPath = options.RulesPath,
                ContextItemId = options.ContextItemID
            };

            GetRenderedRuleElements.Run(args);

            var elementsFolders = args.ElementFolders.Values;

            var groupedElements = elementsFolders.Select(e => new
            {
                GroupItem = Client.ContentDatabase.GetItem(e["Group"]),
                Folder = e
            }).Where(i => i.GroupItem != null).ToList();

            var notGroupedFolder = elementsFolders.Except(groupedElements.Select(i => i.Folder)).ToList();

            var elements = groupedElements.GroupBy(e => e.GroupItem, e => e.Folder.GetChildren(), (groupItem, children) => new Pair<string, IEnumerable<Item>>(groupItem.DisplayName, children.SelectMany(c => c)), new ItemIdComparer()).ToList();

            var notGroupedElements = notGroupedFolder.Select(f => new Pair<string, IEnumerable<Item>>(f.DisplayName, f.GetChildren())).ToList();

            elements = elements.Union(notGroupedElements).ToList();

            return elements.OrderBy(i => i.Part1).ToList();
        }

        private bool IsInheritedFrom([NotNull] TemplateItem template, [NotNull] ID templateId)
        {
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(templateId, nameof(templateId));

            return template.ID == templateId || template.BaseTemplates.Any(q => q.ID == templateId);
        }

        private void WriteActions([NotNull] XmlTextWriter output, [NotNull] ICollection<Pair<string, IEnumerable<Item>>> elements)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(elements, nameof(elements));

            output.WriteStartElement("actions");

            foreach (var pair in elements)
            {
                foreach (var item in pair.Part2)
                {
                    if (IsInheritedFrom(item.Template, RuleIds.Action))
                    {
                        WriteAction(output, item, pair.Part1);
                    }
                }
            }

            output.WriteEndElement();
        }

        private void WriteConditions([NotNull] XmlTextWriter output, [NotNull] ICollection<Pair<string, IEnumerable<Item>>> elements)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(elements, nameof(elements));

            output.WriteStartElement("conditions");

            foreach (var pair in elements)
            {
                foreach (var item in pair.Part2)
                {
                    if (IsInheritedFrom(item.Template, RuleIds.Condition))
                    {
                        WriteCondition(output, item, pair.Part1);
                    }
                }
            }

            output.WriteEndElement();
        }
    }
}
