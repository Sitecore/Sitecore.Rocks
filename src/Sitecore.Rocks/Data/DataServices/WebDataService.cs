// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data.DataServices.Dialogs;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XmlNodeExtensions;
using Sitecore.Rocks.Net.Pipelines.Troubleshooter;
using Sitecore.Rocks.NewWebService;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.Data.DataServices
{
    public abstract class WebDataService : DataService, IComparer<TemplateHeader>
    {
        public override ItemUri AddFromMaster(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Assert.ArgumentNotNull(parentUri, nameof(parentUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            Func<ItemUri> execute = delegate
            {
                var element = ExecuteAddFromMaster(parentUri, templateUri, newName);
                if (element == null)
                {
                    return ItemUri.Empty;
                }

                if (!ProcessStatus(element))
                {
                    return ItemUri.Empty;
                }

                var data = element.SelectSingleNode(@"/data/data");
                if (data == null)
                {
                    return ItemUri.Empty;
                }

                Guid newId;
                if (!Guid.TryParse(data.InnerText, out newId))
                {
                    return ItemUri.Empty;
                }

                return new ItemUri(templateUri.DatabaseUri, new ItemId(newId));
            };

            AppHost.Usage.ReportRequest("AddFromMaster");
            return Execute(execute, ItemUri.Empty);
        }

        public override ItemUri AddFromTemplate(ItemUri parentUri, ItemUri templateUri, string newName)
        {
            Assert.ArgumentNotNull(parentUri, nameof(parentUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            Func<ItemUri> execute = delegate
            {
                var element = ExecuteAddFromTemplate(parentUri, templateUri, newName);
                if (element == null)
                {
                    return ItemUri.Empty;
                }

                if (!ProcessStatus(element))
                {
                    return ItemUri.Empty;
                }

                var data = element.SelectSingleNode(@"/data/data");
                if (data == null)
                {
                    return ItemUri.Empty;
                }

                Guid newId;
                if (!Guid.TryParse(data.InnerText, out newId))
                {
                    return ItemUri.Empty;
                }

                return new ItemUri(templateUri.DatabaseUri, new ItemId(newId));
            };

            AppHost.Usage.ReportRequest("AddFromTemplate");
            return Execute(execute, ItemUri.Empty);
        }

        public override void AddVersion(ItemVersionUri uri, GetValueCompleted<Version> callback)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            EventHandler<AddVersionCompletedEventArgs> c = delegate(object sender, AddVersionCompletedEventArgs args)
            {
                if (!HandleAsyncOperation(args, () => args.Result))
                {
                    callback(Version.Empty);
                    return;
                }

                var result = args.Result;

                var data = result.SelectSingleNode(@"/data/data");
                if (data == null)
                {
                    callback(Version.Empty);
                    return;
                }

                int version;
                if (!int.TryParse(data.InnerText, out version))
                {
                    callback(Version.Empty);
                    return;
                }

                callback(new Version(version));
            };

            Execute(() => ExecuteAddVersionAsync(uri, c));
        }

        public override bool CheckDataService()
        {
            if (!CheckCredentials())
            {
                return false;
            }

            return CheckVersion();
        }

        public int Compare([NotNull] TemplateHeader x, [NotNull] TemplateHeader y)
        {
            Assert.ArgumentNotNull(x, nameof(x));
            Assert.ArgumentNotNull(y, nameof(y));

            var result = string.Compare(x.Section, y.Section, StringComparison.InvariantCultureIgnoreCase);

            if (result == 0)
            {
                result = string.Compare(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
            }

            return result;
        }

        public override ItemUri Copy(ItemUri itemUri, ItemId newParentId, string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newParentId, nameof(newParentId));
            Assert.ArgumentNotNull(newName, nameof(newName));

            Func<ItemUri> execute = delegate
            {
                var element = ExecuteCopyTo(itemUri, newParentId, newName);
                if (element == null)
                {
                    return ItemUri.Empty;
                }

                if (!ProcessStatus(element))
                {
                    return ItemUri.Empty;
                }

                var firstChild = element.FirstChild;
                if (firstChild == null)
                {
                    return ItemUri.Empty;
                }

                var value = firstChild.InnerText;
                if (string.IsNullOrEmpty(value))
                {
                    return ItemUri.Empty;
                }

                var guid = Guid.Parse(value);

                return new ItemUri(itemUri.DatabaseUri, new ItemId(guid));
            };

            AppHost.Usage.ReportRequest("Copy");
            return Execute(execute, ItemUri.Empty);
        }

        public override bool Delete(ItemUri itemUri)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));

            Func<bool> execute = delegate
            {
                var element = ExecuteDelete(itemUri);
                if (element == null)
                {
                    return false;
                }

                return ProcessStatus(element);
            };

            AppHost.Usage.ReportRequest("Delete");
            return Execute(execute, false);
        }

        public override ItemUri Duplicate(ItemUri itemUri, string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            Func<ItemUri> execute = delegate
            {
                var result = ExecuteDuplicate(itemUri, newName);
                if (result == null)
                {
                    return ItemUri.Empty;
                }

                if (!ProcessStatus(result))
                {
                    return ItemUri.Empty;
                }

                var firstChild = result.FirstChild;
                if (firstChild != null)
                {
                    var value = firstChild.InnerText;
                    if (!string.IsNullOrEmpty(value))
                    {
                        Guid guid;
                        try
                        {
                            guid = Guid.Parse(value);
                        }
                        catch
                        {
                            return ItemUri.Empty;
                        }

                        return new ItemUri(itemUri.DatabaseUri, new ItemId(guid));
                    }
                }

                return ItemUri.Empty;
            };

            AppHost.Usage.ReportRequest("Delete");
            return Execute(execute, ItemUri.Empty);
        }

        public override void ExecuteAsync(string typeName, ExecuteCompleted executeCompleted, params object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(executeCompleted, nameof(executeCompleted));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            if (!typeName.StartsWith(Constants.SitecoreRocksServer + @".Requests."))
            {
                typeName = Constants.SitecoreRocksServer + @".Requests." + typeName;
            }

            EventHandler<ExecuteCompletedEventArgs> callback = (sender, args) =>
            {
                Action action = delegate
                {
                    EndCommunication();

                    var executeResult = new ExecuteResult(this, args.Error, args.Cancelled);

                    if (args.Error != null)
                    {
                        executeCompleted(string.Empty, executeResult);
                    }
                    else
                    {
                        executeCompleted(args.Result.Trim(), executeResult);
                    }
                };

                Dispatcher.CurrentDispatcher.Invoke(new Action(action));
            };

            Action fail = delegate
            {
                var executeResult = new ExecuteResult(this, new Exception(@"No Service"), false);
                executeCompleted(string.Empty, executeResult);
            };

            AppHost.Usage.ReportRequest(typeName);
            Execute(() => ExecuteExecuteAsync(typeName, parameters, callback), fail);
        }

        public override void ExecuteAsync(string typeName, object state, ExecuteStateCompleted executeCompleted, params object[] parameters)
        {
            Assert.ArgumentNotNull(typeName, nameof(typeName));
            Assert.ArgumentNotNull(executeCompleted, nameof(executeCompleted));
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            if (!typeName.StartsWith(Constants.SitecoreRocksServer + @".Requests."))
            {
                typeName = Constants.SitecoreRocksServer + ".Requests." + typeName;
            }

            EventHandler<ExecuteCompletedEventArgs> callback = (sender, args) =>
            {
                Action action = delegate
                {
                    EndCommunication();

                    var executeResult = new ExecuteResult(this, args.Error, args.Cancelled);

                    if (args.Error != null)
                    {
                        executeCompleted(string.Empty, executeResult, state);
                    }
                    else
                    {
                        executeCompleted(args.Result.Trim(), executeResult, state);
                    }
                };

                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(action));
            };

            Action fail = delegate
            {
                var executeResult = new ExecuteResult(this, new Exception(@"No Service"), false);
                executeCompleted(string.Empty, executeResult, state);
            };

            AppHost.Usage.ReportRequest(typeName);
            Execute(() => ExecuteExecuteAsync(typeName, parameters, callback), fail);
        }

        public override bool GetChildrenAsync(ItemUri itemUri, GetItemsCompleted<ItemHeader> getChildrenCallback)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(getChildrenCallback, nameof(getChildrenCallback));

            EventHandler<GetChildrenCompletedEventArgs> callback = delegate(object sender, GetChildrenCompletedEventArgs args)
            {
                try
                {
                    // ReSharper disable UnusedVariable
                    var element = args.Result;

                    // ReSharper restore UnusedVariable
                }
                catch
                {
                    // SitecoreApplication.HandleException(ex);
                    AppHost.MessageBox(Resources.WebDataService_GetChildrenAsync_The_item_was_not_found__It_may_have_been_deleted_, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Information);
                    getChildrenCallback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                if (!HandleAsyncOperation(args, () => args.Result))
                {
                    getChildrenCallback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                try
                {
                    var result = args.Result;

                    var items = new List<ItemHeader>();

                    foreach (XmlNode child in result)
                    {
                        items.Add(GetItemHeader(child, itemUri.DatabaseUri));
                    }

                    getChildrenCallback(items);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                    getChildrenCallback(Enumerable.Empty<ItemHeader>());
                }
            };

            AppHost.Usage.ReportRequest("GetChildren");
            return Execute(() => ExecuteGetChildrenAsync(itemUri, callback), false);
        }

        public override IEnumerable<DatabaseInfo> GetDatabases()
        {
            Func<IEnumerable<DatabaseInfo>> execute = delegate
            {
                var element = ExecuteGetDatabases();
                if (element == null)
                {
                    return Enumerable.Empty<DatabaseInfo>();
                }

                if (!ProcessStatus(element))
                {
                    return Enumerable.Empty<DatabaseInfo>();
                }

                var list = new List<DatabaseInfo>();

                foreach (XmlElement database in element)
                {
                    var databaseName = database.InnerText;
                    var connectionString = database.GetAttributeValue("connectionstring");

                    list.Add(new DatabaseInfo(new DatabaseName(databaseName)));
                }

                return list;
            };

            AppHost.Usage.ReportRequest("GetDatabases");
            return Execute(execute, Enumerable.Empty<DatabaseInfo>());
        }

        public override Item GetItemFields(ItemVersionUri itemVersionUri)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));

            Func<Item> execute = delegate
            {
                var element = ExecuteGetItemFields(itemVersionUri);
                if (element == null)
                {
                    return Item.Empty;
                }

                if (!ProcessStatus(element))
                {
                    return Item.Empty;
                }

                return GetItem(itemVersionUri, element);
            };

            AppHost.Usage.ReportRequest("GetItemFields");
            return Execute(execute, Item.Empty);
        }

        public override bool GetItemFieldsAsync(ItemVersionUri itemVersionUri, GetValueCompleted<Item> getItemFieldsCallback)
        {
            Assert.ArgumentNotNull(itemVersionUri, nameof(itemVersionUri));
            Assert.ArgumentNotNull(getItemFieldsCallback, nameof(getItemFieldsCallback));

            EventHandler<GetItemFieldsCompletedEventArgs> callback = delegate(object sender, GetItemFieldsCompletedEventArgs args)
            {
                if (!HandleAsyncOperation(args, () => args.Result))
                {
                    getItemFieldsCallback(Item.Empty);
                    return;
                }

                var item = GetItem(itemVersionUri, args.Result);

                getItemFieldsCallback(item);
            };

            AppHost.Usage.ReportRequest("GetItemFields");

            return Execute(() => ExecuteGetItemFieldsAsync(itemVersionUri, callback), false);
        }

        public override IEnumerable<ItemPath> GetItemPath(DatabaseUri databaseUri, string itemPath)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            Func<IEnumerable<ItemPath>> execute = delegate
            {
                var element = ExecuteGetItemPath(databaseUri.DatabaseName, itemPath);
                if (element == null)
                {
                    return Enumerable.Empty<ItemPath>();
                }

                if (!ProcessStatus(element))
                {
                    return Enumerable.Empty<ItemPath>();
                }

                var items = element.SelectNodes(@"/path/item");
                if (items == null)
                {
                    return Enumerable.Empty<ItemPath>();
                }

                var result = new List<ItemPath>();

                foreach (XmlNode item in items)
                {
                    var itemId = new Guid(item.GetAttributeValue("id"));

                    result.Add(new ItemPath
                    {
                        Name = item.GetAttributeValue("name"),
                        ItemUri = new ItemUri(databaseUri, new ItemId(itemId))
                    });
                }

                return result;
            };

            AppHost.Usage.ReportRequest("GetItemPath");
            return Execute(execute, Enumerable.Empty<ItemPath>());
        }

        public override void GetItemXmlAsync(ItemUri itemUri, bool deep, GetValueCompleted<string> getItemXmlCallback)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(getItemXmlCallback, nameof(getItemXmlCallback));

            EventHandler<GetXMLCompletedEventArgs> callback = delegate(object sender, GetXMLCompletedEventArgs args)
            {
                if (!HandleAsyncOperation(args, () => args.Result))
                {
                    getItemXmlCallback(string.Empty);
                    return;
                }

                var result = args.Result;
                if (result == null)
                {
                    return;
                }

                if (!ProcessStatus(result))
                {
                    return;
                }

                var node = result.SelectSingleNode(@"data/data");

                var innerXml = node == null ? string.Empty : node.InnerXml;

                getItemXmlCallback(innerXml);
            };

            AppHost.Usage.ReportRequest("GetItemXml");
            Execute(() => ExecuteGetXmlAsync(itemUri, deep, callback), () => getItemXmlCallback(string.Empty));
        }

        public override IEnumerable<ItemHeader> GetRootItems(DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            if (!CheckDataService())
            {
                yield break;
            }

            yield return new ItemHeader
            {
                HasChildren = true,
                ItemUri = new ItemUri(databaseUri, new ItemId(DatabaseTreeViewItem.RootItemGuid)),
                Name = @"sitecore",
                Path = "/sitecore",
                Icon = new Icon("Resources/16x16/copy.png"),
                TemplateId = new ItemId(new Guid(@"{C6576836-910C-4A3D-BA03-C277DBD3B827}")),
                TemplateName = "Root"
            };
        }

        [NotNull]
        public override Control GetSiteEditorControl()
        {
            return new WebServiceSiteEditor();
        }

        public override void GetTemplatesAsync(DatabaseUri databaseUri, GetItemsCompleted<TemplateHeader> getTemplatesCallback)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(getTemplatesCallback, nameof(getTemplatesCallback));

            EventHandler<GetTemplatesCompletedEventArgs> callback = delegate(object sender, GetTemplatesCompletedEventArgs args)
            {
                if (!HandleAsyncOperation(args, () => args.Result))
                {
                    getTemplatesCallback(Enumerable.Empty<TemplateHeader>());
                    return;
                }

                var result = args.Result;
                if (result == null)
                {
                    return;
                }

                if (!ProcessStatus(result))
                {
                    return;
                }

                var list = new List<TemplateHeader>();
                foreach (XmlNode child in result)
                {
                    list.Add(GetTemplateHeader(child, databaseUri));
                }

                list.Sort(this);

                getTemplatesCallback(list);
            };

            AppHost.Usage.ReportRequest("GetTemplates");
            Execute(() => ExecuteGetTemplatesAsync(databaseUri.DatabaseName, callback));
        }

        [NotNull]
        public override BaseTreeViewItem GetTreeViewItem(Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            return new SiteTreeViewItem(site);
        }

        public override bool Move(ItemUri itemUri, ItemId newParentId)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newParentId, nameof(newParentId));

            Func<bool> execute = delegate
            {
                var element = ExecuteMove(itemUri, newParentId);
                if (element == null)
                {
                    return false;
                }

                if (!ProcessStatus(element))
                {
                    return false;
                }

                return true;
            };

            AppHost.Usage.ReportRequest("Move");
            return Execute(execute, false);
        }

        public override void PasteXml(ItemUri itemUri, string xml, bool changeIds)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(xml, nameof(xml));

            Func<bool> execute = delegate
            {
                ExecutePasteXml(itemUri, xml, changeIds);
                return true;
            };

            AppHost.Usage.ReportRequest("PasteXml");
            Execute(execute, false);
        }

        public override Version RemoveVersion(ItemVersionUri uri)
        {
            Assert.ArgumentNotNull(uri, nameof(uri));

            Func<Version> execute = delegate
            {
                var element = ExecuteRemoveVersion(uri);
                if (element == null)
                {
                    return Version.Empty;
                }

                if (!ProcessStatus(element))
                {
                    return Version.Empty;
                }

                return Version.Empty;
            };

            AppHost.Usage.ReportRequest("RemoveVersion");
            return Execute(execute, Version.Empty);
        }

        public override bool Rename(ItemUri itemUri, string newName)
        {
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(newName, nameof(newName));

            Func<bool> execute = delegate
            {
                var element = ExecuteRename(itemUri, newName);
                if (element == null)
                {
                    return false;
                }

                if (!ProcessStatus(element))
                {
                    return false;
                }

                return true;
            };

            AppHost.Usage.ReportRequest("Rename");
            return Execute(execute, false);
        }

        public override bool Save(DatabaseName databaseName, List<Field> fields)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(fields, nameof(fields));

            var xml = GetXml(fields, true);

            Func<bool> execute = delegate
            {
                var element = ExecuteSave(xml, databaseName);
                if (element == null)
                {
                    return false;
                }

                return ProcessStatus(element);
            };

            AppHost.Usage.ReportRequest("Save");
            return Execute(execute, false);
        }

        protected abstract bool CheckVersion();

        protected T Execute<T>([NotNull] Func<T> execute, T defaultValue)
        {
            Debug.ArgumentNotNull(execute, nameof(execute));

            if (!CheckDataService())
            {
                return defaultValue;
            }

            try
            {
                BeginCommunication();
                try
                {
                    return execute();
                }
                finally
                {
                    EndCommunication();
                }
            }
            catch (Exception ex)
            {
                HandleSyncOperation(ex);
                return defaultValue;
            }
        }

        protected void Execute([NotNull] Action execute, [CanBeNull] Action fail = null)
        {
            Debug.ArgumentNotNull(execute, nameof(execute));

            if (!CheckDataService())
            {
                if (fail != null)
                {
                    fail();
                }

                return;
            }

            try
            {
                BeginCommunication();
                try
                {
                    execute();
                }
                finally
                {
                    EndCommunication();
                }
            }
            catch (Exception ex)
            {
                if (fail != null)
                {
                    fail();
                }

                HandleSyncOperation(ex);
            }
        }

        [CanBeNull]
        protected abstract XmlElement ExecuteAddFromMaster([NotNull] ItemUri parent, [NotNull] ItemUri templateUri, [NotNull] string newName);

        [CanBeNull]
        protected abstract XmlElement ExecuteAddFromTemplate([NotNull] ItemUri parent, [NotNull] ItemUri templateUri, [NotNull] string newName);

        protected abstract void ExecuteAddVersionAsync([NotNull] ItemVersionUri uri, [NotNull] EventHandler<AddVersionCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteCopyTo([NotNull] ItemUri itemUri, [NotNull] ItemId newParentId, [NotNull] string newName);

        protected abstract void ExecuteCopyToAsync([NotNull] ItemUri itemUri, [NotNull] ItemId newParentId, [NotNull] string newName, [NotNull] EventHandler<CopyToCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteDelete([NotNull] ItemUri itemUri);

        [CanBeNull]
        protected abstract XmlElement ExecuteDuplicate([NotNull] ItemUri itemUri, [NotNull] string newName);

        protected abstract void ExecuteExecuteAsync([NotNull] string typeName, [NotNull] object[] parameters, [NotNull] EventHandler<ExecuteCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteGetChildren([NotNull] ItemUri itemUri);

        protected abstract bool ExecuteGetChildrenAsync([NotNull] ItemUri itemUri, [NotNull] EventHandler<GetChildrenCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteGetDatabases();

        [CanBeNull]
        protected abstract XmlElement ExecuteGetItemFields([NotNull] ItemVersionUri uri);

        protected abstract bool ExecuteGetItemFieldsAsync([NotNull] ItemVersionUri itemVersionUri, [NotNull] EventHandler<GetItemFieldsCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteGetItemPath([NotNull] DatabaseName databaseName, [NotNull] string itemPath);

        protected abstract void ExecuteGetTemplatesAsync([NotNull] DatabaseName databaseName, [NotNull] EventHandler<GetTemplatesCompletedEventArgs> callback);

        protected abstract void ExecuteGetXmlAsync([NotNull] ItemUri itemUri, bool deep, [NotNull] EventHandler<GetXMLCompletedEventArgs> callback);

        [CanBeNull]
        protected abstract XmlElement ExecuteMove([NotNull] ItemUri itemUri, [NotNull] ItemId newParentId);

        protected abstract void ExecutePasteXml([NotNull] ItemUri itemUri, [NotNull] string xml, bool changeIds);

        [CanBeNull]
        protected abstract XmlElement ExecuteRemoveVersion([NotNull] ItemVersionUri uri);

        [CanBeNull]
        protected abstract XmlElement ExecuteRename([NotNull] ItemUri itemId, [NotNull] string newName);

        [CanBeNull]
        protected abstract XmlElement ExecuteSave([NotNull] string xml, [NotNull] DatabaseName databaseName);

        [NotNull]
        protected string GetHostName()
        {
            var result = Connection.HostName;
            if (result.IndexOf(@"://", StringComparison.Ordinal) < 0)
            {
                result = @"http://" + result;
            }

            return result;
        }

        [NotNull]
        protected virtual ItemHeader GetItemHeader([NotNull] XmlNode child, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(child, nameof(child));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return WebServiceMethods.GetItemHeader.Call(child, databaseUri);
        }

        [NotNull, Localizable(false)]
        protected abstract string GetServiceUrl();

        [NotNull]
        protected virtual TemplateHeader GetTemplateHeader([NotNull] XmlNode child, [NotNull] DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(child, nameof(child));
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var itemId = new ItemId(new Guid(child.GetAttributeValue("id")));
            var itemUri = new ItemUri(databaseUri, itemId);

            var parentPath = (Path.GetDirectoryName(child.GetAttributeValue("path")) ?? string.Empty).Replace("\\", "/");
            return new TemplateHeader(itemUri, child.InnerText, child.GetAttributeValue("icon"), child.GetAttributeValue("path"), parentPath, child.LocalName == "branch");
        }

        protected bool HandleAsyncOperation([NotNull] AsyncCompletedEventArgs args, [NotNull] Func<XmlElement> getResult)
        {
            Debug.ArgumentNotNull(args, nameof(args));
            Debug.ArgumentNotNull(getResult, nameof(getResult));

            EndCommunication();

            if (args.Cancelled)
            {
                return false;
            }

            if (args.Error != null)
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => TroubleshooterPipeline.Run().WithParameters(this, false, args.Error, GetEndpoint())));
                return false;
            }

            return ProcessStatus(getResult());
        }

        protected bool HandleSyncOperation([NotNull] Exception ex)
        {
            Debug.ArgumentNotNull(ex, nameof(ex));

            Dispatcher.CurrentDispatcher.Invoke(new Action(() => TroubleshooterPipeline.Run().WithParameters(this, false, ex, GetEndpoint())));

            return false;
        }

        private void BeginCommunication()
        {
            AppHost.Statusbar.SetText(Resources.WebDataService_BeginCommunication_Communicating_with_the_Sitecore_server___);
        }

        private bool CheckCredentials()
        {
            if (!string.IsNullOrEmpty(Connection.UserName))
            {
                return true;
            }

            AppHost.MessageBox(Resources.WebDataService_CheckCredentials_, Resources.Information, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            return false;
        }

        private void EndCommunication()
        {
            AppHost.Statusbar.SetText(string.Empty);
        }

        [NotNull]
        private Item GetItem([NotNull] ItemVersionUri uri, [NotNull] XmlElement root)
        {
            Debug.ArgumentNotNull(uri, nameof(uri));
            Debug.ArgumentNotNull(root, nameof(root));

            return WebServiceMethods.GetItem.Call(uri, root);
        }

        [NotNull]
        private string GetXml([NotNull] List<Field> fields, bool modifiedOnly)
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

        private bool ProcessStatus([CanBeNull] XmlElement result)
        {
            if (result == null)
            {
                return false;
            }

            var status = result.SelectSingleNode(@"/status");
            if (status == null)
            {
                return true;
            }

            var text = status.InnerText;
            if (text == @"ok")
            {
                return true;
            }

            var error = result.SelectSingleNode(@"/error");
            if (error == null)
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => AppHost.MessageBox(Resources.WebDataService_ProcessStatus_Unspecified_error_, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error)));
                return false;
            }

            text = error.InnerText;
            if (string.IsNullOrEmpty(text))
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(() => AppHost.MessageBox(Resources.WebDataService_ProcessStatus_Unspecified_error_, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error)));
                return false;
            }

            Dispatcher.CurrentDispatcher.Invoke(new Action(() => AppHost.MessageBox(text, Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error)));

            return false;
        }
    }
}
