// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.UI.Commandy.Modes
{
    [Export(typeof(IMode))]
    public class EditMode : SearchBasedMode
    {
        public EditMode([NotNull] Commandy commandy) : base(commandy)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));

            Name = "Edit";
            Alias = "e";
        }

        public override string Watermark
        {
            get { return "Text"; }
        }

        protected override void Execute(ItemHeader itemHeader)
        {
            Debug.ArgumentNotNull(itemHeader, nameof(itemHeader));

            AppHost.OpenContentEditor(new ItemVersionUri(itemHeader.ItemUri, LanguageManager.CurrentLanguage, Version.Latest));
        }
    }
}
