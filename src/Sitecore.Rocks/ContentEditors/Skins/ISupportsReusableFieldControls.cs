// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors.Skins
{
    public interface ISupportsReusableFieldControls : ISkin
    {
        void Clear();

        bool RemoveFieldControl([NotNull] Field control);
    }
}
