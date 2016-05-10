// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects.FileItems
{
    public abstract class FileItemHandler : IFileItemHandler
    {
        public abstract bool CanHandle(string fileName);

        [NotNull]
        public virtual string GetItemPath([NotNull] ProjectItem projectItem)
        {
            Assert.ArgumentNotNull(projectItem, nameof(projectItem));

            return string.Empty;
        }

        [NotNull]
        public virtual string GetItemPath([NotNull] string itemPath)
        {
            Assert.ArgumentNotNull(itemPath, nameof(itemPath));

            return string.Empty;
        }

        public abstract ItemId GetRootItemId();

        [Localizable(false)]
        public abstract string GetTemplateName();

        public abstract void Handle(DatabaseName databaseName, ProjectItem projectItem, string itemPath, ProcessedEventHandler callback);

        public abstract void UpdateItemPath(ItemUri itemUri, string path);
    }
}
