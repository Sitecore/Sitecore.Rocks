// © 2015 Sitecore Corporation A/S. All rights reserved.

namespace Sitecore.Runners
{
    public class Settings
    {
        public Settings()
        {
            ContextName = "Site";
            DatabaseAndLanguages = "master^en";
            InactiveValidations = string.Empty;
        }

        public string ContextName { get; set; }

        public string DatabaseAndLanguages { get; set; }

        public string InactiveValidations { get; set; }

        public bool ProcessSiteValidations { get; set; }

        public string RootItemPath { get; set; }
    }
}
