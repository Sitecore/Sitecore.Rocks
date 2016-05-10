// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.LayoutDesigners.LayoutTreeViews.Models;

namespace Sitecore.Rocks.UI.LayoutDesigners.Designers
{
    public class PageContext : ICommandContext
    {
        public PageContext([NotNull] PageModel pageModel)
        {
            Assert.ArgumentNotNull(pageModel, nameof(pageModel));

            PageModel = pageModel;
        }

        [NotNull]
        public PageModel PageModel { get; private set; }
    }
}
