// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.DebugTraces
{
    public class DeleteSessions
    {
        [NotNull]
        public string Execute()
        {
            var folder = FileUtil.MapPath("/temp/diagnostics");

            foreach (var file in Directory.GetFiles(folder, "trace_*.xml"))
            {
                File.Delete(file);
            }

            foreach (var file in Directory.GetFiles(folder, "profile_*.xml"))
            {
                File.Delete(file);
            }

            return string.Empty;
        }
    }
}
