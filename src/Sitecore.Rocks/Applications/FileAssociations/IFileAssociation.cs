// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Applications.FileAssociations
{
    public interface IFileAssociation
    {
        void Open([NotNull] string fileName);
    }
}
