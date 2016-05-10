// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.UpdateServerComponents
{
    public class RemoveServerComponentOptions
    {
        public RemoveServerComponentOptions([NotNull] TextWriter output, [NotNull] string webSiteRootFolder)
        {
            Assert.ArgumentNotNull(output, nameof(output));
            Assert.ArgumentNotNull(webSiteRootFolder, nameof(webSiteRootFolder));

            Output = output;
            WebSiteRootFolder = webSiteRootFolder;
        }

        public TextWriter Output { get; private set; }

        public string WebSiteRootFolder { get; private set; }
    }
}
