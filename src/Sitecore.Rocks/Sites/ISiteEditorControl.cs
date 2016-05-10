// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;

namespace Sitecore.Rocks.Sites
{
    public interface ISiteEditorControl
    {
        [NotNull]
        SiteEditor SiteEditor { get; set; }

        void Apply([NotNull] Site site);

        void CopyFrom(ISiteEditorControl control);

        void Display([NotNull] Site site);

        void EnableButtons();

        void Test();

        bool Validate();
    }
}
