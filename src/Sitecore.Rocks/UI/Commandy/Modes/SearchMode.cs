// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class SearchMode : SearchBasedMode
    {
        public SearchMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Search";
            Alias = "s";
        }

        public override string Watermark
        {
            get { return "Text"; }
        }

        protected override void Execute(ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            var contentTree = AppHost.CurrentContentTree;
            if (contentTree != null)
            {
                contentTree.Locate(itemHeader.ItemUri);
            }
        }
    }
}
