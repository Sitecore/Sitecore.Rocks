// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Gutters;

namespace Sitecore.Rocks.Server.Requests
{
    public class SetGutter
    {
        [NotNull]
        public string Execute([NotNull] string id)
        {
            Assert.ArgumentNotNull(id, nameof(id));

            GutterManager.ToggleActiveRendererID(new ID(id));

            return string.Empty;
        }
    }
}
