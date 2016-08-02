// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Extensions.DataServiceExtensions
{
    public static class DataServiceExtensions
    {
        private static readonly XDocument emptyDocument = new XDocument();

        [NotNull]
        public static XDocument EmptyDocument
        {
            get { return emptyDocument; }
        }

        public static void Attach([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] string filePath, [NotNull] byte[] file, [NotNull] GetValueCompleted<bool> uploadCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(filePath, nameof(filePath));
            Assert.ArgumentNotNull(file, nameof(file));
            Assert.ArgumentNotNull(uploadCompleted, nameof(uploadCompleted));

            var data = Convert.ToBase64String(file);

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (DataService.HandleExecute(response, executeResult))
                {
                    uploadCompleted(true);
                }
            };

            itemUri.Site.DataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Media.Attach", c, itemUri.DatabaseName.Name, itemUri.ItemId.ToString(), filePath, data);
        }

        public static void ChangeTemplate([NotNull] this DataService dataService, [NotNull] ItemVersionUri itemUri, [NotNull] ItemUri templateUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Templates.ChangeTemplate", callback, itemUri.ItemId.ToString(), templateUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        public static void ChangeTemplates([NotNull] this DataService dataService, [NotNull] IEnumerable<ItemUri> uriList, [NotNull] ItemUri templateUri, [NotNull] ExecuteCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(uriList, nameof(uriList));
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            var list = uriList.ToList();

            var itemUri = list.FirstOrDefault();
            if (itemUri == null || itemUri == ItemUri.Empty)
            {
                return;
            }

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Templates.ChangeTemplates", callback, string.Join(@"|", list.Select(uri => uri.ItemId.ToString())), templateUri.ItemId.ToString(), itemUri.DatabaseName.ToString());
        }

        public static void Commit([NotNull] this DataService dataService, [NotNull] string fileName, [NotNull] string fileContent, long serverTimestamp, long serverFileSize, [NotNull] string action, bool isDryRun, [NotNull] CommitCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(fileContent, nameof(fileContent));
            Assert.ArgumentNotNull(action, nameof(action));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(string.Empty, 0, 0);
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(string.Empty, 0, 0);
                    return;
                }

                var result = root.Value;

                long st;
                if (!long.TryParse(root.GetAttributeValue("timestamp"), out st))
                {
                    st = 0;
                }

                long ss;
                if (!long.TryParse(root.GetAttributeValue("filesize"), out ss))
                {
                    ss = 0;
                }

                callback(result, st, ss);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Projects.Commit", c, fileName, fileContent, serverTimestamp.ToString(), serverFileSize, action, isDryRun);
        }

        public static void CreateItemPath([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] string path, [NotNull] GetItemsCompleted<ItemPath> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemPath>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<ItemPath>());
                    return;
                }

                var result = new List<ItemPath>();

                foreach (var element in root.Elements())
                {
                    var itemPath = new ItemPath
                    {
                        Name = element.GetAttributeValue("name"),
                        ItemUri = new ItemUri(databaseUri, new ItemId(new Guid(element.GetAttributeValue("id"))))
                    };

                    result.Add(itemPath);
                }

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.CreateItemPath", c, databaseUri.DatabaseName.Name, path);
        }

        public static void Detach([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] GetValueCompleted<bool> detachCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(detachCompleted, nameof(detachCompleted));

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                detachCompleted(true);
            };

            itemUri.Site.DataService.ExecuteAsync("Media.Detach", completed, itemUri.DatabaseName.Name, itemUri.ItemId.ToString());
        }

        public static void DownloadAttachment([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] GetValueCompleted<byte[]> downloadCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(downloadCompleted, nameof(downloadCompleted));

            ExecuteCompleted completed = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var data = Convert.FromBase64String(response);

                downloadCompleted(data);
            };

            itemUri.Site.DataService.ExecuteAsync("Media.DownloadAttachment", completed, itemUri.DatabaseName.Name, itemUri.ItemId.ToString());
        }

        [UsedImplicitly]
        public static void Exists([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] string itemPath, [NotNull] GetValueCompleted<bool> existsCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));
            Assert.ArgumentNotNull(existsCompleted, nameof(existsCompleted));

            ExecuteCompleted c = (response, executeResult) => existsCompleted(response == @"true");

            databaseUri.Site.DataService.ExecuteAsync("Items.Exists", c, databaseUri.DatabaseName.Name, itemPath);
        }

        public static void GetArchivedItems([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [Localizable(false), NotNull] string archiveName, int pageNumber, [NotNull] GetValueCompleted<XDocument> getArchivedItemsCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(archiveName, nameof(archiveName));
            Assert.ArgumentNotNull(getArchivedItemsCompleted, nameof(getArchivedItemsCompleted));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    getArchivedItemsCompleted(EmptyDocument);
                    return;
                }

                getArchivedItemsCompleted(GetDocument(response));
            };

            dataService.ExecuteAsync("Archives.GetArchivedItems", c, databaseUri.DatabaseName.ToString(), archiveName, pageNumber);
        }

        public static void GetFieldTypes([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] GetFieldTypesCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<FieldTypeHeader>(), Enumerable.Empty<FieldValidationHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<FieldTypeHeader>(), Enumerable.Empty<FieldValidationHeader>());
                    return;
                }

                var fieldTypes = new List<FieldTypeHeader>();

                var fieldTypesElement = root.Element(@"fieldtypes");
                if (fieldTypesElement != null)
                {
                    foreach (var element in fieldTypesElement.Elements())
                    {
                        var item = new FieldTypeHeader(new ItemUri(databaseUri, new ItemId(new Guid(element.GetAttributeValue("id")))), element.GetAttributeValue("name"), element.GetAttributeValue("icon"), element.GetAttributeValue("path"), element.GetAttributeValue("section"));

                        fieldTypes.Add(item);
                    }
                }

                fieldTypes.Sort(new FieldTypeHeaderComparer());

                var validations = new List<FieldValidationHeader>();

                var validationsElement = root.Element(@"validations");
                if (validationsElement != null)
                {
                    foreach (var element in validationsElement.Elements())
                    {
                        var item = new FieldValidationHeader
                        {
                            Name = element.GetAttributeValue("name"),
                            Section = element.GetAttributeValue("section"),
                            ItemUri = new ItemUri(databaseUri, new ItemId(new Guid(element.GetAttributeValue("id"))))
                        };

                        validations.Add(item);
                    }
                }

                callback(fieldTypes, validations);
            };

            dataService.ExecuteAsync("Templates.GetFieldTypes", c, databaseUri.DatabaseName.ToString());
        }

        public static void GetFiles([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] string folder, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(folder, nameof(folder));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var result = new List<ItemHeader>();
                foreach (var element in root.Elements())
                {
                    var name = element.GetAttributeValue("name");
                    var path = element.GetAttributeValue("path");
                    var type = element.GetAttributeValue("type");
                    var updated = element.GetAttributeIsoDateTime("updated", DateTime.MinValue);

                    var item = new ItemHeader
                    {
                        Name = name,
                        Path = path,
                        ItemUri = new ItemUri(databaseUri, ItemId.Empty),
                        HasChildren = type == @"folder",
                        Updated = updated
                    };

                    item.SetData("ServerFileName", element.GetAttributeValue("filename"));

                    result.Add(item);
                }

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.GetFiles", c, folder);
        }

        public static void GetInsertOptions([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var result = root.Elements().Select(element => ItemHeader.Parse(itemUri.DatabaseUri, element)).ToList();

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.GetInsertOptions", c, itemUri.DatabaseName.Name, itemUri.ItemId.ToString());
        }

        [UsedImplicitly]
        public static void GetItemHeader([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] GetValueCompleted<ItemHeader> getItemHeaderCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(getItemHeaderCompleted, nameof(getItemHeaderCompleted));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = Parse(response);
                if (root != null)
                {
                    getItemHeaderCompleted(ItemHeader.Parse(itemUri.DatabaseUri, root));
                }
            };

            itemUri.Site.DataService.ExecuteAsync("Items.GetItemHeader", c, itemUri.ItemId.ToString(), itemUri.DatabaseName.Name);
        }

        [NotNull, Obsolete("Use ItemHeader.Parse instead")]
        public static ItemHeader GetItemHeader([NotNull] XElement element, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            return ItemHeader.Parse(databaseUri, element);
        }

        public static void GetJobs([NotNull] this DataService dataService, [NotNull] GetValueCompleted<XDocument> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(EmptyDocument);
                    return;
                }

                callback(GetDocument(response));
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.UI.JobViewer.GetJobs", c);
        }

        public static void GetLayout([NotNull] this DataService dataService, [NotNull] string layout, [NotNull] DatabaseUri databaseUri, [NotNull] GetValueCompleted<XDocument> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(layout, nameof(layout));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(EmptyDocument);
                    return;
                }

                callback(GetDocument(response));
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.GetLayout", c, layout, databaseUri.DatabaseName.Name);
        }

        public static void GetLayoutsAsync([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<LayoutHeader> completed)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(completed, nameof(completed));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    completed(Enumerable.Empty<LayoutHeader>());
                    return;
                }

                var list = new List<LayoutHeader>();

                var doc = GetDocument(response);
                if (doc == null)
                {
                    completed(Enumerable.Empty<LayoutHeader>());
                    return;
                }

                var root = doc.Root;
                if (root == null)
                {
                    completed(Enumerable.Empty<LayoutHeader>());
                    return;
                }

                foreach (var child in root.Elements())
                {
                    var itemId = new ItemId(new Guid(child.GetAttributeValue("id")));

                    var itemUri = new ItemUri(databaseUri, itemId);

                    var parentPath = (Path.GetDirectoryName(child.GetAttributeValue("path")) ?? string.Empty).Replace("\\", "/");
                    var layout = new LayoutHeader(itemUri, child.Value, child.GetAttributeValue("icon"), child.GetAttributeValue("path"), parentPath);

                    list.Add(layout);
                }

                completed(list);
            };

            dataService.ExecuteAsync("Layouts.GetLayouts", c, databaseUri.DatabaseName.ToString());
        }

        public static void GetLinks([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, [NotNull] GetLinksCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(string.Empty, Enumerable.Empty<ItemHeader>(), Enumerable.Empty<ItemHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(string.Empty, Enumerable.Empty<ItemHeader>(), Enumerable.Empty<ItemHeader>());
                    return;
                }

                var referencesElement = root.Element(@"references");
                var referrersElement = root.Element(@"referrers");

                if (referencesElement == null || referrersElement == null)
                {
                    callback(string.Empty, Enumerable.Empty<ItemHeader>(), Enumerable.Empty<ItemHeader>());
                    return;
                }

                var name = root.GetAttributeValue("name");
                var references = referencesElement.Elements().Select(element => ItemHeader.Parse(itemUri.DatabaseUri, element)).ToList();
                var referrers = referrersElement.Elements().Select(element => ItemHeader.Parse(itemUri.DatabaseUri, element)).ToList();

                callback(name, references, referrers);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Links.GetLinks", c, itemUri.DatabaseName.Name, itemUri.ItemId.ToString());
        }

        public static void GetLog([NotNull] this DataService dataService, int maxItems, [NotNull] string categories, [NotNull] string includeFilter, [NotNull] string excludeFilter, [NotNull] GetValueCompleted<XDocument> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(categories, nameof(categories));
            Assert.ArgumentNotNull(includeFilter, nameof(includeFilter));
            Assert.ArgumentNotNull(excludeFilter, nameof(excludeFilter));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(EmptyDocument);
                    return;
                }

                callback(GetDocument(response));
            };

            dataService.ExecuteAsync("UI.LogViewer.GetLog", c, maxItems, categories, includeFilter, excludeFilter);
        }

        public static void GetTemplateXml([NotNull] this DataService dataService, [NotNull] ItemUri itemUri, bool includeInheritedFields, [NotNull] GetValueCompleted<XDocument> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(itemUri, nameof(itemUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(EmptyDocument);
                    return;
                }

                callback(GetDocument(response));
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Templates.GetTemplateXml", c, itemUri.DatabaseName.Name, itemUri.ItemId.ToString(), includeInheritedFields);
        }

        public static void Publish([NotNull] this DataService dataService, int mode, [NotNull] DatabaseUri databaseUri)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Publishing.Publish", HandleExecute, databaseUri.DatabaseName.Name, mode);
        }

        public static void PublishItem([NotNull] this DataService dataService, [NotNull] IEnumerable<ItemUri> items, bool deep, bool compareRevisions)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(items, nameof(items));

            dataService.ExecuteAsync("Publishing.PublishItem", HandleExecute, items.First().DatabaseName.Name, string.Join(@",", items.Select(item => item.ItemId.ToString())), deep, compareRevisions);
        }

        public static void Query([NotNull] this DataService dataService, [NotNull] string queryText, [NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var result = new List<ItemHeader>();

                foreach (var element in root.Elements())
                {
                    result.Add(ItemHeader.Parse(databaseUri, element));
                }

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Query", c, queryText);
        }

        public static void SaveTemplateXml([NotNull] this DataService dataService, [NotNull] string xml, [NotNull] DatabaseUri databaseUri, [NotNull] GetValueCompleted<string> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(xml, nameof(xml));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(string.Empty);
                    return;
                }

                callback(response);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Templates.SaveTemplateXml", c, databaseUri.DatabaseName.Name, xml);
        }

        public static void Search([NotNull] this DataService dataService, [NotNull] string queryText, [NotNull] DatabaseUri databaseUri, [Localizable(false), NotNull] string field, [NotNull] string condition, [NotNull] ItemUri root, int pageNumber, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(condition, nameof(condition));
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(callback, nameof(callback));

            var databaseName = databaseUri.DatabaseName.Name;
            var itemId = string.Empty;

            if (root != ItemUri.Empty)
            {
                databaseName = root.DatabaseName.ToString();
                itemId = root.ItemId.ToString();
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var r = Parse(response);
                if (r == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var result = new List<ItemHeader>();

                foreach (var element in r.Elements())
                {
                    result.Add(ItemHeader.Parse(databaseUri, element));
                }

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Search.Search", c, queryText, field, condition, databaseName, itemId, pageNumber);
        }

        public static void SearchMedia([NotNull] this DataService dataService, [NotNull] string queryText, [NotNull] DatabaseUri databaseUri, [Localizable(false), NotNull] string field, [NotNull] string condition, [NotNull] ItemUri root, int pageNumber, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(condition, nameof(condition));
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(callback, nameof(callback));

            var databaseName = databaseUri.DatabaseName.Name;
            var itemId = string.Empty;

            if (root != ItemUri.Empty)
            {
                databaseName = root.DatabaseName.ToString();
                itemId = root.ItemId.ToString();
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var r = Parse(response);
                if (r == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var result = new List<ItemHeader>();

                foreach (var element in r.Elements())
                {
                    result.Add(ItemHeader.Parse(databaseUri, element));
                }

                callback(result);
            };

            dataService.ExecuteAsync("Media.Search", c, queryText, field, condition, databaseName, itemId, pageNumber);
        }

        public static void SelectItems([NotNull] this DataService dataService, [NotNull] string queryText, [NotNull] DatabaseUri databaseUri, [NotNull] GetItemsCompleted<ItemHeader> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(queryText, nameof(queryText));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<ItemHeader>());
                    return;
                }

                var items = root.Elements().Select(element => ItemHeader.Parse(databaseUri, element)).ToList();

                callback(items);
            };

            dataService.ExecuteAsync("Search.SelectItems", c, queryText, databaseUri.DatabaseName.ToString());
        }

        public static void Update([NotNull] this DataService dataService, [NotNull] string fileName, [NotNull] string siteTimestamp, long siteFileSize, [NotNull] string action, [NotNull] UpdateCompleted callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(fileName, nameof(fileName));
            Assert.ArgumentNotNull(siteTimestamp, nameof(siteTimestamp));
            Assert.ArgumentNotNull(action, nameof(action));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                byte[] file;

                if (!DataService.HandleExecute(response, executeResult))
                {
                    file = new byte[0];
                    callback("failed", ref file, 0, 0);
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    file = new byte[0];
                    callback("failed", ref file, 0, 0);
                    return;
                }

                file = Convert.FromBase64String(root.Value);

                var result = root.GetAttributeValue("status");

                long serverTimestamp;
                if (!long.TryParse(root.GetAttributeValue("timestamp"), out serverTimestamp))
                {
                    serverTimestamp = 0;
                }

                long serverFileSize;
                if (!long.TryParse(root.GetAttributeValue("filesize"), out serverFileSize))
                {
                    serverFileSize = 0;
                }

                callback(result, ref file, serverTimestamp, serverFileSize);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Projects.Update", c, fileName, siteTimestamp, siteFileSize, action);
        }

        public static void Upload([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] string filePath, [NotNull] byte[] file, [NotNull] GetValueCompleted<ItemHeader> uploadCompleted)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(filePath, nameof(filePath));
            Assert.ArgumentNotNull(file, nameof(file));
            Assert.ArgumentNotNull(uploadCompleted, nameof(uploadCompleted));

            var data = Convert.ToBase64String(file);

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var r = Parse(response);
                if (r == null)
                {
                    return;
                }

                var element = r.Element(@"item");
                if (element == null)
                {
                    return;
                }

                var itemHeader = ItemHeader.Parse(databaseUri, element);

                uploadCompleted(itemHeader);
            };

            databaseUri.Site.DataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Media.Upload", c, databaseUri.DatabaseName.Name, filePath, data);
        }

        public static void Validate([NotNull] this DataService dataService, [NotNull] DatabaseUri databaseUri, [NotNull] List<Field> fields, [NotNull] GetItemsCompleted<Validator> callback)
        {
            Assert.ArgumentNotNull(dataService, nameof(dataService));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(fields, nameof(fields));
            Assert.ArgumentNotNull(callback, nameof(callback));

            ExecuteCompleted c = delegate(string response, ExecuteResult executeResult)
            {
                if (!DataService.HandleExecute(response, executeResult))
                {
                    callback(Enumerable.Empty<Validator>());
                    return;
                }

                var root = Parse(response);
                if (root == null)
                {
                    callback(Enumerable.Empty<Validator>());
                    return;
                }

                var result = new List<Validator>();

                foreach (var element in root.Elements())
                {
                    var fieldId = element.GetAttributeValue("fieldid");

                    var validate = new Validator
                    {
                        Text = element.Value,
                        Result = (ValidatorResult)int.Parse(element.GetAttributeValue("result")),
                        FieldId = string.IsNullOrEmpty(fieldId) ? Guid.Empty : new Guid(fieldId),
                        ValidatorId = new Guid(element.GetAttributeValue("validatorid"))
                    };

                    result.Add(validate);
                }

                callback(result);
            };

            dataService.ExecuteAsync("Sitecore.Rocks.Server.Requests.Validate", c, databaseUri.DatabaseName.Name, GetFieldsXml(fields, false));
        }

        [CanBeNull]
        private static XDocument GetDocument([NotNull] string xml)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch
            {
                return EmptyDocument;
            }

            return doc;
        }

        [NotNull]
        private static string GetFieldsXml([NotNull] List<Field> fields, bool modifiedOnly)
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

        private static void HandleExecute([NotNull] string response, [NotNull] ExecuteResult executeResult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeResult, nameof(executeResult));

            DataService.HandleExecute(response, executeResult);
        }

        [CanBeNull]
        private static XElement Parse([NotNull] string xml)
        {
            Debug.ArgumentNotNull(xml, nameof(xml));

            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml);
            }
            catch
            {
                return null;
            }

            return doc.Root;
        }
    }
}
