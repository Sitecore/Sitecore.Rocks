// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Repositories
{
    public class RepositoryContext : IRepositorySelectionContext
    {
        public RepositoryContext([NotNull] IRepositoryPanel repositoryPanel, [NotNull] string taskListName, [NotNull] IEnumerable<string> fileNames)
        {
            Assert.ArgumentNotNull(repositoryPanel, nameof(repositoryPanel));
            Assert.ArgumentNotNull(taskListName, nameof(taskListName));
            Assert.ArgumentNotNull(fileNames, nameof(fileNames));

            RepositoryPanel = repositoryPanel;
            RepositoryName = taskListName;
            FileNames = fileNames;
        }

        public IEnumerable<string> FileNames { get; }

        public string RepositoryName { get; }

        public IRepositoryPanel RepositoryPanel { get; }
    }
}
