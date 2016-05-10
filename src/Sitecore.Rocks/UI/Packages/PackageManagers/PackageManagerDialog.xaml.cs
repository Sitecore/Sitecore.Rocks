// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Packages.PackageManagers.Sources;
using Sitecore.Rocks.UI.Packages.PackageManagers.Sources.Repositories;
using Sitecore.Rocks.UI.Packages.PackageManagers.Sources.SitePackages;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.UI.Packages.PackageManagers
{
    public partial class PackageManagerDialog
    {
        public PackageManagerDialog()
        {
            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        [NotNull]
        protected Site Site { get; set; }

        public void GoToSitePackages()
        {
            var item = Sources.Items[0] as TreeViewItem;
            if (item == null)
            {
                return;
            }

            item = item.Items[0] as TreeViewItem;
            if (item == null)
            {
                return;
            }

            item.IsSelected = true;
        }

        public void Initialize([NotNull] Site site)
        {
            Assert.ArgumentNotNull(site, nameof(site));

            Site = site;
        }

        public void RefreshSiteRepositories()
        {
            RefreshSiteRepositories(Sources.Items);
        }

        private void AddRepositories([NotNull] List<PackageSourceManager.PackageSourceDescriptor> sources)
        {
            Debug.ArgumentNotNull(sources, nameof(sources));

            var repositoryList = RepositoryManager.GetRepository(RepositoryManager.Packages);

            var index = 0;
            foreach (var entry in repositoryList.Entries)
            {
                var attribute = new PackageSourceAttribute("Local Repositories", entry.Name, 2000 + index);

                var source = new RepositorySource(entry);

                var descriptor = new PackageSourceManager.PackageSourceDescriptor(source, attribute);

                sources.Add(descriptor);

                index++;
            }
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderSources();
        }

        private void EditRepositories([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var repository = RepositoryManager.GetRepository(RepositoryManager.Packages);

            if (repository.Edit(Rocks.Resources.PackageManagerDialog_EditRepositories_Package_Repositories))
            {
                RenderSources();
            }
        }

        private void RefreshSiteRepositories([NotNull] ItemCollection items)
        {
            Debug.ArgumentNotNull(items, nameof(items));

            foreach (var item in items)
            {
                var treeViewItem = item as TreeViewItem;
                if (treeViewItem == null)
                {
                    continue;
                }

                RefreshSiteRepositories(treeViewItem.Items);

                var source = treeViewItem.Tag as SitePackagesSource;
                if (source != null)
                {
                    source.Refresh();
                }
            }
        }

        private void RenderSources()
        {
            Sources.Items.Clear();

            string sectionName = null;
            TreeViewItem treeViewItem = null;

            TreeViewItem selectedItem = null;

            var sources = PackageSourceManager.Sources.ToList();

            AddRepositories(sources);

            foreach (var descriptor in sources.OrderBy(s => s.Attribute.Priority).ThenBy(s => s.Attribute.SectionName).ThenBy(s => s.Attribute.Name))
            {
                if (sectionName != descriptor.Attribute.SectionName)
                {
                    sectionName = descriptor.Attribute.SectionName;

                    var title = sectionName;
                    if (title == @"<Site>")
                    {
                        title = Site.Name;
                    }

                    treeViewItem = new TreeViewItem
                    {
                        Header = title,
                        IsExpanded = true,
                        Margin = new Thickness(0, 8, 0, 0),
                        FontWeight = FontWeights.Bold
                    };

                    Sources.Items.Add(treeViewItem);
                }

                descriptor.Source.ClearControl();

                var item = new TreeViewItem
                {
                    Header = descriptor.Attribute.Name,
                    Tag = descriptor.Source,
                    FontWeight = FontWeights.Normal
                };

                if (treeViewItem != null)
                {
                    treeViewItem.Items.Add(item);
                }

                if (selectedItem == null)
                {
                    selectedItem = item;
                }
            }

            if (selectedItem != null)
            {
                selectedItem.IsSelected = true;
            }
        }

        private void SetSource([NotNull] object sender, [NotNull] RoutedPropertyChangedEventArgs<object> e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var treeViewItem = Sources.SelectedItem as TreeViewItem;
            if (treeViewItem == null)
            {
                return;
            }

            var packageSource = treeViewItem.Tag as IPackageSource;
            if (packageSource == null)
            {
                var firstChild = treeViewItem.Items.OfType<TreeViewItem>().FirstOrDefault();
                if (firstChild == null)
                {
                    return;
                }

                packageSource = firstChild.Tag as IPackageSource;
                if (packageSource == null)
                {
                    return;
                }

                firstChild.IsSelected = true;
                return;
            }

            var control = packageSource.GetControl(Site);
            Source.Child = control;
        }
    }
}
