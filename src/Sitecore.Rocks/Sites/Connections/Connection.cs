// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Net;

namespace Sitecore.Rocks.Sites.Connections
{
    public class Connection
    {
        public Connection()
        {
            FileName = string.Empty;

            var binding = new BasicHttpBinding();

            HostName = string.Empty;
            UserName = string.Empty;
            Password = string.Empty;
            DataServiceName = string.Empty;
            WebRootPath = string.Empty;
            Description = string.Empty;
            IsRemoteSitecore = false;
            UseWindowsAuth = false;
            AutomaticallyUpdate = true;

            HostNameComparisonMode = binding.HostNameComparisonMode;
            ReceiveTimeout = binding.ReceiveTimeout;
            SendTimeout = binding.SendTimeout;
            OpenTimeout = binding.OpenTimeout;
            CloseTimeout = binding.CloseTimeout;
            MaxBufferSize = 1024 * 1024 * 16;
            MaxBufferPoolSize = binding.MaxBufferPoolSize;
            MaxStringContentLength = 1024 * 1024 * 16;
            MaxReceivedMessageSize = 1024 * 1024 * 16;
            TransferMode = binding.TransferMode;
            MessageEncoding = binding.MessageEncoding;
            TextEncoding = binding.TextEncoding;
            BypassProxyOnLocal = binding.BypassProxyOnLocal;
            UseDefaultWebProxy = binding.UseDefaultWebProxy;
            ProxyAddress = binding.ProxyAddress;
        }

        [Category("Updates"), ReadOnly(true)]
        public bool AutomaticallyUpdate { get; set; }

        [Category("Proxy")]
        public bool BypassProxyOnLocal { get; set; }

        [Category("Timeouts")]
        public TimeSpan CloseTimeout { get; set; }

        [NotNull, ReadOnly(true), Category("Connection")]
        public string DataServiceName { get; set; }

        [NotNull, Browsable(false)]
        public string Description { get; set; }

        [NotNull, ReadOnly(true), Category("Connection")]
        public string FileName { get; set; }

        [NotNull, ReadOnly(true), Category("Connection")]
        public string HostName { get; set; }

        [Browsable(false)]
        public bool IsHidden { get; set; }

        [Category("Quotas")]
        public long MaxBufferPoolSize { get; set; }

        [Category("Quotas")]
        public int MaxBufferSize { get; set; }

        [Category("Quotas")]
        public long MaxReceivedMessageSize { get; set; }

        [Category("Quotas")]
        public int MaxStringContentLength { get; set; }

        [Category("Protocol")]
        public WSMessageEncoding MessageEncoding { get; set; }

        [Category("Timeouts")]
        public TimeSpan OpenTimeout { get; set; }

        [NotNull, Browsable(false)]
        public string Password { get; set; }

        [CanBeNull, Category("Proxy")]
        public Uri ProxyAddress { get; set; }

        [Category("Timeouts")]
        public TimeSpan ReceiveTimeout { get; set; }

        [Category("Timeouts")]
        public TimeSpan SendTimeout { get; set; }

        [NotNull, Browsable(false)]
        public Encoding TextEncoding { get; set; }

        [Category("Protocol")]
        public TransferMode TransferMode { get; set; }

        [Category("Proxy")]
        public bool UseDefaultWebProxy { get; set; }

        [NotNull, ReadOnly(true), Category("Security")]
        public string UserName { get; set; }

        [ReadOnly(true), Category("Security")]
        public bool UseWindowsAuth { get; set; }

        [NotNull, ReadOnly(true), Category("Connection")]
        public string WebRootPath { get; set; }

        [Obsolete, Browsable(false)]
        protected bool AutoUpdate { get; set; }

        protected HostNameComparisonMode HostNameComparisonMode { get; set; }

        protected bool IsRemoteSitecore { get; set; }

