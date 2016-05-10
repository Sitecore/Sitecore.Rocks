// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Externals
{
    public class ExternaStartPagelCommand : StartPageCommandBase
    {
        public ExternaStartPagelCommand([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            Url = url;
        }

        [NotNull]
        public string Url { get; }

        protected override void Execute()
        {
            AppHost.Browsers.Navigate(Url);
        }
    }
}
