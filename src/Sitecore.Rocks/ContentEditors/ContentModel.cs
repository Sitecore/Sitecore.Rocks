// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentEditors
{
    public class ContentModel
    {
        private bool _isModified;

        public ContentModel()
        {
            Items = new List<Item>();
            Fields = new List<Field>();
            Warnings = new List<Warning>();
        }

        [NotNull]
        public List<Field> Fields { get; set; }

        [NotNull]
        public Item FirstItem
        {
            get
            {
                if (IsEmpty)
                {
                    throw Exceptions.InvalidOperation(Resources.ContentModel_FirstItem_Content_Model_is_empty);
                }

                return Items[0];
            }
        }

        public bool IsEmpty => Items.Count == 0;

        public bool IsModified
        {
            get { return _isModified; }

            set
            {
                _isModified = value;

                Notifications.RaiseItemModified(this, this, value);
            }
        }

        public bool IsMultiple => Items.Count > 1;

        public bool IsSingle => Items.Count == 1;

        [NotNull]
        public List<Item> Items { get; set; }

        [NotNull]
        public List<ItemVersionUri> UriList { get; set; }

        [NotNull]
        public List<Warning> Warnings { get; private set; }

        public void BuildModel()
        {
            if (IsSingle)
            {
                Fields = FirstItem.Fields;
            }
            else if (IsMultiple)
            {
                MergeFields();
            }

            if (Fields != null)
            {
                Fields.Sort(new FieldComparer());
            }
        }

        public void GetChanges()
        {
            foreach (var field in Fields)
            {
                var fieldControl = field.Control;
                if (fieldControl == null)
                {
                    continue;
                }

                var value = fieldControl.GetValue();

                if (string.IsNullOrEmpty(value) && !field.HasValue)
                {
                    continue;
                }

                field.Value = value;
                field.HasValue = true;
            }
        }

        public void MergeFields()
        {
            Fields.Clear();

            foreach (var item in Items)
            {
                foreach (var field in item.Fields)
                {
                    if (field == null)
                    {
                        continue;
                    }

                    var f = Find(Fields, field);
                    if (f != null)
                    {
                        field.FieldUris.ForEach(fieldUri => f.FieldUris.Add(fieldUri.Clone()));

                        if (f.Value != field.Value || f.OriginalValue != field.OriginalValue)
                        {
                            f.Value = string.Empty;
                            f.OriginalValue = string.Empty;
                            f.HasValue = false;
                        }

                        continue;
                    }

                    if (IsInAll(field))
                    {
                        Fields.Add(field.Clone());
                    }
                }
            }
        }

        public void ValueModified()
        {
            IsModified = true;
        }

        [CanBeNull]
        private Field Find([NotNull] IEnumerable<Field> list, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(list, nameof(list));
            Debug.ArgumentNotNull(field, nameof(field));

            return list.FirstOrDefault(f => f != null && f.Name == field.Name && f.Type == field.Type);
        }

        private bool IsInAll([NotNull] Field field)
        {
            Debug.ArgumentNotNull(field, nameof(field));

            return Items.All(item => item.Fields.Any(f => f != null && f.Name == field.Name && f.Type == field.Type));
        }
    }
}
