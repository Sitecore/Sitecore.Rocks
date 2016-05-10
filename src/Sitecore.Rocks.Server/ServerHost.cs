// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Server.Extensibility;
using Sitecore.Rocks.Server.Hosts;

namespace Sitecore
{
    public static class ServerHost
    {
        private static AppVersionHost appVersionHost;

        private static EnvHost env;

        private static ExtensibilityHost extensibility;

        [NotNull]
        public static AppVersionHost AppVersion
        {
            get { return appVersionHost ?? (appVersionHost = new AppVersionHost()); }
        }

        [NotNull]
        public static EnvHost Env
        {
            get { return env ?? (env = new EnvHost()); }
        }

        [NotNull]
        public static ExtensibilityHost Extensibility
        {
            get { return extensibility ?? (extensibility = new ExtensibilityHost()); }
        }

        internal static void Internal()
        {
        }
    }
}
