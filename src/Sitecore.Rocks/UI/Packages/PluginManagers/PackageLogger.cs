// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using NuGet;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.Packages.PluginManagers
{
    public class PackageLogger : ILogger
    {
        public PackageLogger([NotNull] string outputPane)
        {
            Assert.ArgumentNotNull(outputPane, nameof(outputPane));

            OutputPane = outputPane;
        }

        public bool HasConflict { get; private set; }

        [NotNull]
        public string OutputPane { get; private set; }

        public void Log(MessageLevel level, [NotNull] string message, [NotNull] params object[] args)
        {
            Assert.ArgumentNotNull(message, nameof(message));
            Assert.ArgumentNotNull(args, nameof(args));

            AppHost.Output.Write(string.Format(message, args));
        }

        public FileConflictResolution ResolveFileConflict([NotNull] string message)
        {
            Assert.ArgumentNotNull(message, nameof(message));

            HasConflict = true;

            return FileConflictResolution.Overwrite;
        }
    }
}
