// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ContextMenuExtensions;
using Sitecore.Rocks.UI.TemplateDesigner.Commands;

namespace Sitecore.Rocks.UI.TemplateDesigner
{
    public partial class TemplateSection
    {
        public TemplateSection()
        {
            InitializeComponent();
        }

        [NotNull]
        public TemplateDesigner.TemplateSection Section { get; set; }

        [NotNull]
        protected TemplateDesigner TemplateDesigner { get; set; }

        public void Commit()
        {
            Section.Name = SectionName.Text;
        }

        public void CreateEmpyField()
        {
            var field = new TemplateDesigner.TemplateField();

            var fieldControl = new TemplateField();
            fieldControl.Initialize(TemplateDesigner, field);

            field.Id = Guid.NewGuid().ToString(@"B").ToUpperInvariant();
            field.Control = fieldControl;
            field.Section = Section;

            Section.Fields.Add(field);

            Section.Control.FieldStack.Children.Add(fieldControl);
        }

        public void Initialize([NotNull] TemplateDesigner templateDesigner, [NotNull] TemplateDesigner.TemplateSection section)
        {
            Assert.ArgumentNotNull(templateDesigner, nameof(templateDesigner));
            Assert.ArgumentNotNull(section, nameof(section));

            TemplateDesigner = templateDesigner;
            Section = section;

            Update();
        }

        public bool IsLastSection()
        {
            var index = TemplateDesigner.Stack.Children.IndexOf(this);

            return index == TemplateDesigner.Stack.Children.Count - 1;
        }

        private void HandleGotFocus(object sender, RoutedEventArgs e)
        {
            TemplateDesigner.HandleGotFocus(this);
        }

        private void HandleLostFocus(object sender, RoutedEventArgs e)
        {
            TemplateDesigner.HandleLostFocus(this);
        }

        private void HandleMouseLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TemplateDesigner.HandleMouseLeftButtonDown(this, e);
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            TemplateDesigner.HandleMouseMove(this, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            ContextMenu = null;

            var context = new TemplateDesignerContext
            {
                TemplateDesigner = TemplateDesigner,
                Section = Section
            };

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

        private void SectionNameChanged([NotNull] object sender, [NotNull] TextChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (TemplateDesigner.IsModifiedTracking > 0)
            {
                return;
            }

            TemplateDesigner.SetModified(true);

            if (string.IsNullOrEmpty(SectionName.Text))
            {
                return;
            }

            if (!IsLastSection())
            {
                return;
            }

            var addField = new AddSection();

            var context = new TemplateDesignerContext
            {
                TemplateDesigner = TemplateDesigner,
                Section = Section
            };

            AppHost.Usage.ReportCommand(addField, context);
            addField.Execute(context);

            if (FieldStack.Children.Count == 0)
            {
                CreateEmpyField();
            }
        }

        private void Update()
        {
            SectionName.Text = Section.Name;
        }
    }
}
