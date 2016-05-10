// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Shell
{
    public interface IHasEditorPane
    {
        [NotNull]
        IEditorPane Pane { get; set; }
    }
}
