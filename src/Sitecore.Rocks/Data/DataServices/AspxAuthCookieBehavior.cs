// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Data.DataServices
{
    internal class AspxAuthCookieBehavior : IEndpointBehavior
    {
        private AspxAuthCookieClientMessageInspector aspxAuthCookieClientMessageInspector;

        public AspxAuthCookieBehavior([NotNull] string hostName)
        {
            HostName = hostName;
        }

        [NotNull]
        public string HostName { get; }

        [NotNull]
        protected AspxAuthCookieClientMessageInspector AspxAuthCookieClientMessageInspector
        {
            get { return aspxAuthCookieClientMessageInspector ?? (aspxAuthCookieClientMessageInspector = new AspxAuthCookieClientMessageInspector(HostName)); }
        }

        public void AddBindingParameters([CanBeNull] ServiceEndpoint endpoint, [CanBeNull] BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior([CanBeNull] ServiceEndpoint endpoint, [NotNull] ClientRuntime clientRuntime)
        {
            Assert.ArgumentNotNull(clientRuntime, nameof(clientRuntime));

            clientRuntime.MessageInspectors.Add(AspxAuthCookieClientMessageInspector);
        }

        public void ApplyDispatchBehavior([CanBeNull] ServiceEndpoint endpoint, [CanBeNull] EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate([CanBeNull] ServiceEndpoint endpoint)
        {
        }
    }
}
