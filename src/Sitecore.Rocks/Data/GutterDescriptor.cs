// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data
{
    public class GutterDescriptor
    {
        public GutterDescriptor([NotNull] Icon icon, [NotNull] string toolTip)
        {
            Assert.ArgumentNotNull(icon, nameof(icon));
            Assert.ArgumentNotNull(toolTip, nameof(toolTip));

            Icon = icon;
            ToolTip = toolTip;
        }

        [NotNull]
        public Icon Icon { get; private set; }

        [NotNull]
        public string ToolTip { get; private set; }
    }
}
