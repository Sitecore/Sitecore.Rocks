// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Data
{
    public static class IdManager
    {
        public const string SitecoreTemplatesSystemLayoutLayout = "/sitecore/templates/system/layout/Layout";

        public const string SitecoreTemplatesSystemLayoutLayoutDataPath = "/sitecore/templates/System/Layout/Layout/Data/Path";

        public const string SitecoreTemplatesSystemLayoutRenderingsSublayout = "/sitecore/templates/System/Layout/Renderings/Sublayout";

        public const string SitecoreTemplatesSystemLayoutRenderingsSublayoutDataPath = "/sitecore/templates/System/Layout/Renderings/Sublayout/Data/Path";

        public const string SitecoreTemplatesSystemLayoutRenderingsXslRendering = "/sitecore/templates/System/Layout/Renderings/Xsl Rendering";

        public const string SitecoreTemplatesSystemLayoutRenderingsXslRenderingDataPath = "/sitecore/templates/System/Layout/Renderings/Xsl Rendering/Data/Path";

        private static bool _isLoaded;

        private static Dictionary<Guid, Descriptor> _itemIds;

        private static Dictionary<string, Descriptor> _items;

        [UsedImplicitly]
        public static void AddItem([NotNull] string path, [NotNull] ItemId itemId)
        {
            Assert.ArgumentNotNull(path, nameof(path));
            Assert.ArgumentNotNull(itemId, nameof(itemId));

            Load();

            _items[path.ToLowerInvariant()] = new Descriptor(itemId.ToGuid(), path);
        }

        [NotNull]
        public static FieldId GetFieldId([NotNull, Localizable(false)] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            Load();

            Descriptor result;
            if (_items.TryGetValue(path.ToLowerInvariant(), out result))
            {
                return new FieldId(result.Id);
            }

            throw Exceptions.InvalidOperation(path + @" not found");
        }

        [NotNull]
        public static ItemId GetItemId([NotNull, Localizable(false)] string path)
        {
            Assert.ArgumentNotNull(path, nameof(path));

            Load();

            Descriptor result;
            if (_items.TryGetValue(path.ToLowerInvariant(), out result))
            {
                return new ItemId(result.Id);
            }

            throw Exceptions.InvalidOperation(path + @" not found");
        }

        [NotNull, Localizable(false)]
        public static string GetTemplateType([NotNull] ItemId templateId)
        {
            Assert.ArgumentNotNull(templateId, nameof(templateId));

            Load();

            Descriptor result;
            if (_itemIds.TryGetValue(templateId.ToGuid(), out result))
            {
                return result.Type;
            }

            return string.Empty;
        }

        public static bool IsTemplate([NotNull] ItemId templateId, [NotNull, Localizable(false)] string type)
        {
            Assert.ArgumentNotNull(templateId, nameof(templateId));
            Assert.ArgumentNotNull(type, nameof(type));

            var t = GetTemplateType(templateId);

            return string.Compare(t, type, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static void Load()
        {
            if (_isLoaded)
            {
                return;
            }

            _isLoaded = true;

            _items = new Dictionary<string, Descriptor>();
            _itemIds = new Dictionary<Guid, Descriptor>();

            var doc = GetDocument();
            if (doc == null)
            {
                return;
            }

            var root = doc.Root;
            if (root == null)
            {
                return;
            }

            foreach (var element in root.Elements(@"item"))
            {
                var id = element.GetAttributeValue("id");
                var path = element.GetAttributeValue("path").ToLowerInvariant();
                var type = element.GetAttributeValue("type");

                var descriptor = new Descriptor(new Guid(id), path, type);

                _items[descriptor.Path] = descriptor;
                _itemIds[descriptor.Id] = descriptor;
            }
        }

        [UsedImplicitly]
        public static void Save()
        {
            if (!_isLoaded)
            {
                return;
            }

            var fileName = GetFileName();

            using (var output = new XmlTextWriter(fileName, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 2,
                IndentChar = ' '
            })
            {
                output.WriteStartElement(@"_items");

                foreach (var item in _items)
                {
                    output.WriteStartElement(@"item");

                    output.WriteAttributeString(@"id", item.Value.Id.ToString(@"B").ToUpperInvariant());
                    output.WriteAttributeString(@"path", item.Value.Path);

                    if (!string.IsNullOrEmpty(item.Value.Type))
                    {
                        output.WriteAttributeString(@"type", item.Value.Type);
                    }

                    output.WriteEndElement();
                }

                output.WriteEndElement();
            }
        }

        [CanBeNull]
        private static XDocument GetDocument()
        {
            var fileName = GetFileName();
            if (!File.Exists(fileName))
            {
                return null;
            }

            return XDocument.Load(fileName);
        }

        [NotNull]
        private static string GetFileName()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"id.xml");
        }

        private class Descriptor
        {
            public Descriptor(Guid id, [NotNull] string path)
            {
                Assert.ArgumentNotNull(path, nameof(path));

                Id = id;
                Path = path;
                Type = string.Empty;
            }

            public Descriptor(Guid id, [NotNull] string path, [NotNull] string type)
            {
                Assert.ArgumentNotNull(path, nameof(path));
                Assert.ArgumentNotNull(type, nameof(type));

                Id = id;
                Path = path;
                Type = type;
            }

            public Guid Id { get; }

            [NotNull]
            public string Path { get; }

            [NotNull]
            public string Type { get; }
        }
    }
}