        [NotNull]
        public BasicHttpBinding GetBinding()
        {
            var securityMode = BasicHttpSecurityMode.None;
            var credentialType = HttpClientCredentialType.None;
            if (HostName.IndexOf(@"https://", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                securityMode = BasicHttpSecurityMode.Transport;
            }
            else if (UseWindowsAuth)
            {
                securityMode = BasicHttpSecurityMode.TransportCredentialOnly;
                credentialType = HttpClientCredentialType.Windows;
            }

            var result = new BasicHttpBinding(securityMode)
            {
                HostNameComparisonMode = HostNameComparisonMode,
                ReceiveTimeout = ReceiveTimeout,
                SendTimeout = SendTimeout,
                OpenTimeout = OpenTimeout,
                CloseTimeout = CloseTimeout,
                MaxBufferSize = MaxBufferSize,
                MaxBufferPoolSize = MaxBufferPoolSize,
                MaxReceivedMessageSize = MaxReceivedMessageSize,
                TransferMode = TransferMode,
                MessageEncoding = MessageEncoding,
                TextEncoding = TextEncoding,
                BypassProxyOnLocal = BypassProxyOnLocal,
                UseDefaultWebProxy = UseDefaultWebProxy,
                ReaderQuotas =
                {
                    MaxStringContentLength = MaxStringContentLength
                },
                ProxyAddress = ProxyAddress,
                AllowCookies = true
            };

            result.Security.Transport.ClientCredentialType = credentialType;

            return result;
        }

        public void Load([NotNull] XElement element)
        {
            Assert.ArgumentNotNull(element, nameof(element));

            HostName = element.GetAttributeValue(@"hostName");
            UseWindowsAuth = element.GetAttributeValue(@"useWindowsAuth") == @"true";
            UserName = element.GetAttributeValue(@"userName");
            DataServiceName = element.GetAttributeValue(@"dataService");
            WebRootPath = element.GetAttributeValue(@"webRootPath");
            Description = element.GetAttributeValue(@"description");
            IsRemoteSitecore = element.GetAttributeValue(@"isRemoteSitecore") == @"true";
            IsHidden = element.GetAttributeValue(@"isHidden") == @"true";

            if (element.HasAttribute("automaticallyUpdate"))
            {
                AutomaticallyUpdate = element.GetAttributeValue(@"automaticallyUpdate") == @"true";
            }

            TimeSpan timeSpan;

            HostNameComparisonMode = (HostNameComparisonMode)Enum.Parse(typeof(HostNameComparisonMode), element.GetAttributeValue(@"hostNameComparisonMode"));
            ReceiveTimeout = TimeSpan.Parse(element.GetAttributeValue(@"receiveTimeout"));
            SendTimeout = TimeSpan.TryParse(element.GetAttributeValue(@"sendTimeout"), out timeSpan) ? timeSpan : new TimeSpan(0, 1, 0);
            OpenTimeout = TimeSpan.TryParse(element.GetAttributeValue(@"openTimeout"), out timeSpan) ? timeSpan : new TimeSpan(0, 1, 0);
            CloseTimeout = TimeSpan.TryParse(element.GetAttributeValue(@"closeTimeout"), out timeSpan) ? timeSpan : new TimeSpan(0, 1, 0);
            MaxReceivedMessageSize = long.Parse(element.GetAttributeValue(@"maxReceivedMessageSize"));
            MaxBufferSize = int.Parse(element.GetAttributeValue(@"maxBufferSize"));
            MaxBufferPoolSize = long.Parse(element.GetAttributeValue(@"maxBufferPoolSize"));
            TransferMode = (TransferMode)Enum.Parse(typeof(TransferMode), element.GetAttributeValue(@"transferMode"));
            MessageEncoding = (WSMessageEncoding)Enum.Parse(typeof(WSMessageEncoding), element.GetAttributeValue(@"messageEncoding"));
            TextEncoding = Encoding.GetEncoding(element.GetAttributeValue(@"textEncoding"));
            BypassProxyOnLocal = element.GetAttributeValue(@"bypassProxyOnLocal") == @"true";
            UseDefaultWebProxy = element.GetAttributeValue(@"useDefaultWebProxy") == @"true";

            int maxStringContentLength;
            if (int.TryParse(element.GetAttributeValue(@"maxStringContentLength"), out maxStringContentLength))
            {
                MaxStringContentLength = maxStringContentLength;
            }

            var proxyAddress = element.GetAttributeValue(@"proxyAddress");
            if (!string.IsNullOrEmpty(proxyAddress))
            {
                try
                {
                    ProxyAddress = new Uri(proxyAddress);
                }
                catch (UriFormatException ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            var password = element.GetAttributeValue(@"password");
            if (!string.IsNullOrEmpty(password))
            {
                var blowFish = new BlowFish(BlowFish.CipherKey);
                password = blowFish.Decrypt_ECB(password);
            }

            Password = password;
        }

        public void Save([NotNull] XmlTextWriter output)
        {
            Assert.ArgumentNotNull(output, nameof(output));

            var blowFish = new BlowFish(BlowFish.CipherKey);
            var encryptedPassword = blowFish.Encrypt_ECB(Password);

            output.WriteStartElement(@"binding");

            output.WriteAttributeString(@"hostName", HostName);
            output.WriteAttributeString(@"useWindowsAuth", UseWindowsAuth ? @"true" : @"false");
            output.WriteAttributeString(@"userName", UserName);
            output.WriteAttributeString(@"password", encryptedPassword);
            output.WriteAttributeString(@"dataService", DataServiceName);
            output.WriteAttributeString(@"webRootPath", WebRootPath);
            output.WriteAttributeString(@"description", Description);
            output.WriteAttributeString(@"isRemoteSitecore", IsRemoteSitecore ? @"true" : @"false");
            output.WriteAttributeString(@"automaticallyUpdate", AutomaticallyUpdate ? @"true" : @"false");
            output.WriteAttributeString(@"isHidden", IsHidden ? @"true" : @"false");

            output.WriteAttributeString(@"hostNameComparisonMode", HostNameComparisonMode.ToString());
            output.WriteAttributeString(@"receiveTimeout", ReceiveTimeout.ToString());
            output.WriteAttributeString(@"sendTimeout", SendTimeout.ToString());
            output.WriteAttributeString(@"openTimeout", OpenTimeout.ToString());
            output.WriteAttributeString(@"closeTimeout", CloseTimeout.ToString());
            output.WriteAttributeString(@"maxReceivedMessageSize", MaxReceivedMessageSize.ToString());
            output.WriteAttributeString(@"maxBufferSize", MaxBufferSize.ToString());
            output.WriteAttributeString(@"maxBufferPoolSize", MaxBufferPoolSize.ToString());
            output.WriteAttributeString(@"maxStringContentLength", MaxStringContentLength.ToString());
            output.WriteAttributeString(@"transferMode", TransferMode.ToString());
            output.WriteAttributeString(@"messageEncoding", MessageEncoding.ToString());
            output.WriteAttributeString(@"textEncoding", TextEncoding.WebName);
            output.WriteAttributeString(@"bypassProxyOnLocal", BypassProxyOnLocal ? @"true" : @"false");
            output.WriteAttributeString(@"useDefaultWebProxy", UseDefaultWebProxy ? @"true" : @"false");

            if (ProxyAddress != null)
            {
                output.WriteAttributeString(@"proxyAddress", ProxyAddress.ToString());
            }
            else
            {
                output.WriteAttributeString(@"proxyAddress", string.Empty);
            }

            output.WriteEndElement();
        }
    }
}
