// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.Initialization
{
    public class InitializationPipeline : Pipeline<InitializationPipeline>
    {
        public bool IsStartUp { get; set; }

        public void WithParameters(bool startUp)
        {
            IsStartUp = startUp;

            Start();
        }
    }
}
