// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Extensions.XElementExtensions;

namespace Sitecore.Rocks.Server.Requests.Templates
{
    public class SaveTemplateXml
    {
        [NotNull]
        public string Execute([NotNull] string databaseName, [NotNull] string xml)
        {
            Assert.ArgumentNotNull(databaseName, nameof(databaseName));
            Assert.ArgumentNotNull(xml, nameof(xml));

            var database = Factory.GetDatabase(databaseName);
            if (database == null)
            {
                throw new Exception("Database not found");
            }

            var doc = XDocument.Parse(xml);

            var element = doc.Root;
            if (element == null)
            {
                return string.Empty;
            }

            var template = new Template(database, element);
            if (template.Item == null)
            {
                CreateNewTemplate(database, template);
            }
            else
            {
                UpdateTemplate(template);
                DeleteSections(template);
            }

            SortSections(template);

            var item = template.Item;
            return item == null ? string.Empty : item.ID.ToString();
        }

        private void CreateNewTemplate([NotNull] Database database, [NotNull] Template template)
        {
            Debug.ArgumentNotNull(database, nameof(database));
            Debug.ArgumentNotNull(template, nameof(template));

            Item root = null;

            if (!string.IsNullOrEmpty(template.ParentPath))
            {
                root = database.GetItem(template.ParentPath);

                if (root == null && !ID.IsID(template.ParentPath))
                {
                    var innerItem = database.GetItem(TemplateIDs.TemplateFolder);
                    if (innerItem != null)
                    {
                        var templateFolder = new TemplateItem(innerItem);
                        root = database.CreateItemPath(template.ParentPath, templateFolder, templateFolder);
                    }
                }
            }

            if (root == null)
            {
                root = database.GetItem(ItemIDs.TemplateRoot);
                if (root == null)
                {
                    return;
                }

                var item = root.Children["User Defined"];
                if (item != null)
                {
                    root = item;
                }
            }

            if (string.IsNullOrEmpty(template.ID))
            {
                template.ID = Guid.NewGuid().ToString("B").ToUpperInvariant();
            }

            template.Item = ItemManager.AddFromTemplate(template.Name, new TemplateID(TemplateIDs.Template), root, new ID(template.ID));

            template.Item.Editing.BeginEdit();
            template.Item[FieldIDs.BaseTemplate] = template.BaseTemplates;
            template.Item.Appearance.Icon = template.Icon;
            template.Item.Editing.EndEdit();

            foreach (var section in template.Sections)
            {
                UpdateSection(template, section);
            }
        }

        private void DeleteFields([NotNull] Section section)
        {
            Debug.ArgumentNotNull(section, nameof(section));

            foreach (Item child in section.Item.Children)
            {
                if (child.TemplateID != TemplateIDs.TemplateField)
                {
                    continue;
                }

                var found = false;

                foreach (var field in section.Fields)
                {
                    if (field.Item == null)
                    {
                        continue;
                    }

                    if (field.Item.ID == child.ID)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    child.Recycle();
                }
            }
        }

