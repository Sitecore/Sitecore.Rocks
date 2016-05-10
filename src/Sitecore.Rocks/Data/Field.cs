// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public class Field
    {
        public Field()
        {
            FieldUris = new List<FieldUri>();
            ValueItems = new List<ValueItem>();
            TemplateFieldId = ItemId.Empty;
        }

        [CanBeNull]
        public IFieldControl Control { get; set; }

        public string DisplayData { get; set; }

        public List<FieldUri> FieldUris { get; set; }

        public bool HasValue { get; set; }

        public bool IsBlob { get; set; }

        public bool IsDeclaringTemplate { get; set; }

        public bool IsFiltered { get; set; }

        public bool IsVisible { get; set; }

        public string Lookup { get; set; }

        public string Name { get; set; }

        public string OriginalValue { get; set; }

        public bool ResetOnSave { get; set; }

        public ItemHeader Root { get; set; }

        public Section Section { get; set; }

        public bool Shared { get; set; }

        public int SortOrder { get; set; }

        public string Source { get; set; }

        public bool StandardValue { get; set; }

        public ItemId TemplateFieldId { get; set; }

        public string Title { get; set; }

        public string ToolTip { get; set; }

        public string Type { get; set; }

        public bool Unversioned { get; set; }

        public string Value { get; set; }

        public List<ValueItem> ValueItems { get; private set; }

        internal string ActualFieldType { get; set; }

        [NotNull]
        public Field Clone()
        {
            var result = new Field
            {
                Control = Control,
                Name = Name,
                OriginalValue = OriginalValue,
                Section = Section,
                Source = Source,
                Title = Title,
                ToolTip = ToolTip,
                Type = Type,
                Value = Value,
                HasValue = HasValue,
                IsDeclaringTemplate = IsDeclaringTemplate,
                IsFiltered = IsFiltered,
                Root = Root,
                Shared = Shared,
                SortOrder = SortOrder,
                StandardValue = StandardValue,
                TemplateFieldId = TemplateFieldId,
                Unversioned = Unversioned,
                ValueItems = ValueItems,
                DisplayData = DisplayData,
                IsBlob = IsBlob
            };

            foreach (var r in FieldUris)
            {
                result.FieldUris.Add(r.Clone());
            }

            return result;
        }
    }
}
