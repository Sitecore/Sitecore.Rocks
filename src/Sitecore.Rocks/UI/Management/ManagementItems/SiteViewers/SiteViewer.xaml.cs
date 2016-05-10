// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.Management.ManagementItems.SiteViewers
{
    [Management(ItemName, 1000)]
    public partial class SiteViewer : IManagementItem, IContextProvider
    {
        public const string ItemName = "Sites";

        private readonly ListViewSorter listViewSorter;

        private readonly List<SiteDefinition> sites = new List<SiteDefinition>();

        public SiteViewer()
        {
            InitializeComponent();

            listViewSorter = new ListViewSorter(SiteList);
            Loaded += ControlLoaded;
        }

        public SiteManagementContext Context { get; set; }

        [NotNull]
        public IEnumerable<SiteDefinition> Sites
        {
            get { return sites; }
        }

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
            return new SiteViewerContext(this);
        }

        public UIElement GetControl(IManagementContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            Context = (SiteManagementContext)context;

            return this;
        }

        public void Refresh()
        {
            LoadSites();
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadSites();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void LoadSites()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(SiteList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var element = response.ToXElement();
                if (element == null)
                {
                    return;
                }

                var sitesElement = element.Element("sites");
                if (sitesElement == null)
                {
                    return;
                }

                ParseSites(sitesElement);
                RenderSites();
            };

            Properties.SelectedObject = null;
            Loading.ShowLoading(SiteList);
            Context.Site.DataService.ExecuteAsync("ContentTrees.GetWebConfig", callback);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseSites([NotNull] XElement sitesElement)
        {
            Debug.ArgumentNotNull(sitesElement, nameof(sitesElement));

            sites.Clear();
            var index = 1;

            foreach (var element in sitesElement.Elements().OrderBy(e => e.Name.ToString()))
            {
                var site = new SiteDefinition();
                site.Parse(element);

                site.Index = index;
                index++;

                sites.Add(site);
            }
        }

        private void RenderSites()
        {
            SiteList.ItemsSource = null;
            SiteList.ItemsSource = sites;
            listViewSorter.Resort();

            ResizeGridViewColumn(NameColumn);
            ResizeGridViewColumn(HostNameColumn);
            ResizeGridViewColumn(DatabaseColumn);
            ResizeGridViewColumn(DomainColumn);

            if (sites.Count > 0)
            {
                SiteList.SelectedIndex = 0;
            }
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

        private void SiteChanged([NotNull] object sender, [NotNull] SelectionChangedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var selectedItem = SiteList.SelectedItem;
            if (selectedItem == null)
            {
                Properties.SelectedObject = null;
                return;
            }

            Properties.SelectedObject = selectedItem;
            Properties.IsReadOnly = true;
        }
    }
}
