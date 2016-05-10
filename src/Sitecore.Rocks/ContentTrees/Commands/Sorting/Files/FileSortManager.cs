// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    public static class FileSortManager
    {
        private static readonly Dictionary<string, IFileSorter> sorters = new Dictionary<string, IFileSorter>();

        [NotNull]
        public static IEnumerable<string> Names
        {
            get { return sorters.Keys; }
        }

        [NotNull]
        public static string GetSorterName([NotNull] FileUri fileUri)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));

            var key = GetKey(fileUri);

            return AppHost.Settings.GetString("ContentTree\\FileSorting", key, string.Empty);
        }

        public static void LoadType([NotNull] Type type, [NotNull] FileSortAttribute fileSortAttribute)
        {
            Assert.ArgumentNotNull(type, nameof(type));
            Assert.ArgumentNotNull(fileSortAttribute, nameof(fileSortAttribute));

            var instance = Activator.CreateInstance(type) as IFileSorter;
            if (instance == null)
            {
                Trace.Write(string.Format("Type {0} does not implement IFileSorter", type.FullName));
                return;
            }

            sorters[fileSortAttribute.Name] = instance;
        }

        public static void SetFileSort([NotNull] FileUri fileUri, [NotNull] string sorterName)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));
            Assert.ArgumentNotNull(sorterName, nameof(sorterName));

            var key = GetKey(fileUri);

            AppHost.Settings.SetString("ContentTree\\FileSorting", key, sorterName);
        }

        public static void Sort([NotNull] FileUri fileUri, [NotNull] List<FileTreeViewItem> items)
        {
            Assert.ArgumentNotNull(fileUri, nameof(fileUri));
            Assert.ArgumentNotNull(items, nameof(items));

            var sorterName = GetSorterName(fileUri);
            if (string.IsNullOrEmpty(sorterName))
            {
                return;
            }

            IFileSorter sorter;
            if (!sorters.TryGetValue(sorterName, out sorter))
            {
                return;
            }

            sorter.Sort(items);
        }

        [NotNull]
        private static string GetKey([NotNull] FileUri fileUri)
        {
            Debug.ArgumentNotNull(fileUri, nameof(fileUri));

            return fileUri.Site.Name + @"/" + fileUri.ToServerPath();
        }
    }
}
