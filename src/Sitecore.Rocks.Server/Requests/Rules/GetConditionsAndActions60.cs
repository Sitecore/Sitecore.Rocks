// © 2015-2017 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Requests.Rules
{
    public class GetConditionsAndActions60
    {
        private static readonly ID ActionId = new ID("{F90052A5-B4E6-4E6D-9812-1E1B88A6FCEA}");

        private static readonly ID ConditionId = new ID("{F0D16EEE-3A05-4E43-A082-795A32B873C0}");

        private static readonly ID RulesContextFolder = new ID("{DDA66314-03F3-4C89-84A9-39DFFB235B06}");

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

        public virtual void WriteRules([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string dataSource)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(dataSource, nameof(dataSource));

            output.WriteStartElement("rules");

            WriteConditions(output, database, dataSource);
            WriteActions(output, database, dataSource);

            output.WriteEndElement();
        }

        private void WriteActions([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string dataSource)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(dataSource, nameof(dataSource));

            output.WriteStartElement("actions");

            if (!string.IsNullOrEmpty(dataSource))
            {
                var dataSourceItem = database.GetItem(dataSource);
                if (dataSourceItem != null)
                {
                    var local = dataSourceItem.Children["Actions"];
                    if (local != null)
                    {
                        WriteActions(output, local);
                    }
                }
            }

            var item = database.GetItem("/sitecore/system/Settings/Rules/Common/Actions");
            if (item != null)
            {
                WriteActions(output, item);
            }

            output.WriteEndElement();
        }

        private void WriteActions([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID == ActionId)
            {
                WriteAction(output, item, item.Parent.DisplayName);
            }

            foreach (Item child in item.Children)
            {
                WriteActions(output, child);
            }
        }

        private void WriteConditions([NotNull] XmlTextWriter output, [NotNull] Database database, [NotNull] string dataSource)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(dataSource, nameof(dataSource));

            output.WriteStartElement("conditions");

            if (!string.IsNullOrEmpty(dataSource))
            {
                var dataSourceItem = database.GetItem(dataSource);
                if (dataSourceItem != null)
                {
                    var local = dataSourceItem.Children["Conditions"];
                    if (local != null)
                    {
                        WriteConditions(output, local);
                    }
                }
            }

            var item = database.GetItem("/sitecore/system/Settings/Rules/Common/Conditions");
            if (item != null)
            {
                WriteConditions(output, item);
            }

            output.WriteEndElement();
        }

        private void WriteConditions([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            if (item.TemplateID == ConditionId)
            {
                WriteCondition(output, item, item.Parent.DisplayName);
            }

            foreach (Item child in item.Children)
            {
                WriteConditions(output, child);
            }
        }
    }
}
