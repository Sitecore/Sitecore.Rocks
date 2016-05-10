// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Projects;

namespace Sitecore.Rocks.Shell.ProjectOptions
{
    public interface IPropertyPage
    {
        [NotNull]
        string Header { get; }

        void LoadProject([CanBeNull] ProjectBase project, [NotNull] object visualStudioProject);
    }
}