        private void DeleteSections([NotNull] Template template)
        {
            Debug.ArgumentNotNull(template, nameof(template));

            foreach (Item child in template.Item.Children)
            {
                if (child.TemplateID != TemplateIDs.TemplateSection)
                {
                    continue;
                }

                var found = false;

                foreach (var section in template.Sections)
                {
                    if (section.Item == null)
                    {
                        continue;
                    }

                    if (section.Item.ID == child.ID)
                    {
                        DeleteFields(section);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    child.Recycle();
                }
            }
        }

        private void SortFields([NotNull] Section section)
        {
            Debug.ArgumentNotNull(section, nameof(section));

            var lastSortorder = 0;

            for (var index = 0; index < section.Fields.Count; index++)
            {
                var field = section.Fields[index];

                var sortorder = field.Item.Appearance.Sortorder;

                if (sortorder <= lastSortorder)
                {
                    var nextSortorder = lastSortorder + 200;

                    if (index < section.Fields.Count - 1)
                    {
                        nextSortorder = section.Fields[index + 1].Item.Appearance.Sortorder;
                        if (nextSortorder < lastSortorder + 2)
                        {
                            nextSortorder = lastSortorder + 200;
                        }
                    }

                    sortorder = lastSortorder + (nextSortorder - lastSortorder) / 2;

                    field.Item.Editing.BeginEdit();
                    field.Item.Appearance.Sortorder = sortorder;
                    field.Item.Editing.EndEdit();
                }

                lastSortorder = sortorder;
            }
        }

        private void SortSections([NotNull] Template template)
        {
            Debug.ArgumentNotNull(template, nameof(template));

            var lastSortorder = 0;

            for (var index = 0; index < template.Sections.Count; index++)
            {
                var section = template.Sections[index];

                var sortorder = section.Item.Appearance.Sortorder;

                if (sortorder <= lastSortorder)
                {
                    var nextSortorder = lastSortorder + 200;

                    if (index < template.Sections.Count - 1)
                    {
                        nextSortorder = template.Sections[index + 1].Item.Appearance.Sortorder;
                        if (nextSortorder < lastSortorder + 2)
                        {
                            nextSortorder = lastSortorder + 200;
                        }
                    }

                    sortorder = lastSortorder + (nextSortorder - lastSortorder) / 2;

                    section.Item.Editing.BeginEdit();
                    section.Item.Appearance.Sortorder = sortorder;
                    section.Item.Editing.EndEdit();
                }

                SortFields(section);

                lastSortorder = sortorder;
            }
        }

        private void UpdateField([NotNull] Section section, [NotNull] Field field)
        {
            Debug.ArgumentNotNull(section, nameof(section));
            Debug.ArgumentNotNull(field, nameof(field));

            if (field.Item == null)
            {
                if (string.IsNullOrEmpty(field.ID))
                {
                    field.ID = Guid.NewGuid().ToString("B").ToUpperInvariant();
                }

                field.Item = ItemManager.AddFromTemplate(field.Name, new TemplateID(TemplateIDs.TemplateField), section.Item, new ID(field.ID));
            }
            else if (field.Item.ParentID != section.Item.ID)
            {
                field.Item.MoveTo(section.Item);
            }

            field.Item.Editing.BeginEdit();

            field.Item.Name = field.Name;
            field.Item["Type"] = field.Type;
            field.Item["Shared"] = field.Shared ? "1" : string.Empty;
            field.Item["Unversioned"] = field.Unversioned ? "1" : string.Empty;
            /* field.Item["Title"] = field.Title; */
            field.Item["Source"] = field.Source;
            field.Item["Validator Bar"] = field.ValidatorBar;

            field.Item.Editing.EndEdit();
        }

        private void UpdateSection([NotNull] Template template, [NotNull] Section section)
        {
            Debug.ArgumentNotNull(template, nameof(template));
            Debug.ArgumentNotNull(section, nameof(section));

            if (section.Item == null)
            {
                if (string.IsNullOrEmpty(section.ID))
                {
                    section.ID = Guid.NewGuid().ToString("B").ToUpperInvariant();
                }

                section.Item = ItemManager.AddFromTemplate(section.Name, new TemplateID(TemplateIDs.TemplateSection), template.Item, new ID(section.ID));
            }
            else
            {
                if (section.Item.ParentID != template.Item.ID)
                {
                    section.Item.MoveTo(template.Item);
                }

                if (section.Item.Name != section.Name)
                {
                    section.Item.Editing.BeginEdit();
                    section.Item.Name = section.Name;
                    section.Item.Editing.EndEdit();
                }
            }

            foreach (var field in section.Fields)
            {
                UpdateField(section, field);
            }
        }

        private void UpdateTemplate([NotNull] Template template)
        {
            Debug.ArgumentNotNull(template, nameof(template));

            template.Item.Editing.BeginEdit();
            template.Item[FieldIDs.BaseTemplate] = template.BaseTemplates;
            template.Item.Editing.EndEdit();

            foreach (var section in template.Sections)
            {
                UpdateSection(template, section);
            }

            if (template.Item.Name == template.Name && template.Item.Appearance.Icon == template.Icon)
            {
                return;
            }

            template.Item.Editing.BeginEdit();
            template.Item.Name = template.Name;
            template.Item.Appearance.Icon = template.Icon;
            template.Item.Editing.EndEdit();
        }

        private class Field
        {
            public Field([NotNull] Database database, [NotNull] XElement element)
            {
                Debug.ArgumentNotNull(database, nameof(database));
                Debug.ArgumentNotNull(element, nameof(element));

                Name = element.GetAttributeValue("name");
                ID = element.GetAttributeValue("id");
                Type = element.GetAttributeValue("type");
                Shared = element.GetAttributeValue("shared") == "1";
                Unversioned = element.GetAttributeValue("unversioned") == "1";
                Title = element.GetAttributeValue("title");
                Source = element.GetAttributeValue("source");
                ValidatorBar = element.GetAttributeValue("validatorbar");

                if (!string.IsNullOrEmpty(ID))
                {
                    Item = database.GetItem(ID);
                }
            }

            public string ID { get; set; }

            public Item Item { get; set; }

            public string Name { get; }

            public bool Shared { get; }

            public string Source { get; }

            public string Title { get; private set; }

            public string Type { get; }

            public bool Unversioned { get; }

            public string ValidatorBar { get; }
        }

        private class Section
        {
            public Section([NotNull] Database database, [NotNull] XElement element)
            {
                Debug.ArgumentNotNull(database, nameof(database));
                Debug.ArgumentNotNull(element, nameof(element));

                Fields = new List<Field>();

                Name = element.GetAttributeValue("name");
                ID = element.GetAttributeValue("id");

                if (!string.IsNullOrEmpty(ID))
                {
                    Item = database.GetItem(ID);
                }

                foreach (var child in element.Elements())
                {
                    var field = new Field(database, child);
                    Fields.Add(field);
                }
            }

            public List<Field> Fields { get; }

            public string ID { get; set; }

            public Item Item { get; set; }

            public string Name { get; }
        }

        private class Template
        {
            public Template([NotNull] Database database, [NotNull] XElement element)
            {
                Debug.ArgumentNotNull(database, nameof(database));
                Debug.ArgumentNotNull(element, nameof(element));

                Sections = new List<Section>();

                Name = element.GetAttributeValue("name");
                ID = element.GetAttributeValue("id");
                BaseTemplates = element.GetAttributeValue("basetemplates");
                Icon = element.GetAttributeValue("icon");
                ParentPath = element.GetAttributeValue("parentpath");

                if (string.IsNullOrEmpty(BaseTemplates))
                {
                    BaseTemplates = "{1930BBEB-7805-471A-A3BE-4858AC7CF696}";
                }

                if (!string.IsNullOrEmpty(ID))
                {
                    Item = database.GetItem(ID);
                }

                foreach (var child in element.Elements())
                {
                    var section = new Section(database, child);
                    Sections.Add(section);
                }
            }

            public string BaseTemplates { get; }

            public string Icon { get; }

            public string ID { get; set; }

            public Item Item { get; set; }

            public string Name { get; }

            [NotNull]
            public string ParentPath { get; }

            public List<Section> Sections { get; }
        }
    }
}
