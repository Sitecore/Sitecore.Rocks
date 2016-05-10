// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentEditors;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;

namespace Sitecore.Rocks.CodeGeneration.DesignSurfaces
{
    public partial class DesignSurface : IContextProvider
    {
        private static readonly List<object> emptyList = new List<object>();

        private readonly List<IShape> selectedItems = new List<IShape>();

        private Point origin;

        public DesignSurface([NotNull] IDesignSurfaceOwner owner, [NotNull] string emptyText)
        {
            Assert.ArgumentNotNull(owner, nameof(owner));
            Assert.ArgumentNotNull(emptyText, nameof(emptyText));

            InitializeComponent();

            Journal = new Journal<string>();
            Owner = owner;
            Empty.Text = emptyText;
        }

        [NotNull]
        public Journal<string> Journal { get; }

        [NotNull]
        public IDesignSurfaceOwner Owner { get; set; }

        [NotNull]
        public IEnumerable<IShape> SelectedItems
        {
            get { return selectedItems; }
        }

        [NotNull]
        public IEnumerable<IShape> Shapes
        {
            get { return Canvas.Children.Cast<IShape>(); }
        }

        protected bool IsDragging { get; set; }

        public void AddSelectedItem([NotNull] IShape shape)
        {
            Assert.ArgumentNotNull(shape, nameof(shape));

            if (shape.IsSelected)
            {
                return;
            }

            shape.IsSelected = true;
            selectedItems.Add(shape);

            TrackSelection(selectedItems);

            RaiseSelectionChanged();
        }

        public void AddToJournal()
        {
            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement(@"journal");

            Owner.SaveState(output);

            output.WriteEndElement();

            var entry = writer.ToString();

            if (entry != Journal.Peek())
            {
                Journal.Push(entry);
            }
        }

        public void Clear()
        {
            Canvas.Children.Clear();

            Empty.Visibility = Visibility.Visible;
            Canvas.Visibility = Visibility.Collapsed;
        }

        public void ClearJournal()
        {
            Journal.Clear();
        }

        public void ClearSelectedItems()
        {
            if (!SelectedItems.Any())
            {
                return;
            }

            foreach (var shape in SelectedItems)
            {
                shape.IsSelected = false;
            }

            selectedItems.Clear();
            TrackSelection(selectedItems);

            RaiseSelectionChanged();
        }

        [NotNull]
        public IShape CreateShape([NotNull] IShapeContent shapeContent)
        {
            Assert.ArgumentNotNull(shapeContent, nameof(shapeContent));

            Empty.Visibility = Visibility.Collapsed;
            Canvas.Visibility = Visibility.Visible;

            var result = new DesignSurfaceShape(shapeContent);
            Canvas.Children.Add(result);

            var header = result.HeaderGrid;

            header.AddHandler(MouseLeftButtonDownEvent, (MouseButtonEventHandler)HandleLeftButtonDown, true);
            header.MouseMove += HandleMouseMove;
            header.MouseLeftButtonUp += HandleLeftButtonUp;

            shapeContent.Initialize(result);

            return result;
        }

        public void Delete([NotNull] IShape shape)
        {
            Assert.ArgumentNotNull(shape, nameof(shape));

            Canvas.Children.Remove(shape as FrameworkElement);
            RemoveSelectedItem(shape);

            if (!Shapes.Any())
            {
                Empty.Visibility = Visibility.Visible;
                Canvas.Visibility = Visibility.Collapsed;
            }
        }

        public object GetContext()
        {
            return Owner.GetContext();
        }

        [NotNull]
        public IShape LoadShape([NotNull] XElement root, [NotNull] IShapeContent shapeContent)
        {
            Assert.ArgumentNotNull(root, nameof(root));
            Assert.ArgumentNotNull(shapeContent, nameof(shapeContent));

            var result = CreateShape(shapeContent);

            result.Load(root);

            return result;
        }

