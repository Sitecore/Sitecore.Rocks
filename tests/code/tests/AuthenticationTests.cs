using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Sitecore.Rocks.Server.IntegrationTests.Extensions;
using Sitecore.Rocks.Server.IntegrationTests.HardRocks;
using Xunit;

namespace Sitecore.Rocks.Server.IntegrationTests
{
    public class AuthenticationTests
    {
        private static readonly string[] IdentityServerAuthCookies = new[]
        {
            ".AspNet.ExternalCookie",
            ".AspNet.Cookies"
        };

        private static readonly string[] AspNetMembershipCookies = new[]
        {
            ".ASPXAUTH"
        };

        [Fact]
        public async Task LogsIn()
        {
            var client = ClientFactory.Client;
            client.Endpoint.EndpointBehaviors.Add(new CookieAssertionBehavior(IdentityServerAuthCookies, AspNetMembershipCookies, contains: true));

            var response = await client.LoginAsync(Properties.Credentials);
            var result = response?.Body?.LoginResult;

            Assert.StartsWith("<login>", result);
            var xmlResult = result.ToDynamic();
            Assert.NotNull(xmlResult.login.sitecorerocks);
        }

        [Fact]
        public async Task WindowsAuthLoginFails()
        {
            var client = ClientFactory.Client;
            client.Endpoint.EndpointBehaviors.Add(new CookieAssertionBehavior(IdentityServerAuthCookies, AspNetMembershipCookies, contains: false));

            var response = await client.LoginAsync(new Credentials
            {
                UserName = "sitecore\\admin",
                Password = string.Empty,
                CustomData = "windowsauth"
            });
            var result = response?.Body?.LoginResult;
            Assert.DoesNotContain("<login>", result);
        }

        private class CookieAssertionBehavior : IEndpointBehavior
        {
            private readonly string[] _identityCookies;
            private readonly string[] _membershipCookies;
            private readonly bool _contains;

            public CookieAssertionBehavior(string[] identityCookies, string[] membershipCookies, bool contains)
            {
                _identityCookies = identityCookies;
                _membershipCookies = membershipCookies;
                _contains = contains;
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
                clientRuntime.MessageInspectors.Add(new CookieAssertionInspector(_identityCookies, _membershipCookies, _contains));
            }

            #region unused
            public void Validate(ServiceEndpoint endpoint) { }
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }
            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }
            #endregion

        }

        private class CookieAssertionInspector : IClientMessageInspector
        {
            private readonly Version FirstIdentityVersion = new Version(9, 1);

            private readonly string[] _identityCookies;
            private readonly string[] _membershipCookies;
            private readonly bool _contains;

            public CookieAssertionInspector(string[] identityCookies, string[] membershipCookies, bool contains)
            {
                _identityCookies = identityCookies;
                _membershipCookies = membershipCookies;
                _contains = contains;
            }

            public void AfterReceiveReply(ref Message reply, object correlationState)
            {
                var expectedCookies = _membershipCookies;
                if (ClientFactory.SitecoreVersion >= FirstIdentityVersion)
                {
                    expectedCookies = _identityCookies;
                }

                var httpResponse = reply.Properties[HttpResponseMessageProperty.Name] as HttpResponseMessageProperty;
                Assert.NotNull(httpResponse);

                var cookieHeader = httpResponse.Headers[HttpResponseHeader.SetCookie];
                if (!_contains && cookieHeader == null)
                {
                    return;
                }
                Assert.NotNull(cookieHeader);

                foreach (var cookie in expectedCookies)
                {
                    if (_contains)
                    {
                        Assert.Contains(cookie, cookieHeader);
                    }
                    else
                    {
                        Assert.DoesNotContain(cookie, cookieHeader);
                    }
                }
            }

            #region unused
            public object BeforeSendRequest(ref Message request, IClientChannel channel)
            {
                return null;
            }
            #endregion
        }
    }
}
