// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.IO;

namespace Sitecore.Rocks.Server.Requests.DebugTraces
{
    public class GetSession
    {
        [NotNull]
        public string Execute([NotNull] string sessionName)
        {
            Assert.ArgumentNotNull(sessionName, nameof(sessionName));

            var folder = FileUtil.MapPath("/temp/diagnostics");

            var profileFile = Path.Combine(folder, "profile_" + sessionName + ".xml");
            var traceFile = Path.Combine(folder, "trace_" + sessionName + ".xml");

            var writer = new StringWriter();
            var output = new XmlTextWriter(writer);

            output.WriteStartElement("info");

            if (File.Exists(profileFile))
            {
                var profile = FileUtil.ReadFromFile(profileFile);

                output.WriteStartElement("profile");

                output.WriteRaw(profile);

                output.WriteEndElement();
            }

            if (File.Exists(traceFile))
            {
                var trace = FileUtil.ReadFromFile(traceFile);

                output.WriteStartElement("trace");

                output.WriteRaw(trace);

                output.WriteEndElement();
            }

            output.WriteEndElement();

            return writer.ToString();
        }
    }
}
