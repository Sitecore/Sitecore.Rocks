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
            "sitecore_userticket",
            ".AspNet.ExternalCookie",
            ".AspNet.Cookies"
        };

        public AspxAuthCookieClientMessageInspector([NotNull] string hostName)
        {
            HostName = hostName;
        }

        [NotNull]
        public string HostName { get; }

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
        public object BeforeSendRequest([CanBeNull] ref Message request, [CanBeNull] IClientChannel channel)
        {
            return null;
        }

        [CanBeNull]
        public string GetCookie(string cookieHeader, string cookieName)
        {
            var result = cookieHeader ?? string.Empty;
            var n = result.IndexOf(cookieName, StringComparison.Ordinal);

            if (n >= 0)
            {
                result = result.Mid(n);
                result = result.Mid(cookieName.Length+1);
                n = result.IndexOf(";", StringComparison.Ordinal);
                if (n >= 0)
                {
                    result = result.Left(n);
                }

                return result;
            }

            return string.Empty;
        }
    }
}
