// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml.Linq;
using Sitecore.Diagnostics;
using Sitecore.IO;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.Login
{
    [Pipeline(typeof(LoginPipeline), 3000)]
    public class WebCommands : PipelineProcessor<LoginPipeline>
    {
        protected override void Process(LoginPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Output.WriteStartElement("webcommands");

            GetCommands(pipeline, FileUtil.MapPath("/sitecore"));
            GetCommands(pipeline, FileUtil.MapPath("/sitecore modules"));

            pipeline.Output.WriteEndElement();
        }

        private void GetCommands([NotNull] LoginPipeline pipeline, [NotNull] string folder)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));
            Debug.ArgumentNotNull(folder, nameof(folder));

            foreach (var fileName in Directory.GetFiles(folder, "*.sitecorerocks.xml", SearchOption.AllDirectories))
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(fileName);
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to load: " + fileName, ex, this);
                    continue;
                }

                if (doc.Root == null)
                {
                    Log.Error("Failed to load: " + fileName, this);
                    continue;
                }

                pipeline.Output.WriteRaw(doc.Root.ToString());
            }
        }
    }
}