        public void RemoveSelectedItem([NotNull] IShape shape)
        {
            Assert.ArgumentNotNull(shape, nameof(shape));

            shape.IsSelected = false;
            selectedItems.Remove(shape);
            TrackSelection(selectedItems);

            RaiseSelectionChanged();
        }

        public event SelectionChangedEventHandler SelectionChanged;

        public void SetModifiedFlag(bool isModified)
        {
            Owner.SetModifiedFlag(isModified);
        }

        private void HandleDragOver([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
            e.Effects = DragDropEffects.None;

            Owner.DragOver(sender, e);
        }

        private void HandleDrop([NotNull] object sender, [NotNull] DragEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;
            e.Effects = DragDropEffects.None;

            Owner.Drop(sender, e);
        }

        private void HandleLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Mouse.Captured is ButtonBase)
            {
                return;
            }

            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }

            var shape = grid.GetAncestor<IShape>();
            if (shape == null)
            {
                return;
            }

            var frameworkElement = shape as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            if (!shape.IsSelected && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
            {
                ClearSelectedItems();
            }

            AddSelectedItem(shape);

            origin = e.GetPosition(frameworkElement.Parent as Panel); // has to be before CaptureMouse

            if (!grid.CaptureMouse())
            {
                return;
            }

            if (Canvas.Children[Canvas.Children.Count - 1] != frameworkElement)
            {
                Canvas.Children.Remove(frameworkElement);
                Canvas.Children.Add(frameworkElement);
                Owner.SetModifiedFlag(true);
            }

            IsDragging = false;
            e.Handled = true;
        }

        private void HandleLeftButtonUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }

            var shape = grid.GetAncestor<IShape>();
            if (shape == null)
            {
                return;
            }

            var frameworkElement = shape as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            if (grid.IsMouseCaptured)
            {
                grid.ReleaseMouseCapture();
            }

            if (IsDragging)
            {
                Owner.SetModifiedFlag(true);
            }

            e.Handled = true;
        }

        private void HandleMouseMove([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var grid = sender as Grid;
            if (grid == null)
            {
                return;
            }

            if (!grid.IsMouseCaptured)
            {
                return;
            }

            var shape = grid.GetAncestor<IShape>();
            if (shape == null)
            {
                return;
            }

            var frameworkElement = shape as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }

            var currentPosition = e.GetPosition(frameworkElement.Parent as Panel);

            var dx = currentPosition.X - origin.X;
            var dy = currentPosition.Y - origin.Y;

            if (!IsDragging && Math.Abs(dx) < 4 && Math.Abs(dy) < 4)
            {
                return;
            }

            IsDragging = true;

            foreach (var s in SelectedItems)
            {
                var p = s.GetPosition();

                p.Offset(dx, dy);

                s.SetPosition(p);
            }

            origin = currentPosition;
            e.Handled = true;
        }

        private void HandleSurfaceLeftButtonDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                return;
            }

            Canvas.Focus();
            ClearSelectedItems();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var clearSelection = true;
            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var shape = frameworkElement.GetInterfaceAncestorOrSelf<IShape>();

                if (shape != null)
                {
                    if (!shape.IsSelected && (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift)
                    {
                        ClearSelectedItems();
                    }

                    AddSelectedItem(shape);
                    clearSelection = false;
                }
            }

            if (clearSelection)
            {
                ClearSelectedItems();
            }

            var context = GetContext();
            if (context != null)
            {
                ContextMenu = AppHost.ContextMenus.Build(context, e);
            }
        }

        private void RaiseSelectionChanged()
        {
            var handler = SelectionChanged;

            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs(Selector.SelectionChangedEvent, emptyList, emptyList));
            }
        }

        private void TrackSelection([CanBeNull] IEnumerable<object> objects)
        {
            if (objects != null && objects.Any())
            {
                return;
            }

            var configurator = this.GetAncestorOrSelf<CodeGenerationConfigurator>();
            if (configurator != null)
            {
                Shell.TrackSelection.SelectObjects(configurator.Pane, null);
            }
        }
    }
}
