// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class Item : ITemplatedItem, IItemData
    {
        private readonly Dictionary<string, string> data = new Dictionary<string, string>();

        static Item()
        {
            Empty = new Item();
        }

        public Item()
        {
            Uri = ItemVersionUri.Empty;
            Name = string.Empty;
            Fields = new List<Field>();
            Versions = new List<int>();
            Languages = new List<string>();
            Path = new List<ItemPath>();
            Warnings = new List<Warning>();
            Icon = Icon.Empty;
            TemplateId = ItemId.Empty;
            TemplateName = string.Empty;
            Breadcrumb = string.Empty;
            Source = string.Empty;
        }

        public List<ItemHeader> BaseTemplates { get; set; }

        public string Breadcrumb { get; set; }

        [NotNull]
        public static Item Empty { get; }

        public List<Field> Fields { get; set; }

        public Icon Icon { get; set; }

        public ItemUri ItemUri => Uri.ItemUri;

        public List<string> Languages { get; set; }

        public string Name { get; set; }

        public List<ItemPath> Path { get; }

        [NotNull]
        public string Source { get; set; }

        public ItemId StandardValuesId { get; set; }

        public ItemId TemplateId { get; set; }

        public string TemplateName { get; set; }

        public ItemVersionUri Uri { get; set; }

        public List<int> Versions { get; set; }

        public List<Warning> Warnings { get; set; }

        [NotNull]
        public string GetPath()
        {
            var result = string.Empty;

            for (var index = Path.Count - 1; index >= 0; index--)
            {
                var itemPath = Path[index];
                result += @"/" + itemPath.Name;
            }

            return result;
        }

        internal void SetData([NotNull] string key, [NotNull] string value)
        {
            Debug.ArgumentNotNull(key, nameof(key));
            Debug.ArgumentNotNull(value, nameof(value));

            data[key] = value;
        }

        string IItemData.GetData(string key)
        {
            Assert.ArgumentNotNull(key, nameof(key));

            string value;
            if (!data.TryGetValue(key, out value))
            {
                return null;
            }

            return value;
        }
    }
}
