// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Pipelines.DeleteItem;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;
using Sitecore.Rocks.Extensions.DateTimeExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Data
{
    public enum SerializationStatus
    {
        NotSerialized,

        Serialized,

        Modified,

        Error
    }

    public class ItemHeader : ITemplatedItem, IItemData, ICanDelete
    {
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();

        private string _path;

        public ItemHeader()
        {
            HasChildren = false;
            ItemUri = ItemUri.Empty;
            TemplateId = ItemId.Empty;
            Icon = Icon.Empty;
            Name = string.Empty;
            Path = string.Empty;
            TemplateName = string.Empty;
            Gutters = new List<GutterDescriptor>();
            Category = string.Empty;
            UpdatedBy = string.Empty;
            Updated = DateTime.MinValue;
            Locked = string.Empty;
            Ownership = string.Empty;
            ParentName = string.Empty;
            SerializationStatus = SerializationStatus.NotSerialized;
            StandardValuesId = ItemId.Empty;
            StandardValuesField = ItemId.Empty;
            Source = string.Empty;

            // this.ParentPath = string.Empty; - set by this.Path
        }

        [NotNull]
        public string Category { get; set; }

        [NotNull]
        public List<GutterDescriptor> Gutters { get; set; }

        public bool HasChildren { get; set; }

        public Icon Icon { get; set; }

        [NotNull]
        public ItemId ItemId => ItemUri.ItemId;

        public ItemUri ItemUri { get; set; }

        [NotNull]
        public string Locked { get; set; }

        public string Name { get; set; }

        [NotNull]
        public string Ownership { get; set; }

        [NotNull]
        public string ParentName { get; set; }

        [NotNull]
        public string ParentPath { get; private set; }

        [NotNull, Localizable(false)]
        public string Path
        {
            get { return _path; }

            set
            {
                _path = value;

                if (!string.IsNullOrEmpty(_path))
                {
                    var parentPath = Path.Replace("\\", "/");
                    var n = parentPath.LastIndexOf('/');
                    if (n >= 0)
                    {
                        parentPath = parentPath.Left(n);
                    }

                    ParentPath = parentPath;
                }
                else
                {
                    ParentPath = string.Empty;
                }
            }
        }

        public SerializationStatus SerializationStatus { get; set; }

        public int SortOrder { get; set; }

        [NotNull]
        public string Source { get; set; }

        [NotNull]
        public ItemId StandardValuesField { get; set; }

        [NotNull]
        public ItemId StandardValuesId { get; set; }

        public ItemId TemplateId { get; set; }

        [Localizable(false)]
        public string TemplateName { get; set; }

        public DateTime Updated { get; set; }

        [NotNull]
        public string UpdatedBy { get; set; }

        string ICanDelete.Text => Name;

        [NotNull]
        public ItemHeader Clone()
        {
            var result = new ItemHeader
            {
                Name = Name,
                TemplateId = TemplateId,
                TemplateName = TemplateName,
                HasChildren = HasChildren,
                Icon = Icon,
                Path = Path,
                StandardValuesField = StandardValuesField,
                StandardValuesId = StandardValuesId,
                ItemUri = ItemUri,
                SortOrder = SortOrder,
                Gutters = Gutters,
                Category = Category,
                Updated = Updated,
                UpdatedBy = UpdatedBy,
                Locked = Locked,
                Ownership = Ownership,
                ParentName = ParentName,
                Source = Source,
                SerializationStatus = SerializationStatus
            };

            foreach (var pair in _data)
            {
                result._data[pair.Key] = pair.Value;
            }

            return result;
        }

        [NotNull]
        public static ItemHeader Parse([NotNull] DatabaseUri databaseUri, [NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));

            var templateId = ItemId.Empty;
            var standardValuesField = ItemId.Empty;
            var standardValuesId = ItemId.Empty;
            var sortorder = 0;

            var name = element.Value;
            var path = element.GetAttributeValue("path");
            var templateName = element.GetAttributeValue("templatename");
            var category = element.GetAttributeValue("category");
            var updatedBy = element.GetAttributeValue("updatedby");
            var updated = DateTimeExtensions.FromIso(element.GetAttributeValue("updated"));
            var locked = element.GetAttributeValue("locked");
            var ownership = element.GetAttributeValue("owner");
            var parentName = element.GetAttributeValue("parentname");
            var isClone = element.GetAttributeValue("clone") == "1";
            var source = element.GetAttributeValue("source");
            var serializationStatus = SerializationStatus.NotSerialized;

            var value = element.GetAttributeValue("templateid");
            if (!string.IsNullOrEmpty(value))
            {
                templateId = new ItemId(new Guid(value));
            }

            value = element.GetAttributeValue("standardvaluesid");
            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                standardValuesId = Guid.TryParse(value, out guid) ? new ItemId(guid) : ItemId.Empty;
            }

            value = element.GetAttributeValue("standardvaluesfield");
            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                standardValuesField = Guid.TryParse(value, out guid) ? new ItemId(guid) : ItemId.Empty;
            }

            value = element.GetAttributeValue("sortorder");
            if (!string.IsNullOrEmpty(value))
            {
                int.TryParse(value, out sortorder);
            }

            value = element.GetAttributeValue("serializationstatus");
            if (!string.IsNullOrEmpty(value))
            {
                switch (value)
                {
                    case "0":
                        serializationStatus = SerializationStatus.NotSerialized;
                        break;
                    case "1":
                        serializationStatus = SerializationStatus.Serialized;
                        break;
                    case "2":
                        serializationStatus = SerializationStatus.Modified;
                        break;
                    case "3":
                        serializationStatus = SerializationStatus.Error;
                        break;
                }
            }

            var itemDatabaseUri = databaseUri;

            var databaseName = element.GetAttributeValue("database");
            if (!string.IsNullOrEmpty(databaseName) && databaseName != databaseUri.DatabaseName.Name)
            {
                itemDatabaseUri = new DatabaseUri(databaseUri.Site, new DatabaseName(databaseName));
            }

            var result = new ItemHeader
            {
                HasChildren = element.GetAttributeValue("haschildren") == @"1",
                Name = name,
                TemplateId = templateId,
                StandardValuesId = standardValuesId,
                StandardValuesField = standardValuesField,
                Icon = new Icon(itemDatabaseUri.Site, element.GetAttributeValue("icon")),
                ItemUri = new ItemUri(itemDatabaseUri, new ItemId(new Guid(element.GetAttributeValue("id")))),
                TemplateName = templateName,
                Path = path,
                Category = category,
                SortOrder = sortorder,
                UpdatedBy = updatedBy,
                Updated = updated,
                Locked = locked,
                Ownership = ownership,
                ParentName = parentName,
                /* IsClone = isClone, */
                Source = source,
                SerializationStatus = serializationStatus
            };

            foreach (var attribute in element.Attributes())
            {
                if (attribute.Name.ToString().StartsWith(@"ex."))
                {
                    result.SetData(attribute.Name.ToString(), attribute.Value);
                }
            }

            return result;
        }

        public override string ToString()
        {
            return Name;
        }

        internal void SetData([NotNull] string key, [NotNull] string value)
        {
            Debug.ArgumentNotNull(key, nameof(key));
            Debug.ArgumentNotNull(value, nameof(value));

            _data[key] = value;
        }

        void ICanDelete.Delete(bool deleteFiles)
        {
            var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();

            pipeline.ItemUri = ItemUri;
            pipeline.DeleteFiles = deleteFiles;

            pipeline.Start();
        }

        string IItemData.GetData(string key)
        {
            Debug.ArgumentNotNull(key, nameof(key));

            string value;
            if (!_data.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }
    }
}
