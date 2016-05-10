// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.CommandViewers
{
    [Management(ItemName, 2500)]
    public partial class CommandViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Commands";

        private readonly List<CommandDescriptor> _commands = new List<CommandDescriptor>();

        private readonly ListViewSorter _listViewSorter;

        [CanBeNull]
        private CollectionView view;

        public CommandViewer()
        {
            InitializeComponent();

            _listViewSorter = new ListViewSorter(CommandsList);

            Loaded += ControlLoaded;
        }

        [NotNull]
        public XElement CommandsElement { get; set; }

        [NotNull]
        public SiteManagementContext Context { get; set; }

        public bool CanExecute(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var site = context as SiteManagementContext;
            if (site == null)
            {
                return false;
            }

            return site.Site.DataService.CanExecuteAsync("ContentTrees.GetWebConfig");
        }

        [NotNull]
        public object GetContext()
        {
            return new CommandViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void Refresh()
        {
            LoadCommands();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadCommands();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (view != null)
            {
                view.Refresh();
            }
        }

        private void LoadCommands()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(CommandsList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var commandsElement = element.Element(@"commands");
                if (commandsElement == null)
                {
                    return;
                }

                CommandsElement = commandsElement;

                ParseCommands(commandsElement);
                RenderCommands();
            };

            Loading.ShowLoading(CommandsList);
            Context.Site.DataService.ExecuteAsync("ContentTrees.GetWebConfig", callback);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseCommands([NotNull] XElement commandsElement)
        {
            Debug.ArgumentNotNull(commandsElement, nameof(commandsElement));

            _commands.Clear();

            foreach (var element in commandsElement.Elements().OrderBy(e => e.Name.ToString()))
            {
                var command = new CommandDescriptor(element, element.GetAttributeValue("name"), element.GetAttributeValue("type"));

                _commands.Add(command);
            }
        }

        private void RenderCommands()
        {
            CommandsList.ItemsSource = _commands;

            _listViewSorter.Resort();

            view = CollectionViewSource.GetDefaultView(_commands) as CollectionView;
            if (view == null)
            {
                return;
            }

            view.Filter = delegate(object o)
            {
                var command = o as CommandDescriptor;
                return command != null && command.Name.IsFilterMatch(Filter.Text);
            };

            view.Refresh();

            ResizeGridViewColumn(NameColumn);
            ResizeGridViewColumn(TypeColumn);
        }

        private void ResizeGridViewColumn([NotNull] GridViewColumn column)
        {
            Debug.ArgumentNotNull(column, nameof(column));

            if (double.IsNaN(column.Width))
            {
                column.Width = column.ActualWidth;
            }

            column.Width = double.NaN;
        }

        public class CommandDescriptor
        {
            public CommandDescriptor([NotNull] XElement element, [NotNull] string name, [NotNull] string type)
            {
                Assert.ArgumentNotNull(element, nameof(element));
                Assert.ArgumentNotNull(name, nameof(name));
                Assert.ArgumentNotNull(type, nameof(type));

                Element = element;
                Name = name;
                Type = type;
            }

            [NotNull]
            public XElement Element { get; private set; }

            [NotNull]
            public string Name { get; private set; }

            [NotNull]
            public string Type { get; private set; }
        }
    }
}
