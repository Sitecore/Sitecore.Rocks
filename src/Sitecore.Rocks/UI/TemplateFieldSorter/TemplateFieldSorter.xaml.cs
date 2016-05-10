// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;

namespace Sitecore.Rocks.UI.TemplateFieldSorter
{
    public partial class TemplateFieldSorter : ISavable, IContextProvider
    {
        private bool _modified;

        public TemplateFieldSorter()
        {
            InitializeComponent();

            HomeIcon.Source = new Icon("Resources/16x16/home.png").GetSource();

            Model = new List<TemplateFields>();
            Sections = new List<TemplateSection>();
        }

        public List<TemplateFields> Model { get; set; }

        public bool Modified
        {
            get { return _modified; }

            set
            {
                if (IsModifiedTracking > 0)
                {
                    return;
                }

                if (_modified == value)
                {
                    return;
                }

                _modified = value;
                Pane.SetModifiedFlag(value);
            }
        }

        public IEditorPane Pane { get; set; }

        public bool SystemFields { get; set; }

        public ItemUri TemplateUri { get; set; }

        protected List<TemplateSection> Sections { get; set; }

        private int IsModifiedTracking { get; set; }

        public void AddTemplate([NotNull] ItemUri templateUri, bool primary)
        {
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));

            var template = new TemplateFields(templateUri, primary);

            Model.Add(template);

