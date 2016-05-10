// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.LoggedIn
{
    public class LoggedInPipeline : Pipeline<LoggedInPipeline>
    {
        [NotNull]
        public XElement RootElement { get; private set; }

        [NotNull]
        public WebDataService WebDataService { get; private set; }

        [NotNull]
        public LoggedInPipeline WithParameters([NotNull] WebDataService webDataService, [NotNull] XElement output)
        {
            Assert.ArgumentNotNull(webDataService, nameof(webDataService));
            Assert.ArgumentNotNull(output, nameof(output));

            WebDataService = webDataService;
            RootElement = output;

            Start();

            return this;
        }
    }
}
