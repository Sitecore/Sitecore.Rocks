// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Server.Extensibility;

namespace Sitecore
{
    public static class ServerHost
    {
        private static EnvHost env;

        private static ExtensibilityHost extensibility;

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