            LoadFields(template, templateUri);
        }

        [NotNull]
        public object GetContext()
        {
            return new TemplateFieldSorterContext(this);
        }

        public void Initialize([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            TemplateUri = templateUri;

            AddTemplate(templateUri, true);

            Pane.Caption = string.Format(Rocks.Resources.TemplateFieldSorter_Initialize_Template_Field_Sorter, templateUri.DatabaseName, templateUri.Site.Name);

            AppHost.Server.Templates.GetTemplateFieldSorterFields(templateUri, RenderInstances);
        }

        public void Refresh()
        {
            IsModifiedTracking++;

            try
            {
                Pane.Caption = string.Format(Rocks.Resources.TemplateFieldSorter_Refresh__0____Template_Field_Sorter, Model.First().Name);
                Templates.Children.Clear();
                Templates.ColumnDefinitions.Clear();
                Sections.Clear();

                foreach (var template in Model)
                {
                    if (Templates.ColumnDefinitions.Count > 0)
                    {
                        var splitterColumn = new ColumnDefinition
                        {
                            Width = new GridLength(3)
                        };
                        Templates.ColumnDefinitions.Add(splitterColumn);

                        var splitter = new GridSplitter
                        {
                            ResizeDirection = GridResizeDirection.Columns,
                            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
                            Width = 3,
                            Background = new SolidColorBrush(Color.FromRgb(0x99, 0x99, 0x99))
                        };

                        Templates.Children.Add(splitter);
                        splitter.SetValue(Grid.ColumnProperty, Templates.ColumnDefinitions.Count - 1);
                    }

                    var columnDefinition = new ColumnDefinition
                    {
                        MinWidth = 300,
                        Width = new GridLength(1, GridUnitType.Star)
                    };
                    Templates.ColumnDefinitions.Add(columnDefinition);

                    template.StackPanel = new StackPanel();

                    Templates.Children.Add(template.StackPanel);
                    template.StackPanel.SetValue(Grid.ColumnProperty, Templates.ColumnDefinitions.Count - 1);

                    template.SortFields();
                    template.Current = 0;
                    template.CurrentSectionName = null;

                    template.VisibleFields = new List<Field>(template.Fields.Where(field => !field.IsSystemField || SystemFields));
                }

                var sectionName = string.Empty;

                while (true)
                {
                    var nextField = GetNextField(sectionName);
                    if (nextField == null)
                    {
                        break;
                    }

                    sectionName = nextField.SectionName;
                    RenderSections(nextField);
                    RenderFields(nextField);
                }
            }
            finally
            {
                IsModifiedTracking--;
            }
        }

        public void RemoveTemplate([NotNull] TemplateFields templateFields)
        {
            Assert.ArgumentNotNull(templateFields, nameof(templateFields));

            Model.Remove(templateFields);
            Refresh();
        }

        public void Save()
        {
            Refresh();

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement(@"fields");

            foreach (var template in Model)
            {
                foreach (var field in template.Fields)
                {
                    if (!field.Modified)
                    {
                        continue;
                    }

                    output.WriteStartElement(@"field");
                    output.WriteAttributeString(@"id", field.TemplateFieldId.ToString());
                    output.WriteAttributeString(@"sortorder", field.SortOrder.ToString());
                    output.WriteAttributeString(@"sectionsortorder", field.SectionSortOrder.ToString());
                    output.WriteEndElement();

                    field.Modified = false;
                }
            }

            output.WriteEndElement();

            ExecuteCompleted completed = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                Modified = false;

                var fieldId = IdManager.GetFieldId("/sitecore/templates/System/Templates/Sections/Appearance/Appearance/__Sortorder");
                foreach (var template in Model)
                {
                    foreach (var field in template.Fields)
                    {
                        var itemVersionUri = new ItemVersionUri(new ItemUri(template.TemplateUri.DatabaseUri, field.TemplateFieldId), LanguageManager.CurrentLanguage, Data.Version.Latest);
                        var fieldUri = new FieldUri(itemVersionUri, fieldId);

                        Notifications.RaiseFieldChanged(this, fieldUri, field.SortOrder.ToString());

                        itemVersionUri = new ItemVersionUri(new ItemUri(template.TemplateUri.DatabaseUri, new ItemId(new Guid(field.SectionId))), LanguageManager.CurrentLanguage, Data.Version.Latest);
                        fieldUri = new FieldUri(itemVersionUri, fieldId);

                        Notifications.RaiseFieldChanged(this, fieldUri, field.SortOrder.ToString());
                    }
                }
            };

            TemplateUri.Site.DataService.ExecuteAsync("Templates.SaveTemplateFieldSorterFields", completed, TemplateUri.DatabaseName.ToString(), writer.ToString());
        }

        public void SetSectionSortOrder([NotNull] TemplateSection templateSection, [NotNull] string sectionId, int value)
        {
            Assert.ArgumentNotNull(templateSection, nameof(templateSection));
            Assert.ArgumentNotNull(sectionId, nameof(sectionId));

            Modified = true;

            foreach (var templateFieldse in Model)
            {
                foreach (var field in templateFieldse.Fields)
                {
                    if (field.SectionId != sectionId)
                    {
                        continue;
                    }

                    field.SectionSortOrder = value;

                    if (IsModifiedTracking == 0)
                    {
                        field.Modified = true;
                    }
                }
            }

            foreach (var s in Sections)
            {
                if (s != templateSection && s.SectionId == sectionId)
                {
                    s.SectionSortOrder.Text = value.ToString();
                }
            }
        }

        public void SetSortOrder([NotNull] Field sourceField, [NotNull] ItemId templateFieldId, int value)
        {
            Assert.ArgumentNotNull(sourceField, nameof(sourceField));
            Assert.ArgumentNotNull(templateFieldId, nameof(templateFieldId));

            Modified = true;

            foreach (var fields in Model)
            {
                foreach (var field in fields.Fields)
                {
                    if (field.TemplateFieldId != templateFieldId)
                    {
                        continue;
                    }

                    field.SortOrder = value;

                    if (IsModifiedTracking == 0)
                    {
                        field.Modified = true;
                    }

                    if (field != sourceField && field.Control != null)
                    {
                        field.Control.SortOrder.Text = value.ToString();
                    }
                }
            }
        }

        [CanBeNull]
        private Field GetNextField([NotNull] string sectionName)
        {
            Debug.ArgumentNotNull(sectionName, nameof(sectionName));

            Field result = null;

            foreach (var template in Model)
            {
                if (template.Current >= template.VisibleFields.Count)
                {
                    continue;
                }

                var field = template.VisibleFields[template.Current];
                if (field.SectionName != sectionName)
                {
                    continue;
                }

                if (result == null)
                {
                    result = field;
                    continue;
                }

                if (field.SortOrder < result.SortOrder)
                {
                    result = field;
                }
            }

            if (result != null)
            {
                return result;
            }

            foreach (var template in Model)
            {
                if (template.Current >= template.VisibleFields.Count)
                {
                    continue;
                }

                var field = template.VisibleFields[template.Current];

                if (result == null)
                {
                    result = field;
                    continue;
                }

                if (field.SortOrder < result.SortOrder)
                {
                    result = field;
                }
            }

            return result;
        }

        private void LoadFields([NotNull] TemplateFields templateFields, [NotNull] ItemUri templateUri)
        {
            Debug.ArgumentNotNull(templateFields, nameof(templateFields));
            Debug.ArgumentNotNull(templateUri, nameof(templateUri));

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                templateFields.Name = root.GetAttributeValue("name");
                templateFields.Icon = new Icon(templateUri.Site, root.GetAttributeValue("icon"));

                if (templateFields.IsPrimary)
                {
                    UpdateQuickInfo(templateFields, root);
                }

                foreach (var element in root.Elements())
                {
                    var name = element.GetAttributeValue("name");

                    var field = new Field
                    {
                        TemplateFieldId = new ItemId(new Guid(element.GetAttributeValue("id"))),
                        Name = name,
                        Type = element.GetAttributeValue("type"),
                        SortOrder = element.GetAttributeInt("sortorder", 0),
                        SectionSortOrder = element.GetAttributeInt("sectionsortorder", 0),
                        SectionName = element.GetAttributeValue("sectionname"),
                        SectionId = element.GetAttributeValue("sectionid"),
                        TemplateName = element.GetAttributeValue("templatename"),
                        TemplateIcon = new Icon(templateUri.Site, element.GetAttributeValue("templateicon")),
                        IsInherited = element.GetAttributeValue("isinherited") == @"true",
                        IsSystemField = name.StartsWith(@"__"),
                    };

                    foreach (var tf in Model)
                    {
                        if (tf == templateFields)
                        {
                            continue;
                        }

                        foreach (var f in tf.Fields)
                        {
                            if (f.TemplateFieldId != field.TemplateFieldId)
                            {
                                continue;
                            }

                            field.SortOrder = f.SortOrder;
                            field.SectionSortOrder = f.SectionSortOrder;
                            break;
                        }
                    }

                    templateFields.Fields.Add(field);
                }

                Refresh();
            };

            templateUri.Site.DataService.ExecuteAsync("Templates.GetTemplateFieldSorterFields", c, templateFields.TemplateUri.DatabaseName.Name, templateFields.TemplateUri.ItemId.ToString());
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = AppHost.ContextMenus.Build(GetContext(), e);
        }

        private void OpenInstances(object sender, EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var templateUri = TemplateUri;
            if (templateUri == null)
            {
                return;
            }

            var linkViewer = AppHost.Windows.Factory.OpenLinkViewer();
            if (linkViewer == null)
            {
                return;
            }

            var linkTab = linkViewer.CreateTab(templateUri);
            linkTab.Initialize(templateUri);
        }

        private void RenderFields([NotNull] Field nextField)
        {
            Debug.ArgumentNotNull(nextField, nameof(nextField));

            foreach (var template in Model)
            {
                UserControl control;

                if (template.Current >= template.VisibleFields.Count)
                {
                    control = new EmptyLine();
                }
                else
                {
                    var field = template.VisibleFields[template.Current];

                    if (field.SortOrder == nextField.SortOrder && field.TemplateFieldId == nextField.TemplateFieldId)
                    {
                        var templateField = new TemplateField();
                        templateField.Initialize(this, field);
                        field.Control = templateField;
                        template.Current++;
                        control = templateField;
                    }
                    else
                    {
                        control = new EmptyLine();
                    }
                }

                template.StackPanel.Children.Add(control);
            }
        }

        private void RenderInstances([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            DataService.HandleExecute(response, executeresult);

            int count;
            int.TryParse(response, out count);

            InstancesTextBlock.Value = count == 1 ? Rocks.Resources.QuickInfo_RenderInstances__1_instance : string.Format(Rocks.Resources.QuickInfo_RenderInstances__0__instances, count);
        }

        private void RenderSections([NotNull] Field nextField)
        {
            Debug.ArgumentNotNull(nextField, nameof(nextField));

            var newSection = false;

            foreach (var template in Model)
            {
                if (template.Current >= template.VisibleFields.Count)
                {
                    continue;
                }

                var field = template.VisibleFields[template.Current];

                if (field.SortOrder == nextField.SortOrder && field.SectionName != template.CurrentSectionName)
                {
                    newSection = true;
                    break;
                }
            }

            if (!newSection)
            {
                return;
            }

            foreach (var template in Model)
            {
                UserControl control;

                if (template.Current >= template.VisibleFields.Count)
                {
                    control = new EmptyTemplateSection();
                }
                else
                {
                    var field = template.VisibleFields[template.Current];

                    if (field.SortOrder == nextField.SortOrder && field.SectionName != template.CurrentSectionName)
                    {
                        var templateSection = new TemplateSection();
                        templateSection.Initialize(this, template, field.SectionName, field.SectionSortOrder, field.SectionId);
                        template.CurrentSectionName = field.SectionName;

                        Sections.Add(templateSection);

                        control = templateSection;
                    }
                    else
                    {
                        control = new EmptyTemplateSection();
                    }
                }

                template.StackPanel.Children.Add(control);
            }
        }

        private void UpdateQuickInfo([NotNull] TemplateFields templateFields, [NotNull] XElement root)
        {
            ItemName.Text = templateFields.Name;
            QuickInfoIcon.Source = templateFields.Icon.GetSource();
            ItemIdTextBlock.Value = TemplateUri.ItemId.ToString();
            PathTextBlock.Value = root.GetAttributeValue("path");
        }

        public class Field
        {
            public TemplateField Control { get; set; }

            public bool IsInherited { get; set; }

            public bool IsSystemField { get; set; }

            public bool Modified { get; set; }

            public string Name { get; set; }

            public string SectionId { get; set; }

            public string SectionName { get; set; }

            public int SectionSortOrder { get; set; }

            public int SortOrder { get; set; }

            public ItemId TemplateFieldId { get; set; }

            [NotNull]
            public Icon TemplateIcon { get; set; }

            public string TemplateName { get; set; }

            public string Type { get; set; }
        }

        public class TemplateFields : IComparer<Field>
        {
            public TemplateFields([NotNull] ItemUri templateUri, bool isPrimary)
            {
                Assert.ArgumentNotNull(templateUri, nameof(templateUri));

                Fields = new List<Field>();
                TemplateUri = templateUri;
                IsPrimary = isPrimary;
            }

            public int Current { get; set; }

            public string CurrentSectionName { get; set; }

            public List<Field> Fields { get; set; }

            public Icon Icon { get; set; }

            public bool IsPrimary { get; }

            public string Name { get; set; }

            public StackPanel StackPanel { get; set; }

            public ItemUri TemplateUri { get; set; }

            public List<Field> VisibleFields { get; set; }

            public void SortFields()
            {
                Fields.Sort(this);
            }

            int IComparer<Field>.Compare([NotNull] Field field1, [NotNull] Field field2)
            {
                Assert.ArgumentNotNull(field1, nameof(field1));
                Assert.ArgumentNotNull(field2, nameof(field2));

                var comparison = CompareSections(field1, field2);
                if (comparison != 0)
                {
                    return comparison;
                }

                return CompareFields(field1, field2);
            }

            private int CompareFields([NotNull] Field field1, [NotNull] Field field2)
            {
                Assert.ArgumentNotNull(field1, nameof(field1));
                Assert.ArgumentNotNull(field2, nameof(field2));

                var sort1 = field1.SortOrder;
                var sort2 = field2.SortOrder;

                if (sort1 != sort2)
                {
                    return sort1 - sort2;
                }

                var name1 = field1.Name;
                var name2 = field2.Name;

                if (name1.Length > 0 && name2.Length > 0)
                {
                    if (name1[0] == '_' && name2[0] != '_')
                    {
                        return 1;
                    }

                    if (name2[0] == '_' && name1[0] != '_')
                    {
                        return -1;
                    }
                }

                return string.Compare(name1, name2, StringComparison.InvariantCultureIgnoreCase);
            }

            private int CompareSections([NotNull] Field field1, [NotNull] Field field2)
            {
                Assert.ArgumentNotNull(field1, nameof(field1));
                Assert.ArgumentNotNull(field2, nameof(field2));

                var sort1 = field1.SectionSortOrder;
                var sort2 = field2.SectionSortOrder;

                if (sort1 != sort2)
                {
                    return sort1 - sort2;
                }

                var name1 = field1.SectionName;
                var name2 = field2.SectionName;

                if (name1.Length > 0 && name2.Length > 0)
                {
                    if (name1[0] == '_' && name2[0] != '_')
                    {
                        return 1;
                    }

                    if (name2[0] == '_' && name1[0] != '_')
                    {
                        return -1;
                    }
                }

                return string.Compare(name1, name2, StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}
