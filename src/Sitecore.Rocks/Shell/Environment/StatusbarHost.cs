// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Shell.Environment
{
    public class StatusbarHost
    {
        public virtual void SetText([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            throw new InvalidOperationException("Statusbar host must be overridden");
        }
    }
}
