// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.IEnumerableExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Shell.Environment
{
    public partial class ServerHost
    {
        [NotNull]
        public ItemUri AddFromTemplate([NotNull] ItemUri parentItemUri, [NotNull] ItemUri templateUri, [NotNull] string name)
        {
            Assert.ArgumentNotNull(parentItemUri, nameof(parentItemUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(name, nameof(name));

            return parentItemUri.Site.DataService.AddFromTemplate(parentItemUri, templateUri, name);
        }

        [NotNull]
        public ItemUri CopyItem([NotNull] ItemUri itemUri, [NotNull] ItemId parentItemId, [NotNull] string newItemName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(parentItemId, nameof(parentItemId));
            Assert.ArgumentNotNull(newItemName, nameof(newItemName));

            return itemUri.Site.DataService.Copy(itemUri, parentItemId, newItemName);
        }

        [NotNull]
        public ItemUri CreateItem([NotNull] ItemUri parentItemUri, [NotNull] ItemId templateItemId, [NotNull] string newItemName)
        {
            Assert.ArgumentNotNull(parentItemUri, nameof(parentItemUri));
            Assert.ArgumentNotNull(templateItemId, nameof(templateItemId));
            Assert.ArgumentNotNull(newItemName, nameof(newItemName));

            var templateItemUri = new ItemUri(parentItemUri.DatabaseUri, templateItemId);

            return parentItemUri.Site.DataService.AddFromTemplate(parentItemUri, templateItemUri, newItemName);
        }

        public bool DeleteItem([NotNull] ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return itemUri.Site.DataService.Delete(itemUri);
        }

        public bool EnsureConnection([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            if (site.DataService.Status == DataServiceStatus.Connected)
            {
                return true;
            }

            return site.DataService.GetDatabases().Any();
        }

        public void GetItem([NotNull] ItemVersionUri itemVersionUri, [NotNull] GetValueCompleted<Item> completed)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            itemVersionUri.Site.DataService.GetItemFieldsAsync(itemVersionUri, completed);
        }

        public void GetItemHeader([NotNull] ItemUri itemUri, [NotNull] GetValueCompleted<ItemHeader> completed)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            itemUri.Site.DataService.GetItemHeader(itemUri, completed);
        }

        public void GetTemplates([NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<TemplateHeader> completed)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            databaseUri.Site.DataService.GetTemplatesAsync(databaseUri, completed);
        }

        public void GetTemplates([NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<TemplateHeader> completed, bool includeBranches)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                var root = response.ToXElement();
                if (root == null)
                {
                    completed(Enumerable.Empty<TemplateHeader>());
                    return;
                }

                var result = new List<TemplateHeader>();

                foreach (var child in root.Elements())
                {
                    var itemId = new ItemId(new Guid(child.GetAttributeValue("id")));
                    var itemUri = new ItemUri(databaseUri, itemId);

                    var parentPath = (Path.GetDirectoryName(child.GetAttributeValue("path")) ?? string.Empty).Replace("\\", "/");
                    var template = new TemplateHeader(itemUri, child.Value, child.GetAttributeValue("icon"), child.GetAttributeValue("path"), parentPath, child.Name == "branch");

                    result.Add(template);
                }

                completed(result);
            };

            databaseUri.Site.DataService.ExecuteAsync("Templates.GetTemplates", callback, databaseUri.DatabaseName.ToString(), includeBranches ? "true" : "false");
        }

        public void HandleCompleted([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Assert.ArgumentNotNull(response, nameof(response));
            Assert.ArgumentNotNull(executeResult, nameof(executeResult));

            DataService.HandleExecute(response, executeResult);
        }

        public bool MoveItem([NotNull] ItemUri itemUri, [NotNull] ItemId parentItemId)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(parentItemId, nameof(parentItemId));

            return itemUri.Site.DataService.Move(itemUri, parentItemId);
        }

        public void Publish([NotNull] DatabaseUri databaseUri, int mode)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            databaseUri.Site.DataService.Publish(mode, databaseUri);
        }

        public bool RenameItem([NotNull] ItemUri itemUri, [NotNull] string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return itemUri.Site.DataService.Rename(itemUri, newName);
        }

        public void SelectItems([NotNull] DatabaseUri databaseUri, [NotNull] string queryText, [NotNull] GetItemsCompleted<ItemHeader> completed)
        {
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            databaseUri.Site.DataService.SelectItems(queryText, databaseUri, completed);
        }

        public void UpdateItem([NotNull] ItemUri itemUri, [NotNull] FieldId fieldId, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldId, nameof(fieldId));
            Assert.ArgumentNotNull(value, nameof(value));

            ItemModifier.Edit(itemUri, fieldId, value);
        }

        public void UpdateItem([NotNull] ItemUri itemUri, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            ItemModifier.Edit(new ItemVersionUri(itemUri, Language.Current, Data.Version.Latest), fieldName, value);
        }

        public void UpdateItem([NotNull] ItemUri itemUri, [NotNull] List<Tuple<FieldId, string>> fieldValues)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(fieldValues, nameof(fieldValues));

            ItemModifier.Edit(itemUri, fieldValues);
        }

        public void UpdateItem([NotNull] ItemVersionUri itemVersionUri, [NotNull] string fieldName, [NotNull] string value)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(value, nameof(value));

            ItemModifier.Edit(itemVersionUri, fieldName, value);
        }

        public void UpdateItems([NotNull] List<Tuple<FieldUri, string>> fieldValues)
        {
            Assert.ArgumentNotNull(fieldValues, nameof(fieldValues));

            var fieldValue = fieldValues.FirstOrDefault();
            if (fieldValue == null)
            {
                return;
            }

            if (!fieldValues.AllHasValue(f => f.Item1.DatabaseUri))
            {
                throw new InvalidOperationException("All items must have the same site and database.");
            }

            var databaseUri = fieldValue.Item1.ItemVersionUri.DatabaseUri;

            ItemModifier.Edit(databaseUri, fieldValues);
        }
    }
}
