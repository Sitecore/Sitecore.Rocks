// © 2015 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Xml.Linq;
using Sitecore.Extensions.XElementExtensions;
using Sitecore.VisualStudio.Net;

namespace Sitecore
{
    public class Connection
    {
        public string HostName { get; private set; }

        public string Password { get; private set; }

        public TimeSpan ReceiveTimeout { get; private set; }

        public string UserName { get; private set; }

        public void Load(XElement element)
        {
            HostName = element.GetAttributeValue(@"hostName");
            UserName = element.GetAttributeValue(@"userName");
            ReceiveTimeout = TimeSpan.Parse(element.GetAttributeValue(@"receiveTimeout"));

            var password = element.GetAttributeValue(@"password");
            if (!string.IsNullOrEmpty(password))
            {
                var blowFish = new BlowFish(BlowFish.CipherKey);
                password = blowFish.Decrypt_ECB(password);
            }

            Password = password;
        }
    }
}
