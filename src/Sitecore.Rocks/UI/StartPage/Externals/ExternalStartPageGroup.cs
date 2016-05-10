// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.StartPage.Externals
{
    public class ExternalStartPageGroup : StartPageGroupBase
    {
        public ExternalStartPageGroup([NotNull] StartPageViewer startPage, [NotNull] string parentName) : base(startPage, parentName)
        {
            Assert.ArgumentNotNull(startPage, nameof(startPage));
            Assert.ArgumentNotNull(parentName, nameof(parentName));
        }
    }
}
