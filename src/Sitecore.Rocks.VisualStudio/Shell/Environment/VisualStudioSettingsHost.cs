// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Applications.Storages;

namespace Sitecore.Rocks.Shell.Environment
{
    public class VisualStudioSettingsHost : SettingsHost
    {
        public VisualStudioSettingsHost()
        {
            Storage = new RegistryStorage(@"Software\\Sitecore\\Sitecore.Rocks.VisualStudio\\");
            Options = SitecorePackage.Instance.Options;
        }
    }
}
