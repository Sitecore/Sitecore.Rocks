// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Xml;
using Sitecore.Diagnostics;
using Sitecore.Rocks.Server.Extensibility.Pipelines;

namespace Sitecore.Rocks.Server.Pipelines.Login
{
    public class LoginPipeline : Pipeline<LoginPipeline>
    {
        [NotNull]
        public XmlTextWriter Output { get; set; }

        [NotNull]
        public LoginPipeline WithParameters([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            Output = output;

            Start();

            return this;
        }
    }
}
