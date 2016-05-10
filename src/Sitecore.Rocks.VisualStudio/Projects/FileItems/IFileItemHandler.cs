// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.Projects.FileItems
{
    public interface IFileItemHandler
    {
        bool CanHandle([NotNull] string fileName);

        [NotNull]
        ItemId GetRootItemId();

        [NotNull]
        string GetTemplateName();

        void Handle([NotNull] DatabaseName databaseName, [NotNull] ProjectItem projectItem, [NotNull] string itemPath, [NotNull] ProcessedEventHandler callback);

        void UpdateItemPath([NotNull] ItemUri itemUri, [NotNull] string path);
    }
}
