// © 2015-2018 Sitecore Corporation A/S. All rights reserved.

using System.ServiceModel;
using System.Windows;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Net.Pipelines.Troubleshooter
{
    [Pipeline(typeof(TroubleshooterPipeline), 3000)]
    public class LoginRedirectOnDataService : PipelineProcessor<TroubleshooterPipeline>
    {
        private const string ContentTypeError = "The content type text/html";
        private const string LoginPageText = "Welcome to Sitecore";
        private const string IdentityServerText = "/identity/externallogin";

        protected override void Process(TroubleshooterPipeline pipeline)
        {
            Debug.ArgumentNotNull(pipeline, nameof(pipeline));

            if (pipeline.Exception == null)
            {
                return;
            }

            if (!(pipeline.Exception is ProtocolException))
            {
                return;
            }

            /*
             * Check if exception message indicates a redirect to an HTML doc, and whether that HTML doc contains either
             *      * Welcome to Sitecore (likely 9.0 or 9.1+ with Identity Server disabled)
             *      * The Identity Server redirect form post (9.1+)
             */
            var message = pipeline.Exception.Message;
            if (!message.Contains(ContentTypeError) || (!message.Contains(LoginPageText) && !message.Contains(IdentityServerText)))
            {
                return;
            }

            pipeline.Abort();
            pipeline.Cancelled = true;
            pipeline.Retry = false;

            if (pipeline.Silent)
            {
                return;
            }

            AppHost.MessageBox("Access was denied when trying to communicate with the Sitecore service. \r\n\r\n" +
                               "Sitecore 9.0+ requires configuring anonymous access to utilize the Good Old Web Service. " +
                               "Installing the Hard Rock Web Service will automatically enable anonymous access to the services. \r\n\r\n" +
                               "See https://kb.sitecore.net/articles/TODO for more information.",
                               Resources.Error, MessageBoxButton.OK, MessageBoxImage.Hand);
        }
    }
}
