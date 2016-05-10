// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Applications.Storages;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.FrameworkElementExtensions;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.ProjectExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Controls.Dialogs.BrowseTypeName
{
    public partial class TypeNameSelector
    {
        public const string RegistryKey = "TypeNameSelector";

        private readonly ListViewSorter listViewSorter;

        private readonly List<TypeNameDescriptor> types = new List<TypeNameDescriptor>();

        [CanBeNull]
        private CollectionView view;

        public TypeNameSelector()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(Types);

            InitialTypeName = string.Empty;

            Loaded += ControlLoaded;
        }

        [NotNull]
        public string InitialTypeName { get; set; }

        [CanBeNull]
        public TypeNameDescriptor SelectedTypeName
        {
            get { return Types.SelectedItem as TypeNameDescriptor; }
        }

        public event MouseButtonEventHandler DoubleClick;

        public event SelectionChangedEventHandler SelectionChanged;

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            GetTypes(types);
            LoadTypes(types);

            var typeNameDescriptor = types.FirstOrDefault(t => t.FullName == InitialTypeName);
            if (typeNameDescriptor != null)
            {
                Types.SelectedItem = typeNameDescriptor;
                Types.ScrollIntoView(Types.SelectedItem);
            }

            Keyboard.Focus(TemplateSelectorFilter.TextBox);
        }

        private void FilterTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (view != null)
            {
                view.Refresh();
            }
        }

        [CanBeNull]
        private string GetGroupName([NotNull] object sender)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));

            var expander = sender as Expander;
            if (expander == null)
            {
                return null;
            }

            var group = expander.Tag as CollectionViewGroup;
            if (group == null)
            {
                return null;
            }

            return group.Name as string ?? string.Empty;
        }

        private void GetTypes([NotNull] List<TypeNameDescriptor> typeList)
        {
            Debug.ArgumentNotNull(typeList, nameof(typeList));

            typeList.Clear();

            foreach (var project in ProjectManager.Projects)
            {
                var proj = project.GetVisualStudioProject();
                if (proj == null)
                {
                    continue;
                }

                var codeModel = proj.CodeModel;
                if (codeModel == null)
                {
                    continue;
                }

                var codeElements = codeModel.CodeElements;
                if (codeElements == null)
                {
                    continue;
                }

                var outputFileName = Path.GetFileNameWithoutExtension(project.OutputFileName);
                var projectName = project.Name;

                GetTypes(typeList, codeElements, outputFileName, projectName);
            }

            typeList.Sort((t0, t1) => string.Compare(t0.Name, t1.Name, StringComparison.InvariantCultureIgnoreCase));
        }

        private void GetTypes([NotNull] List<TypeNameDescriptor> typeList, [NotNull] CodeElements codeElements, [NotNull] string outputFileName, [NotNull] string projectName)
        {
            Debug.ArgumentNotNull(typeList, nameof(typeList));
            Debug.ArgumentNotNull(codeElements, nameof(codeElements));
            Debug.ArgumentNotNull(outputFileName, nameof(outputFileName));
            Debug.ArgumentNotNull(projectName, nameof(projectName));

            foreach (CodeElement codeElement in codeElements)
            {
                if (codeElement == null)
                {
                    continue;
                }

                var codeNamespace = codeElement as CodeNamespace;
                if (codeNamespace != null)
                {
                    GetTypes(typeList, codeNamespace.Members, outputFileName, projectName);
                    continue;
                }

                if (codeElement.InfoLocation != vsCMInfoLocation.vsCMInfoLocationProject)
                {
                    continue;
                }

                var codeClass = codeElement as CodeClass2;
                if (codeClass != null)
                {
                    var fullName = codeClass.Namespace.Name + "." + codeClass.Name + ", " + outputFileName;
                    typeList.Add(new TypeNameDescriptor(codeClass.Name, codeClass.Namespace.Name, projectName, fullName));
                }

                var codeInterface = codeElement as CodeInterface2;
                if (codeInterface != null)
                {
                    var name = codeInterface.Namespace.Name + "." + codeInterface.Name + ", " + outputFileName;
                    typeList.Add(new TypeNameDescriptor(codeInterface.Name, codeInterface.Namespace.Name, projectName, name));
                }
            }
        }

        private void HandleMouseDoubleClick([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (SelectedTypeName == null)
            {
                return;
            }

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                if (frameworkElement.GetAncestorOrSelf<RepeatButton>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<GridViewColumnHeader>() != null)
                {
                    return;
                }

                if (frameworkElement.GetAncestorOrSelf<Thumb>() != null)
                {
                    return;
                }
            }

            var doubleClick = DoubleClick;
            if (doubleClick != null)
            {
                doubleClick(sender, e);
            }
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void InitExpander([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var expander = sender as Expander;
            if (expander == null)
            {
                return;
            }

            var name = GetGroupName(sender);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!Storage.ReadBool(RegistryKey + "\\Groups", name, true))
            {
                expander.IsExpanded = false;
            }
        }

        private void LoadTypes([NotNull] IEnumerable<TypeNameDescriptor> types)
        {
            Debug.ArgumentNotNull(types, nameof(types));

            Types.ItemsSource = types;

            listViewSorter.Resort();
            view = CollectionViewSource.GetDefaultView(types) as CollectionView;
            if (view == null)
            {
                return;
            }

            var groupDescription = new PropertyGroupDescription("Project")
            {
                StringComparison = StringComparison.InvariantCultureIgnoreCase
            };

            var collection = view.GroupDescriptions;
            if (collection != null)
            {
                collection.Clear();
                collection.Add(groupDescription);
            }

            view.Filter = delegate(object o)
            {
                var typeNameDescriptor = o as TypeNameDescriptor;
                if (typeNameDescriptor == null)
                {
                    return false;
                }

                return typeNameDescriptor.FullName.IsFilterMatch(TemplateSelectorFilter.Text);
            };

            view.Refresh();

            Types.ResizeColumn(NameColumn);

            Loading.Visibility = Visibility.Collapsed;
            Stack.Visibility = Visibility.Visible;
        }

        private void SetCollapsed([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, false);
            }
        }

        private void SetExpanded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            e.Handled = true;

            var name = GetGroupName(sender);
            if (!string.IsNullOrEmpty(name))
            {
                Storage.WriteBool(RegistryKey + "\\Groups", name, true);
            }
        }

        private void TypesSelectionChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var changed = SelectionChanged;
            if (changed != null)
            {
                changed(sender, e);
            }
        }

        public class TypeNameDescriptor
        {
            public TypeNameDescriptor([NotNull] string name, [NotNull] string ns, [NotNull] string project, [NotNull] string fullName)
            {
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(ns, nameof(ns));
                Assert.ArgumentNotNull(project, nameof(project));
                Assert.ArgumentNotNull(fullName, nameof(fullName));

                Name = name;
                Namespace = ns;
                Project = project;
                FullName = fullName;
            }

            [NotNull]
            public string FullName { get; }

            [NotNull]
            public string Name { get; }

            [NotNull]
            public string Namespace { get; private set; }

            [NotNull]
            public string Project { get; private set; }
        }
    }
}
