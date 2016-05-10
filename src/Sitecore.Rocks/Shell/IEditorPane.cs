// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Rocks.Shell
{
    public interface IEditorPane : IPane
    {
        void Close();

        void SetModifiedFlag(bool flag);
    }
}
