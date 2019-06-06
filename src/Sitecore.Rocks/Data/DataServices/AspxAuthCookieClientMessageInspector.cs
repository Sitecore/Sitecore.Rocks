// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Data.DataServices
{
    internal class AspxAuthCookieClientMessageInspector : IClientMessageInspector
    {
        private readonly string[] _cookies =
        {
            ".ASPXAUTH",
            ".AspNet.ExternalCookie",
            ".AspNet.Cookies"
        };

        public AspxAuthCookieClientMessageInspector([NotNull] string hostName)
        {
            HostName = hostName;
        }

        [NotNull]
        public string HostName { get; }

        /// <summary>
        /// Save select cookies to the WPF Application so that they will be sent with non-WCF requests (e.g. media library images).
        /// </summary>
        public void AfterReceiveReply([NotNull] ref Message reply, [CanBeNull] object correlationState)
        {
            Assert.ArgumentNotNull(reply, nameof(reply));

            var httpResponse = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
            if (httpResponse == null)
            {
                return;
            }

            var cookieHeader = httpResponse.Headers[HttpResponseHeader.SetCookie];
            if (string.IsNullOrEmpty(cookieHeader))
            {
                return;
            }

            foreach (var cookieName in _cookies)
            {
                var cookieValue = GetCookie(cookieHeader, cookieName);
                if (string.IsNullOrEmpty(cookieValue))
                {
                    continue;
                }

                try
                {
                    Application.SetCookie(new Uri(HostName), $"{cookieName}={cookieValue}");
                }
                catch (Exception ex)
                {
                    AppHost.Output.Log($"Failed to set cookie {cookieName}: " + ex.Message + ex.StackTrace);
                }
            }
        }

        [CanBeNull]
        private string GetCookie(string cookieHeader, string cookieName)
        {
            var result = cookieHeader ?? string.Empty;
            var n = result.IndexOf(cookieName, StringComparison.Ordinal);

            if (n >= 0)
            {
                result = result.Mid(n);
                result = result.Mid(cookieName.Length + 1);
                n = result.IndexOf(";", StringComparison.Ordinal);
                if (n >= 0)
                {
                    result = result.Left(n);
                }

                return result;
            }

            return string.Empty;
        }

        /// <summary>
        /// Ensure that WCF only sends back the cookies we want, as cookies like sitecore_userticket cause more harm than good.
        /// </summary>
        [CanBeNull]
        public object BeforeSendRequest([CanBeNull] ref Message request, [CanBeNull] IClientChannel channel)
        {
            var cookieManager = channel.GetProperty<IHttpCookieContainerManager>();
            cookieManager.CookieContainer = FilterCookies(cookieManager.CookieContainer);

            return null;
        }

        private CookieContainer FilterCookies(CookieContainer original)
        {
            var cookies = original.GetCookies(new Uri(HostName));
            var cookieContainer = new CookieContainer();
            foreach (var cookieName in _cookies)
            {
                var value = cookies[cookieName];
                if (value == null)
                {
                    continue;
                }
                cookieContainer.Add(value);
            }
            return cookieContainer;
        }

    }
}
