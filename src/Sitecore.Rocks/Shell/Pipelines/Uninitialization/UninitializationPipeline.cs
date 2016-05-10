// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Uninitialization
{
    public class UninitializationPipeline : Pipeline<UninitializationPipeline>
    {
        public void WithParameters()
        {
            Start();
        }
    }
}
