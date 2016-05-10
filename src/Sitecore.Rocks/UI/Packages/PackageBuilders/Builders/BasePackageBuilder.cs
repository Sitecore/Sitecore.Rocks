// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Packages.PackageBuilders.Builders
{
    public abstract class BasePackageBuilder : IPackageBuilder
    {
        public string Author { get; set; }

        public string Comment { get; set; }

        public string FileName { get; set; }

        public IEnumerable<PackageFile> Files { get; set; }

        public IEnumerable<PackageItem> Items { get; set; }

        public string License { get; set; }

        public string Name { get; protected set; }

        public string PackageName { get; set; }

        public string Publisher { get; set; }

        public string Readme { get; set; }

        public Site Site { get; set; }

        public string TargetFileFolder { get; set; }

        public string Version { get; set; }

        [NotNull, Localizable(false)]
        protected string RequestTypeName { get; set; }

        public virtual void Build(Action<string> completed)
        {
            Assert.ArgumentNotNull(completed, nameof(completed));

            var targetFileName = Path.ChangeExtension(FileName, @".zip");

            var items = new StringBuilder();

            foreach (var packageItem in Items)
            {
                if (items.Length > 0)
                {
                    items.Append('|');
                }

                items.Append(packageItem.ItemUri.DatabaseName);
                items.Append(',');
                items.Append(packageItem.ItemUri.ItemId);
            }

            var files = new StringBuilder();
            foreach (var packageFile in Files)
            {
                if (files.Length > 0)
                {
                    files.Append('|');
                }

                var fileName = packageFile.FileUri.FileName.Replace(@"\", @"/");

                files.Append(fileName);
            }

            ExecuteCompleted c = delegate(string response, ExecuteResult result)
            {
                if (!DataService.HandleExecute(response, result))
                {
                    completed(string.Empty);
                    return;
                }

                if (string.IsNullOrEmpty(response))
                {
                    AppHost.MessageBox("Failed to create the package.", "Information", MessageBoxButton.OK, MessageBoxImage.Hand);
                    completed(string.Empty);
                    return;
                }

                ProcessCompleted(response, targetFileName, completed);
            };

            Execute(c, items, files);
        }

        public virtual bool IsValid()
        {
            return true;
        }

        protected virtual void Execute([NotNull] ExecuteCompleted completed, [NotNull] StringBuilder items, [NotNull] StringBuilder files)
        {
            Debug.ArgumentNotNull(files, nameof(files));
            Debug.ArgumentNotNull(items, nameof(items));
            Debug.ArgumentNotNull(completed, nameof(completed));

            Site.DataService.ExecuteAsync(RequestTypeName, completed, items.ToString(), files.ToString(), PackageName, Author, Version, Publisher, License, Comment, Readme);
        }

        protected abstract void ProcessCompleted([NotNull] string url, [NotNull] string targetFileName, [NotNull] Action<string> completed);
    }
}
