// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.IO;
using System.Reflection;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;
using Sitecore.Rocks.Server.Extensions.AssemblyExtensions;

namespace Sitecore.Rocks.Server.Pipelines.Login
{
    [Pipeline(typeof(LoginPipeline), 2000)]
    public class ServerComponents : PipelineProcessor<LoginPipeline>
    {
        protected override void Process(LoginPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            pipeline.Output.WriteStartElement("servercomponents");

            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
            foreach (var fileName in Directory.GetFiles(folder, @"Sitecore.Rocks.Server*.dll"))
            {
                try
                {
                    var assembly = Assembly.Load(fileName);
                    var version = assembly.GetFileVersion().ToString();

                    pipeline.Output.WriteStartElement("component");
                    pipeline.Output.WriteAttributeString("name", Path.GetFileNameWithoutExtension(fileName));
                    pipeline.Output.WriteAttributeString("version", version);
                    pipeline.Output.WriteEndElement();
                }
                catch
                {
                    continue;
                }
            }

            pipeline.Output.WriteEndElement();
        }
    }
}
