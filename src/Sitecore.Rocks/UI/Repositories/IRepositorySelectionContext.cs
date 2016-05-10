// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.UI.Repositories
{
    public interface IRepositorySelectionContext
    {
        [NotNull]
        IEnumerable<string> FileNames { get; }

        [NotNull]
        string RepositoryName { get; }

        [NotNull]
        IRepositoryPanel RepositoryPanel { get; }
    }
}
