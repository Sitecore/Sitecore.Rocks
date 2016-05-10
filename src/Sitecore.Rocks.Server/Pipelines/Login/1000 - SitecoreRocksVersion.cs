// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Reflection;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Rocks.Server.Extensions.AssemblyExtensions;

namespace Sitecore.Rocks.Server.Pipelines.Login
{
    [Pipeline(typeof(LoginPipeline), 1000)]
    public class SitecoreRocksVersion : PipelineProcessor<LoginPipeline>
    {
        protected override void Process(LoginPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Output.WriteStartElement("sitecorerocks");

            pipeline.Output.WriteAttributeString("rocks", Assembly.GetExecutingAssembly().GetFileVersion().ToString());
            pipeline.Output.WriteAttributeString("sitecore", About.Version);

            pipeline.Output.WriteEndElement();
        }
    }
}
