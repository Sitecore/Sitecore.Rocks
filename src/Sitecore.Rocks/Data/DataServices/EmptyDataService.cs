// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data.DataServices
{
    public class EmptyDataService : DataService
    {
        public override ItemUri AddFromMaster(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Assert.ArgumentNotNull(parentUri, nameof(parentUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return ItemUri.Empty;
        }

        public override ItemUri AddFromTemplate(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Assert.ArgumentNotNull(parentUri, nameof(parentUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return ItemUri.Empty;
        }

        public override void AddVersion(ItemVersionUri uri, GetValueCompleted<Version> callback)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            callback(Version.Empty);
        }

        public override bool CanExecuteAsync(string typeName)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));

            return false;
        }

        public override bool CheckDataService()
        {
            return false;
        }

        public override ItemUri Copy(ItemUri itemUri, ItemId newParentId, string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newParentId, nameof(newParentId));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return ItemUri.Empty;
        }

        public override bool Delete(ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            return false;
        }

        [CanBeNull]
        public override ItemUri Duplicate(ItemUri itemUri, string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return null;
        }

        public override void ExecuteAsync(string typeName, ExecuteCompleted executeCompleted, params object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(executeCompleted, nameof(executeCompleted));
            Assert.ArgumentNotNull(parameters, nameof(parameters));
        }

        public override void ExecuteAsync(string typeName, object state, ExecuteStateCompleted executeCompleted, params object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(executeCompleted, nameof(executeCompleted));
            Assert.ArgumentNotNull(parameters, nameof(parameters));
        }

        public override bool GetChildrenAsync(ItemUri itemUri, GetItemsCompleted<ItemHeader> getChildrenCallback)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(getChildrenCallback, nameof(getChildrenCallback));

            return false;
        }

        public override IEnumerable<DatabaseInfo> GetDatabases()
        {
            yield break;
        }

        public override Item GetItemFields(ItemVersionUri uri)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));

            return Item.Empty;
        }

        public override bool GetItemFieldsAsync(ItemVersionUri uri, GetValueCompleted<Item> getItemFieldsCallback)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));
            Assert.ArgumentNotNull(getItemFieldsCallback, nameof(getItemFieldsCallback));

            getItemFieldsCallback(Item.Empty);

            return true;
        }

        public override IEnumerable<ItemPath> GetItemPath(DatabaseUri databaseUri, string itemPath)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            return Enumerable.Empty<ItemPath>();
        }

        public override void GetItemXmlAsync(ItemUri itemUri, bool deep, GetValueCompleted<string> getItemXmlCallback)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(getItemXmlCallback, nameof(getItemXmlCallback));

            getItemXmlCallback(string.Empty);
        }

        public override IEnumerable<ItemHeader> GetRootItems(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            yield break;
        }

        public override Control GetSiteEditorControl()
        {
            return null;
        }

        public override void GetTemplatesAsync(DatabaseUri databaseUri, GetItemsCompleted<TemplateHeader> getTemplatesCallback)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(getTemplatesCallback, nameof(getTemplatesCallback));

            getTemplatesCallback(Enumerable.Empty<TemplateHeader>());
        }

        public override BaseTreeViewItem GetTreeViewItem(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            return null;
        }

        public override bool Move(ItemUri itemUri, ItemId newParentUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newParentUri, nameof(newParentUri));

            return false;
        }

        public override void PasteXml(ItemUri itemUri, string xml, bool changeIds)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(xml, nameof(xml));
        }

        public override Version RemoveVersion(ItemVersionUri uri)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));

            return Version.Empty;
        }

        public override bool Rename(ItemUri itemId, string newName)
        {
            Assert.ArgumentNotNull(itemId, nameof(itemId));
            Assert.ArgumentNotNull(newName, nameof(newName));

            return false;
        }

        public override void ResetDataService()
        {
        }

        public override bool Save(DatabaseName databaseName, List<Field> fields)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(fields, nameof(fields));

            return false;
        }

        public override bool TestConnection(string physicalPath)
        {
            Assert.ArgumentNotNull(physicalPath, nameof(physicalPath));

            AppHost.MessageBox(Resources.EmptyDataService_TestConnection_The_Empty_Data_Service_cannot_connect_to_anything_, Resources.Test_Connection, MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }
}
