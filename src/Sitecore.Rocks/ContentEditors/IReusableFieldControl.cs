// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.ContentEditors
{
    public interface IReusableFieldControl : IFieldControl
    {
        void UnsetField();
    }
}
