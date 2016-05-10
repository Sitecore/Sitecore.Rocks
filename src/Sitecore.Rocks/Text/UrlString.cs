// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Web;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.Text
{
    public class UrlString
    {
        private readonly NameValueCollection parameters = new NameValueCollection();

        private string hash = string.Empty;

        private string hostName = string.Empty;

        private string path = string.Empty;

        private string protocol = string.Empty;

        public UrlString()
        {
        }

        public UrlString([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            Parse(url);
        }

        public UrlString([NotNull] NameValueCollection parameters)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            this.parameters.Add(parameters);
        }

        public string Extension
        {
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                var extensionStartIndex = path.LastIndexOf(@".", StringComparison.Ordinal);
                path = path.Substring(0, extensionStartIndex) + @"." + value;
            }
        }

        [NotNull]
        public string Hash
        {
            get { return hash; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                hash = value;
            }
        }

        public bool HasPath
        {
            get { return !string.IsNullOrEmpty(path); }
        }

        [NotNull]
        public string HostName
        {
            get { return hostName; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                hostName = value;
            }
        }

        [CanBeNull]
        public string this[[NotNull, Localizable(false)] string key]
        {
            get { return parameters[key]; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                Append(key, value);
            }
        }

        [NotNull]
        public NameValueCollection Parameters
        {
            get { return parameters; }
        }

        [NotNull]
        public string Path
        {
            get { return path; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                path = value;
            }
        }

        [NotNull]
        public string Protocol
        {
            get
            {
                var protocol = this.protocol;
                if (protocol.Length > 0)
                {
                    return protocol;
                }

                return @"http";
            }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                Assert.IsTrue(value.IndexOf(@"://", StringComparison.Ordinal) < 0, Resources.UrlString_Protocol_Protocol_must_not_contain________only_the_protocol_name_should_be_used__ex__http__);

                protocol = value;
            }
        }

        [NotNull]
        public string Query
        {
            get { return GetQuery(); }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                parameters.Clear();
                ParseQuery(value);
            }
        }

        [NotNull]
        public string Add([NotNull, Localizable(false)] string key, [NotNull] string value)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            return Add(key, value, true);
        }

        [NotNull]
        public string Add([NotNull] string key, [NotNull] string value, bool allowBlank)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            if (!allowBlank)
            {
                Assert.IsNotNullOrEmpty(value, Resources.UrlString_Add_Blank_values_are_not_allowed);
            }

            parameters[key] = HttpUtility.UrlEncode(value);

            return GetUrl();
        }

        public void Append([NotNull] string key, [NotNull] string value)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            parameters[key] = HttpUtility.UrlEncode(value);
        }

        public void Append([NotNull] NameValueCollection arguments)
        {
            Assert.ArgumentNotNull(arguments, nameof(arguments));

            foreach (string key in arguments.Keys)
            {
                Append(key, arguments[key]);
            }
        }

        public void Append([NotNull] UrlString url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            Append(url.Parameters);
        }

        [NotNull]
        public string GetUrl(bool xhtmlFormat)
        {
            var result = new StringBuilder();

            if (HostName.Length > 0)
            {
                result.Append(Protocol);
                result.Append(@"://");
                result.Append(HostName);
            }

            result.Append(path);

            if (path.Length > 0 && parameters.Count > 0)
            {
                result.Append('?');
            }

            result.Append(GetQuery());

            if (!string.IsNullOrEmpty(hash))
            {
                result.Append('#');
                result.Append(hash);
            }

            if (xhtmlFormat)
            {
                result.Replace(@"&", @"&amp;");
            }

            return result.ToString();
        }

        [NotNull]
        public string GetUrl()
        {
            return GetUrl(false);
        }

        [NotNull]
        public string Remove([NotNull] string key)
        {
            Assert.ArgumentNotNullOrEmpty(key, "key");

            parameters.Remove(key);

            return GetUrl();
        }

        [NotNull]
        public override string ToString()
        {
            return GetUrl();
        }

        public void Truncate([NotNull] string key)
        {
            Assert.ArgumentNotNullOrEmpty(key, "key");

            parameters.Remove(key);
        }

        [NotNull]
        private string GetQuery()
        {
            var result = new StringBuilder();
            var first = true;
            foreach (string key in parameters.Keys)
            {
                if (!first)
                {
                    result.Append('&');
                }

                result.Append(key);
                if (parameters[key] != string.Empty)
                {
                    result.Append('=');
                    result.Append(parameters[key]);
                }

                first = false;
            }

            return result.ToString();
        }

        private void Parse([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            var startOfHashSection = url.IndexOf(@"#", StringComparison.Ordinal);

            if (startOfHashSection >= 0)
            {
                hash = url.Mid(startOfHashSection + 1);
                url = url.Left(startOfHashSection);
            }

            var startOfParameterSection = url.IndexOf(@"?", StringComparison.Ordinal);
            var hasPathParameterSeparator = startOfParameterSection >= 0;
            var hasQuery = url.Contains(@"=");
            var hasPath = hasPathParameterSeparator || !hasQuery;

            if (hasPathParameterSeparator && !hasQuery)
            {
                path = url.Left(startOfParameterSection);
                return;
            }

            if (!hasQuery)
            {
                path = url;
                return;
            }

            if (!hasPath)
            {
                ParseQuery(url);
                return;
            }

            path = url.Substring(0, startOfParameterSection);

            var parameterSection = url.Substring(startOfParameterSection + 1);

            ParseQuery(parameterSection);
        }

        private void ParseQuery([NotNull] string parameterSection)
        {
            Assert.ArgumentNotNull(parameterSection, nameof(parameterSection));

            var parameters = parameterSection.Split('&');

            foreach (var parameterNameValue in parameters)
            {
                var parameterNameValueArray = parameterNameValue.Split('=');

                if (parameterNameValueArray.Length == 1)
                {
                    this.parameters.Add(parameterNameValueArray[0], string.Empty);
                }
                else
                {
                    this.parameters.Add(parameterNameValueArray[0], parameterNameValueArray[1]);
                }
            }
        }
    }
}
