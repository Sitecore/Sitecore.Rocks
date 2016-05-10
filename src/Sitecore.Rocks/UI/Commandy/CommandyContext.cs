// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy
{
    public class CommandyContext
    {
        public CommandyContext([NotNull] Commandy commandy, [NotNull] IMode mode, [NotNull] string text)
        {
            Assert.ArgumentNotNull(commandy, nameof(commandy));
            Assert.ArgumentNotNull(mode, nameof(mode));
            Assert.ArgumentNotNull(text, nameof(text));

            Commandy = commandy;
            Mode = mode;
            Text = text;
        }

        [NotNull]
        public Commandy Commandy { get; private set; }

        [NotNull]
        public IMode Mode { get; private set; }

        [NotNull]
        public string Text { get; private set; }
    }
}
