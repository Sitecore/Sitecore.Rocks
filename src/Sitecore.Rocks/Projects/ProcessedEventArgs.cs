// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Projects
{
    public class ProcessedEventArgs : EventArgs
    {
        public ProcessedEventArgs([NotNull] string result)
        {
            Assert.ArgumentNotNull(result, nameof(result));

            Text = result;
            Comment = string.Empty;
        }

        public ProcessedEventArgs([NotNull] string text, [NotNull] string comment)
        {
            Assert.ArgumentNotNull(text, nameof(text));
            Assert.ArgumentNotNull(comment, nameof(comment));

            Text = text;
            Comment = comment;
        }

        public string Comment { get; set; }

        public bool Ignore { get; set; }

        public string Text { get; set; }
    }
}
