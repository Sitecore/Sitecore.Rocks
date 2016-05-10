// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Shell;
using Sitecore.Rocks.UI.TemplateDesigner.Commands;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    public partial class TemplateDesigner : ISavable, IContextProvider, ITemplatedItem
    {
        private TemplateDesignerAdorner _adorner;

        private AdornerLayer _adornerLayer;

        private bool _modified;

        private Point _origin;

        public TemplateDesigner()
        {
            InitializeComponent();

            HomeIcon.Source = new Icon("Resources/16x16/home.png").GetSource();

            Sections = new List<TemplateSection>();
            BaseTemplates = new List<ItemId>();

            Loaded += ControlLoaded;

            _origin.X = double.MinValue;
            _origin.Y = double.MinValue;
        }

        [CanBeNull]
        public TemplateField ActiveField { get; set; }

        [CanBeNull]
        public TemplateSection ActiveSection { get; set; }

        [NotNull]
        public List<ItemId> BaseTemplates { get; set; }

        [CanBeNull]
        public IEnumerable<FieldTypeHeader> FieldTypes { get; set; }

        [CanBeNull]
        public IEnumerable<FieldValidationHeader> FieldValidations { get; set; }

        public int IsModifiedTracking { get; private set; }

        [NotNull]
        public IEditorPane Pane { get; set; }

        [NotNull]
        public List<TemplateSection> Sections { get; set; }

        [NotNull]
        public ItemId StandardValueItemId { get; set; }

        [NotNull]
        public string TemplateIcon { get; set; }

        public ItemId TemplateId => TemplateUri.ItemId;

        public string TemplateName { get; set; }

        [NotNull]
        public ItemUri TemplateUri { get; set; }

        protected bool IsControlLoaded { get; set; }

        Icon IItem.Icon => new Icon(TemplateUri.Site, TemplateIcon);

        ItemUri IItemUri.ItemUri => TemplateUri;

        public void DisableModifiedTracking()
        {
            IsModifiedTracking++;
        }

        public void EnableModifiedTracking()
        {
            IsModifiedTracking--;
        }

        [NotNull]
        public object GetContext()
        {
            var context = new TemplateDesignerContext
            {
                TemplateDesigner = this,
                Section = ActiveSection,
                Field = ActiveField
            };

            return context;
        }

        public void HandleGotFocus([NotNull] UI.TemplateDesigner.TemplateSection templateSection)
        {
            ActiveSection = templateSection.Section;
            Ribbon.Update(this);
        }

        public void HandleGotFocus([NotNull] UI.TemplateDesigner.TemplateField templateField)
        {
            ActiveField = templateField.Field;
            Ribbon.Update(this);
        }

        public void HandleLostFocus([NotNull] UI.TemplateDesigner.TemplateSection templateSection)
        {
            ActiveSection = null;
            Ribbon.Update(this);
        }

        public void HandleLostFocus([NotNull] UI.TemplateDesigner.TemplateField templateField)
        {
            ActiveField = null;
            Ribbon.Update(this);
        }

        public void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            DragManager.HandleMouseDown(this, e, out _origin);
        }

        public void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            if (!DragManager.IsDragStart(this, e, ref _origin))
            {
                return;
            }

            var sectionControl = sender as UI.TemplateDesigner.TemplateSection;
            if (sectionControl != null && sectionControl.IsLastSection())
            {
                return;
            }

            var fieldControl = sender as UI.TemplateDesigner.TemplateField;
            if (fieldControl != null && fieldControl.IsLastField())
            {
                return;
            }

            var descriptor = new DragDataDescriptor
            {
                Type = sender is UI.TemplateDesigner.TemplateSection ? @"section" : @"field",
                Data = sender,
                TemplateDesigner = this
            };

            var dragData = new DataObject(@"SitecoreTemplateDesigner", descriptor);

            DragManager.DoDragDrop(this, dragData, DragDropEffects.Move);

            SetAdorner(-1);

            e.Handled = true;
        }

        public void Initialize([NotNull] ItemUri templateUri)
        {
            Assert.ArgumentNotNull(templateUri, nameof(templateUri));

            TemplateUri = templateUri;
            StandardValueItemId = ItemId.Empty;

            Stack.Children.Clear();
            Sections.Clear();
            BaseTemplates.Clear();
            SetModified(false);

            templateUri.Site.DataService.GetTemplateXml(templateUri, false, LoadTemplate);
            templateUri.Site.DataService.ExecuteAsync("Links.GetTemplateInstances", RenderInstances, templateUri.DatabaseName.ToString(), templateUri.ItemId.ToString());
        }

        public void Save()
        {
            foreach (var templateSection in Sections)
            {
                templateSection.Control.Commit();

                foreach (var templateField in templateSection.Fields)
                {
                    templateField.Control.Commit();
                }
            }

            var saveCommand = new Save();
            var context = new Context(this, Sections, TemplateName, TemplateUri, BaseTemplates);
            AppHost.Usage.ReportCommand(saveCommand, context);
            saveCommand.Execute(context);
        }

        public void SetModified(bool value)
        {
            if (IsModifiedTracking > 0)
            {
                return;
            }

            Ribbon.Update(this);

            if (_modified == value)
            {
                return;
            }

            _modified = value;
            Pane.SetModifiedFlag(value);
        }

        public void ShowRibbon()
        {
            var isVisible = AppHost.Settings.GetBool("TemplateDesigner", "ShowRibbon", false);

            Ribbon.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            ShowRibbon();
            Ribbon.FilterElements = element => !element.ContextType.IsAssignableFrom(typeof(IItemSelectionContext));
            Ribbon.Render(GetContext());
            Ribbon.Update(this);

            _adorner = new TemplateDesignerAdorner(this);
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            if (_adornerLayer != null)
            {
                _adornerLayer.Add(_adorner);
            }

            Notifications.RegisterItemEvents(this, deleted: ItemDeleted);
            Notifications.RegisterFieldEvents(this, FieldChanged);
        }

        private void DropTemplateField([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            if (!DataService.HandleExecute(response, executeresult))
            {
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch
            {
                return;
            }

            var field = doc.Root;
            if (field == null)
            {
                return;
            }

            var section = Sections.Last();

            if (section == null)
            {
                section = new TemplateSection();
                Sections.Add(section);
            }

            if (string.IsNullOrEmpty(section.Name))
            {
                section.Name = @"Data";
            }

            if (section.Fields.Any())
            {
                section.Fields.RemoveAt(section.Fields.Count - 1);
            }

            LoadTemplateFields(field, section);

            RenderTemplate();
            SetModified(true);
        }

        private void DropTemplateSection([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            if (!DataService.HandleExecute(response, executeresult))
            {
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(response);
            }
            catch
            {
                return;
            }

            var template = doc.Root;
            if (template == null)
            {
                return;
            }

            if (Sections.Any())
            {
                Sections.RemoveAt(Sections.Count - 1);
            }

            LoadTemplateSections(template);

            RenderTemplate();
            SetModified(true);
        }

        private void FieldChanged([NotNull] object sender, FieldUri fieldUri, [NotNull] string newValue)
        {
            if (fieldUri.ItemVersionUri.ItemUri == TemplateUri && fieldUri.FieldId == FieldIds.StandardValues)
            {
                StandardValueItemId = string.IsNullOrEmpty(newValue) ? ItemId.Empty : new ItemId(new Guid(newValue));
            }
        }

        [CanBeNull]
        private FrameworkElement GetDragSource([NotNull] object source)
        {
            Debug.ArgumentNotNull(source, nameof(source));

            var element = source as FrameworkElement;

            while (element != null)
            {
                if (element is UI.TemplateDesigner.TemplateField)
                {
                    return element;
                }

                if (element is UI.TemplateDesigner.TemplateSection)
                {
                    return element;
                }

                if (element.Parent != null)
                {
                    element = element.Parent as FrameworkElement;
                }
                else
                {
                    element = VisualTreeHelper.GetParent(element) as FrameworkElement;
                }
            }

            return null;
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            if (!e.Data.GetDataPresent(@"SitecoreTemplateDesigner"))
            {
                return;
            }

            var descriptor = e.Data.GetData(@"SitecoreTemplateDesigner") as DragDataDescriptor;
            if (descriptor == null)
            {
                return;
            }

            if (descriptor.TemplateDesigner != this)
            {
                return;
            }

            if (descriptor.Type == @"section")
            {
                HandleDragOverSection(descriptor, e);
            }
            else
            {
                HandleDragOverField(descriptor, e);
            }

            if (e.Effects == DragDropEffects.None)
            {
                SetAdorner(-1);
            }
        }

        private void HandleDragOverField([NotNull] DragDataDescriptor descriptor, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));
            Debug.ArgumentNotNull(e, nameof(e));

            var field = descriptor.Data as UI.TemplateDesigner.TemplateField;
            if (field == null)
            {
                return;
            }

            var y = -1;

            var source = GetDragSource(e.OriginalSource);
            var targetSection = source as UI.TemplateDesigner.TemplateSection;
            var targetField = source as UI.TemplateDesigner.TemplateField;

            if (targetSection != null)
            {
                if (targetSection.IsLastSection())
                {
                    return;
                }

                if (targetSection.Section == field.Field.Section)
                {
                    return;
                }

                var fieldStack = targetSection.FieldStack;
                if (fieldStack.Children.Count == 0)
                {
                    return;
                }

                var element = fieldStack.Children[fieldStack.Children.Count - 1];
                y = (int)element.TranslatePoint(new Point(0, 0), this).Y;

                descriptor.TargetSection = targetSection.Section;
                descriptor.Field = null;
            }
            else if (targetField != null)
            {
                if (targetField == field)
                {
                    return;
                }

                if (targetField.IsLastField())
                {
                    return;
                }

                var position = e.GetPosition(targetField);
                var fy = position.Y < targetField.ActualHeight / 2 ? 0 : targetField.ActualHeight;
                y = (int)targetField.TranslatePoint(new Point(0, fy), this).Y;

                descriptor.TargetSection = targetField.Field.Section;
                descriptor.Field = targetField.Field;
                descriptor.PositionBefore = fy == 0;
            }

            if (y == -1)
            {
                return;
            }

            SetAdorner(y);
            e.Effects = DragDropEffects.Move;
        }

        private void HandleDragOverSection([NotNull] DragDataDescriptor descriptor, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(descriptor, nameof(descriptor));
            Debug.ArgumentNotNull(e, nameof(e));

            var source = GetDragSource(e.OriginalSource);
            var targetSection = source as UI.TemplateDesigner.TemplateSection;
            if (targetSection == null)
            {
                return;
            }

            if (targetSection.IsLastSection())
            {
                return;
            }

            if (targetSection == descriptor.Data)
            {
                return;
            }

            var position = e.GetPosition(targetSection);
            var sy = position.Y < targetSection.ActualHeight / 2 ? 0 : targetSection.ActualHeight;
            var p = targetSection.TranslatePoint(new Point(0, sy), this);

            descriptor.TargetSection = targetSection.Section;
            descriptor.PositionBefore = sy == 0;

            SetAdorner((int)p.Y);
            e.Effects = DragDropEffects.Move;
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            if (!e.Data.GetDataPresent(@"SitecoreTemplateDesigner"))
            {
                return;
            }

            var descriptor = e.Data.GetData(@"SitecoreTemplateDesigner") as DragDataDescriptor;
            if (descriptor == null)
            {
                return;
            }

            if (descriptor.Type == @"section")
            {
                if (descriptor.TargetSection == null)
                {
                    return;
                }

                var sourceSection = descriptor.Data as UI.TemplateDesigner.TemplateSection;
                if (sourceSection == null)
                {
                    return;
                }

                Sections.Remove(sourceSection.Section);
                Stack.Children.Remove(sourceSection);

                var index = Sections.IndexOf(descriptor.TargetSection) + (descriptor.PositionBefore ? 0 : 1);

                Sections.Insert(index, sourceSection.Section);
                Stack.Children.Insert(index, sourceSection);
                SetModified(true);
            }
            else
            {
                if (descriptor.TargetSection == null)
                {
                    return;
                }

                var sourceField = descriptor.Data as UI.TemplateDesigner.TemplateField;
                if (sourceField == null)
                {
                    return;
                }

                sourceField.Field.Section.Fields.Remove(sourceField.Field);
                sourceField.Field.Section.Control.FieldStack.Children.Remove(sourceField);

                int index;

                if (descriptor.Field == null)
                {
                    index = descriptor.TargetSection.Fields.Count - 1;
                }
                else
                {
                    index = descriptor.TargetSection.Fields.IndexOf(descriptor.Field) + (descriptor.PositionBefore ? 0 : 1);
                }

                descriptor.TargetSection.Fields.Insert(index, sourceField.Field);
                descriptor.TargetSection.Control.FieldStack.Children.Insert(index, sourceField);
                sourceField.Field.Section = descriptor.TargetSection;
                SetModified(true);
            }
        }

        private void HandleItemsDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (e.Source != ScrollViewer)
            {
                return;
            }

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var items = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (items == null)
            {
                return;
            }

            if (items.Count() != 1)
            {
                return;
            }

            var item = items.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            var templateSectionId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template section");
            var templateFieldId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template field");

            if (item.TemplateId != templateSectionId && item.TemplateId != templateFieldId)
            {
                return;
            }

            e.Effects = DragDropEffects.Copy;
        }

        private void HandleItemsDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (!e.Data.GetDataPresent(DragManager.DragIdentifier))
            {
                return;
            }

            e.Handled = true;

            var items = e.Data.GetData(DragManager.DragIdentifier) as IEnumerable<IItem>;
            if (items == null)
            {
                return;
            }

            var item = items.First() as ITemplatedItem;
            if (item == null)
            {
                return;
            }

            var templateSectionId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template section");

            if (item.TemplateId == templateSectionId)
            {
                TemplateUri.Site.DataService.ExecuteAsync("Templates.GetSectionXml", DropTemplateSection, item.ItemUri.DatabaseName.Name, item.ItemUri.ItemId.ToString(), false);
            }

            var templateFieldId = IdManager.GetItemId("/sitecore/templates/System/Templates/Template field");

            if (item.TemplateId == templateFieldId)
            {
                TemplateUri.Site.DataService.ExecuteAsync("Templates.GetFieldXml", DropTemplateField, item.ItemUri.DatabaseName.Name, item.ItemUri.ItemId.ToString(), false);
            }
        }

        private void ItemDeleted([NotNull] object sender, [NotNull] ItemUri itemUri)
        {
            if (itemUri.ItemId == StandardValueItemId && itemUri.DatabaseUri == TemplateUri.DatabaseUri)
            {
                StandardValueItemId = ItemId.Empty;
            }
        }

        private void LoadBaseTemplates([NotNull] XElement template)
        {
            Debug.ArgumentNotNull(template, nameof(template));

            BaseTemplates.Clear();

            var baseTemplates = template.GetAttributeValue("basetemplates");

            foreach (var s in baseTemplates.Split('|'))
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                BaseTemplates.Add(new ItemId(new Guid(s)));
            }
        }

        private void LoadFieldTypes()
        {
            TemplateUri.Site.DataService.GetFieldTypes(TemplateUri.DatabaseUri, LoadFieldTypes);
        }

        private void LoadFieldTypes([NotNull] IEnumerable<FieldTypeHeader> fieldTypes, [NotNull] IEnumerable<FieldValidationHeader> fieldValidations)
        {
            Debug.ArgumentNotNull(fieldTypes, nameof(fieldTypes));
            Debug.ArgumentNotNull(fieldValidations, nameof(fieldValidations));

            var t = fieldTypes.ToList();
            var v = fieldValidations.ToList();

            FieldTypes = t;
            FieldValidations = v;

            DisableModifiedTracking();

            foreach (var section in Sections)
            {
                foreach (var field in section.Fields)
                {
                    field.Control.LoadFieldOptions(t, v);
                }
            }

            EnableModifiedTracking();
        }

        private void LoadTemplate([NotNull] XDocument doc)
        {
            Debug.ArgumentNotNull(doc, nameof(doc));

            var template = doc.XPathSelectElement(@"/template");
            if (template == null)
            {
                return;
            }

            TemplateName = template.GetAttributeValue("name");
            TemplateIcon = template.GetAttributeValue("icon");

            var standardValueItemId = template.GetAttributeValue("standardvaluesitemid");
            if (!string.IsNullOrEmpty(standardValueItemId))
            {
                Guid guid;
                if (Guid.TryParse(standardValueItemId, out guid))
                {
                    StandardValueItemId = new ItemId(guid);
                }
            }

            Pane.Caption = string.Format(Rocks.Resources.TemplateDesigner_LoadTemplate__0____Template, TemplateName);

            LoadBaseTemplates(template);

            LoadTemplateSections(template);

            ItemName.Text = TemplateName;
            QuickInfoIcon.Source = new Icon(TemplateUri.Site, Icon.MakePath(TemplateIcon)).GetSource();
            ItemIdTextBlock.Value = TemplateUri.ItemId.ToString();
            PathTextBlock.Value = template.GetAttributeValue("path");

            RenderTemplate();
            LoadFieldTypes();

            Ribbon.Update(this);
        }

        private void LoadTemplateFields([NotNull] XElement sectionElement, [NotNull] TemplateSection templateSection)
        {
            Debug.ArgumentNotNull(sectionElement, nameof(sectionElement));
            Debug.ArgumentNotNull(templateSection, nameof(templateSection));

            foreach (var fieldElement in sectionElement.Elements())
            {
                var templateField = new TemplateField
                {
                    Id = fieldElement.GetAttributeValue("id"),
                    Name = fieldElement.GetAttributeValue("name"),
                    Type = fieldElement.GetAttributeValue("type"),
                    Source = fieldElement.GetAttributeValue("source"),
                    Shared = fieldElement.GetAttributeValue("shared") == @"1",
                    Unversioned = fieldElement.GetAttributeValue("unversioned") == @"1",
                    Validations = fieldElement.GetAttributeValue("validatorbar")
                };

                templateSection.Fields.Add(templateField);
            }
        }

        private void LoadTemplateSections([NotNull] XElement template)
        {
            Debug.ArgumentNotNull(template, nameof(template));

            foreach (var sectionElement in template.Elements())
            {
                var templateSection = new TemplateSection
                {
                    Id = sectionElement.GetAttributeValue("id"),
                    Name = sectionElement.GetAttributeValue("name")
                };

                Sections.Add(templateSection);

                LoadTemplateFields(sectionElement, templateSection);
            }
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = GetContext();

            var commands = Rocks.Commands.CommandManager.GetCommands(context);
            if (!commands.Any())
            {
                e.Handled = true;
                return;
            }

            var contextMenu = new ContextMenu();

            contextMenu.Build(commands, context);

            ContextMenu = contextMenu;
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

        private void RenderInstances([NotNull] string response, [NotNull] ExecuteResult executeresult)
        {
            Debug.ArgumentNotNull(response, nameof(response));
            Debug.ArgumentNotNull(executeresult, nameof(executeresult));

            DataService.HandleExecute(response, executeresult);

            int count;
            int.TryParse(response, out count);

            InstancesTextBlock.Value = count == 1 ? Rocks.Resources.QuickInfo_RenderInstances__1_instance : string.Format(Rocks.Resources.QuickInfo_RenderInstances__0__instances, count);
        }

        private void RenderTemplate()
        {
            Stack.Children.Clear();

            DisableModifiedTracking();

            var hasEmptySection = false;

            foreach (var section in Sections)
            {
                section.Control = new UI.TemplateDesigner.TemplateSection();
                section.Control.Initialize(this, section);

                Stack.Children.Add(section.Control);

                if (string.IsNullOrEmpty(section.Name))
                {
                    hasEmptySection = true;
                }

                var hasEmptyField = false;

                foreach (var field in section.Fields)
                {
                    var fieldControl = new UI.TemplateDesigner.TemplateField();
                    field.Control = fieldControl;
                    field.Section = section;

                    fieldControl.Initialize(this, field);

                    section.Control.FieldStack.Children.Add(fieldControl);

                    if (string.IsNullOrEmpty(field.Name))
                    {
                        hasEmptyField = true;
                    }
                }

                if (!hasEmptyField)
                {
                    section.Control.CreateEmpyField();
                }
            }

            if (!hasEmptySection)
            {
                var newSection = new TemplateSection();
                Sections.Add(newSection);
                newSection.Id = Guid.NewGuid().ToString(@"B").ToUpperInvariant();

                newSection.Control = new UI.TemplateDesigner.TemplateSection();
                newSection.Control.Initialize(this, newSection);

                Stack.Children.Add(newSection.Control);
            }

            EnableModifiedTracking();
        }

        private void SetAdorner(int y)
        {
            _adorner.Y = y;
            _adornerLayer.Update();
        }

        public class Context
        {
            public Context([NotNull] TemplateDesigner templateDesigner, [NotNull] List<TemplateSection> sections, [NotNull] string templateName, [NotNull] ItemUri templateUri, [NotNull] List<ItemId> baseTemplates)
            {
                Assert.ArgumentNotNull(templateDesigner, nameof(templateDesigner));
                Assert.ArgumentNotNull(sections, nameof(sections));
                Assert.ArgumentNotNull(templateName, nameof(templateName));
                Assert.ArgumentNotNull(templateUri, nameof(templateUri));
                Assert.ArgumentNotNull(baseTemplates, nameof(baseTemplates));

                Sections = sections;
                TemplateName = templateName;
                TemplateUri = templateUri;
                TemplateDesigner = templateDesigner;
                BaseTemplates = baseTemplates;
            }

            [NotNull]
            public List<ItemId> BaseTemplates { get; set; }

            [NotNull]
            public List<TemplateSection> Sections { get; set; }

            [NotNull]
            public TemplateDesigner TemplateDesigner { get; set; }

            [NotNull]
            public string TemplateName { get; set; }

            [NotNull]
            public ItemUri TemplateUri { get; set; }
        }

        private class DragDataDescriptor
        {
            public object Data { get; set; }

            public TemplateField Field { get; set; }

            public bool PositionBefore { get; set; }

            public TemplateSection TargetSection { get; set; }

            public TemplateDesigner TemplateDesigner { get; set; }

            public string Type { get; set; }
        }

        public class TemplateField
        {
            public TemplateField()
            {
                Validations = string.Empty;
            }

            public UI.TemplateDesigner.TemplateField Control { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }

            public TemplateSection Section { get; set; }

            public bool Shared { get; set; }

            public string Source { get; set; }

            public string Type { get; set; }

            public bool Unversioned { get; set; }

            [NotNull]
            public string Validations { get; set; }
        }

        public class TemplateSection
        {
            public TemplateSection()
            {
                Fields = new List<TemplateField>();
            }

            public UI.TemplateDesigner.TemplateSection Control { get; set; }

            public List<TemplateField> Fields { get; set; }

            public string Id { get; set; }

            public string Name { get; set; }
        }
    }
}
