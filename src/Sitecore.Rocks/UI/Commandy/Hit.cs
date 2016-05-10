// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Commandy
{
    public class Hit
    {
        public Hit([NotNull] string text, [CanBeNull] object tag = null, int rank = 0)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            Text = text;
            Tag = tag;
            Rank = rank;
        }

        public int Rank { get; private set; }

        [CanBeNull]
        public object Tag { get; private set; }

        [NotNull]
        public string Text { get; private set; }
    }
}
