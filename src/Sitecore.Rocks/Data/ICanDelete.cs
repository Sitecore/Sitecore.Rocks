// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Data
{
    public interface ICanDelete
    {
        [NotNull]
        string Text { get; }

        void Delete(bool deleteFiles);
    }
}
