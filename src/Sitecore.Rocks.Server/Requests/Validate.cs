// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Xml;

namespace Sitecore.Rocks.Server.Requests
{
    public class Validate
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string xml)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(xml, nameof(xml));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var items = new Hashtable();

            var targetItem = GetTargetItem(database, items, xml);
            if (targetItem == null)
            {
                return string.Empty;
            }

            var result = WriteValidators(targetItem);

            foreach (Item item in items.Values)
            {
                item.Editing.CancelEdit();
            }

            return result;
        }

        [CanBeNull]
        private Item GetTargetItem([NotNull] Database database, [NotNull] Hashtable items, [NotNull] string xml)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(xml, nameof(xml));

            var doc = XmlUtil.LoadXml(xml);
            Item targetItem = null;

            var fields = doc.SelectNodes("/sitecore/field");
            if (fields == null)
            {
                return null;
            }

            foreach (XmlNode node in fields)
            {
                var id = XmlUtil.GetAttribute("itemid", node);
                var language = XmlUtil.GetAttribute("language", node);
                var version = XmlUtil.GetAttribute("version", node);

                var fieldid = XmlUtil.GetAttribute("fieldid", node);
                var value = XmlUtil.GetChildValue("value", node);

                var key = id + language + version;

                var item = items[key] as Item;

                if (item == null)
                {
                    item = database.Items[id, Language.Parse(language), Data.Version.Parse(version)];

                    if (item == null)
                    {
                        continue;
                    }

                    items[key] = item;

                    item.Editing.BeginEdit();
                    targetItem = item;
                }

                var field = item.Fields[ID.Parse(fieldid)];

                if (field != null)
                {
                    field.Value = value;
                }
            }

            return targetItem;
        }

        [NotNull]
        private string WriteValidators([NotNull] Item targetItem)
        {
            Debug.ArgumentNotNull(targetItem, nameof(targetItem));

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("validators");

            var validatorCollection = ValidatorManager.BuildValidators(ValidatorsMode.ValidatorBar, targetItem);

            var options = new ValidatorOptions(true);

            ValidatorManager.Validate(validatorCollection, options);

            foreach (BaseValidator validator in validatorCollection)
            {
                if (validator.Result == ValidatorResult.Valid || validator.Result == ValidatorResult.Unknown)
                {
                    continue;
                }

                var fieldId = string.Empty;
                var validatorFieldId = validator.FieldID;
                if (!ItemUtil.IsNull(validatorFieldId))
                {
                    fieldId = validatorFieldId.ToString();
                }

                output.WriteStartElement("validator");

                output.WriteAttributeString("fieldid", fieldId);
                output.WriteAttributeString("validatorid", validator.ValidatorID.ToString());
                output.WriteAttributeString("result", ((int)validator.Result).ToString());
                output.WriteValue(validator.Text);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
