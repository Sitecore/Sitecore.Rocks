// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Validators;
using Sitecore.Diagnostics;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Sitecore.Rocks.Server.Requests.Validation
{
    public class GetValidationIssues
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string id, [NotNull] string rootId)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(id, nameof(id));
            Assert.ArgumentNotNull(rootId, nameof(rootId));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var item = database.GetItem(id);
            if (item == null)
            {
                throw new Exception("Item not found");
            }

            var root = new ID(rootId);

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("issues");

            var count = 0;

            while (item != null)
            {
                if (Validate(output, item))
                {
                    count++;
                }

                item = Next(item, root);

                if (count > 100)
                {
                    output.WriteElementString("next", item == null ? string.Empty : item.ID.ToString());
                    break;
                }
            }

            output.WriteEndElement();

            return writer.ToString();
        }

        [CanBeNull]
        private Item Next([NotNull] Item item, [NotNull] ID rootId)
        {
            Debug.ArgumentNotNull(item, nameof(item));
            Debug.ArgumentNotNull(rootId, nameof(rootId));

            if (item.HasChildren)
            {
                return item.Children[0];
            }

            if (item.ID == rootId)
            {
                return null;
            }

            var nextSibling = item.Axes.GetNextSibling();
            if (nextSibling != null)
            {
                return nextSibling;
            }

            var parent = item;
            while (true)
            {
                parent = parent.Parent;
                if (parent == null)
                {
                    break;
                }

                if (parent.ID == rootId)
                {
                    return null;
                }

                var next = parent.Axes.GetNextSibling();
                if (next != null)
                {
                    return next;
                }
            }

            return null;
        }

        private bool Validate([NotNull] XmlTextWriter output, [NotNull] Item item)
        {
            Debug.ArgumentNotNull(output, nameof(output));
            Debug.ArgumentNotNull(item, nameof(item));

            var result = false;

            var validatorCollection = ValidatorManager.BuildValidators(ValidatorsMode.ValidatorBar, item);

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
                if ((object)validatorFieldId != null && !validatorFieldId.IsNull)
                {
                    fieldId = validatorFieldId.ToString();
                }

                var validatorIcon = "Applications/16x16/preferences.png";
                var category = "Validation";
                var categoryIcon = "Applications/16x16/preferences.png";

                var validatorItem = item.Database.GetItem(validator.ValidatorID);
                if (validatorItem != null)
                {
                    validatorIcon = validatorItem.Appearance.Icon;
                    category = validatorItem.Parent.Name;
                }

                output.WriteStartElement("issue");

                output.WriteAttributeString("validatorid", validator.ValidatorID.ToString());
                output.WriteAttributeString("validatorname", validator.Name);
                output.WriteAttributeString("validatoricon", Images.GetThemedImageSource(validatorIcon, ImageDimension.id16x16));

                output.WriteAttributeString("itemid", item.ID.ToString());
                output.WriteAttributeString("itemname", item.Name);
                output.WriteAttributeString("itemicon", Images.GetThemedImageSource(item.Appearance.Icon, ImageDimension.id16x16));

                output.WriteAttributeString("categoryname", category);
                output.WriteAttributeString("categoryicon", Images.GetThemedImageSource(categoryIcon, ImageDimension.id16x16));

                output.WriteAttributeString("path", item.Paths.Path);
                output.WriteAttributeString("fieldid", fieldId);
                output.WriteAttributeString("fieldname", validator.GetFieldDisplayName());
                output.WriteAttributeString("icon", Images.GetThemedImageSource(validator.GetIcon(), ImageDimension.id16x16));
                output.WriteAttributeString("result", ((int)validator.Result).ToString());

                output.WriteValue(validator.Text);

                output.WriteEndElement();

                result = true;
            }

            return result;
        }
    }
}
