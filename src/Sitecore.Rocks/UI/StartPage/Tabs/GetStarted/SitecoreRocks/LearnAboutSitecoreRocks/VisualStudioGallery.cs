// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Commands;
using Sitecore.Rocks.UI.StartPage.Commands;

namespace Sitecore.Rocks.UI.StartPage.Tabs.GetStarted.SitecoreRocks.LearnAboutSitecoreRocks
{
    [Command, StartPageCommand("Read: Sitecore Rocks on Visual Studio Gallery", StartPageLearnAboutSitecoreRocksGroup.Name, 3000)]
    public class VisualStudioGallery : StartPageCommandBase
    {
        protected override void Execute()
        {
            AppHost.Browsers.Navigate(@"http://visualstudiogallery.msdn.microsoft.com/en-us/44a26c88-83a7-46f6-903c-5c59bcd3d35b");
        }
    }
}
