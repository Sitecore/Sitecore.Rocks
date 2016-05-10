// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public static class ItemModifier
    {
        public static void Edit([NotNull] ItemUri itemUri, [NotNull] FieldId fieldId, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldId, nameof(fieldId));
            Assert.ArgumentNotNull(value, nameof(value));

            var fieldValues = new List<Tuple<FieldId, string>>
            {
                new Tuple<FieldId, string>(fieldId, value)
            };

            Edit(itemUri, fieldValues);
        }

        public static void Edit([NotNull] ItemUri itemUri, [NotNull] List<Tuple<FieldId, string>> fieldValues)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldValues, nameof(fieldValues));

            var fields = new List<Field>();

            foreach (var fieldValue in fieldValues)
            {
                var field = new Field
                {
                    Value = fieldValue.Item2,
                    HasValue = true
                };

                field.FieldUris.Add(new FieldUri(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest), fieldValue.Item1));

                fields.Add(field);
            }

            Edit(itemUri.DatabaseUri, fields, false);
        }

        public static void Edit([NotNull] DatabaseUri databaseUri, [NotNull] List<Tuple<FieldUri, string>> fieldValues)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(fieldValues, nameof(fieldValues));

            var fields = new List<Field>();

            foreach (var fieldValue in fieldValues)
            {
                var field = new Field
                {
                    Value = fieldValue.Item2,
                    HasValue = true
                };

                field.FieldUris.Add(fieldValue.Item1);

                fields.Add(field);
            }

            Edit(databaseUri, fields, false);
        }

        public static void Edit([NotNull] DatabaseUri databaseUri, [NotNull] List<Field> fields, bool modifiedOnly)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(fields, nameof(fields));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    AppHost.MessageBox("Failed to save changes.\n\n" + response, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (var field in fields)
                {
                    foreach (var fieldUri in field.FieldUris)
                    {
                        Notifications.RaiseFieldChanged(fieldUri, fieldUri, field.Value);
                    }
                }
            };

            Edit(databaseUri, fields, modifiedOnly, callback);
        }

        public static void Edit([NotNull] ItemVersionUri itemVersionUri, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var parts = response.Split(',');
                if (parts.Length < 2)
                {
                    return;
                }

                var version = int.Parse(parts[1]);

                Guid fieldGuid;
                if (!Guid.TryParse(parts[0], out fieldGuid))
                {
                    return;
                }

                var fieldUri = new FieldUri(new ItemVersionUri(itemVersionUri.ItemUri, itemVersionUri.Language, new Version(version)), new FieldId(fieldGuid));

                Notifications.RaiseFieldChanged(fieldUri, fieldUri, value);
            };

            itemVersionUri.DatabaseUri.Site.DataService.ExecuteAsync("Items.SetFieldValue", callback, itemVersionUri.DatabaseUri.DatabaseName.ToString(), itemVersionUri.ItemId.ToString(), itemVersionUri.Language.ToString(), itemVersionUri.Version.ToString(), fieldName, value);
        }

        public static void Edit([NotNull] ItemUri itemUri, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var parts = response.Split(',');

                Guid fieldGuid;
                if (!Guid.TryParse(parts[0], out fieldGuid))
                {
                    return;
                }

                var fieldUri = new FieldUri(new ItemVersionUri(itemUri, LanguageManager.CurrentLanguage, Version.Latest), new FieldId(fieldGuid));

                Notifications.RaiseFieldChanged(fieldUri, fieldUri, value);
            };

            itemUri.DatabaseUri.Site.DataService.ExecuteAsync("Items.SetFieldValue", callback, itemUri.DatabaseUri.DatabaseName.ToString(), itemUri.ItemId.ToString(), LanguageManager.CurrentLanguage.ToString(), Version.Latest.ToString(), fieldName, value);
        }

        internal static void Edit([NotNull] DatabaseUri databaseUri, [NotNull] List<Field> fields, bool modifiedOnly, [NotNull] ExecuteCompleted callback)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Debug.ArgumentNotNull(fields, nameof(fields));
            Debug.ArgumentNotNull(callback, nameof(callback));

            if ((databaseUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute)
            {
                var xml = GetXml(fields, modifiedOnly);

                databaseUri.Site.DataService.ExecuteAsync("Items.Save", callback, xml, databaseUri.DatabaseName.Name);
            }
            else
            {
                databaseUri.Site.DataService.Save(databaseUri.DatabaseName, fields);

                callback(string.Empty, new ExecuteResult(databaseUri.Site.DataService, null, false));
            }
        }

        [NotNull]
        private static string GetXml([NotNull] List<Field> fields, bool modifiedOnly)
        {
            Debug.ArgumentNotNull(fields, nameof(fields));

            var stringWriter = new StringWriter();
            var output = new XmlTextWriter(stringWriter);

            output.WriteStartElement(@"sitecore");

            foreach (var field in fields)
            {
                foreach (var fieldUri in field.FieldUris)
                {
                    if (!field.HasValue)
                    {
                        continue;
                    }

                    var value = field.Value;

                    if (modifiedOnly && value == field.OriginalValue && !field.ResetOnSave)
                    {
                        continue;
                    }

                    output.WriteStartElement(@"field");
                    output.WriteAttributeString(@"itemid", fieldUri.ItemId.ToString());
                    output.WriteAttributeString(@"language", fieldUri.Language.ToString());
                    output.WriteAttributeString(@"version", fieldUri.Version.ToString());
                    output.WriteAttributeString(@"fieldid", fieldUri.FieldId.ToString());
                    output.WriteAttributeString(@"templatefieldid", field.TemplateFieldId.ToString());
                    output.WriteAttributeString(@"reset", field.ResetOnSave ? @"1" : @"0");

                    output.WriteStartElement(@"value");
                    output.WriteValue(field.Value);
                    output.WriteEndElement();

                    output.WriteEndElement();
                }
            }

            output.WriteEndElement();

            return stringWriter.ToString();
        }
    }
}
