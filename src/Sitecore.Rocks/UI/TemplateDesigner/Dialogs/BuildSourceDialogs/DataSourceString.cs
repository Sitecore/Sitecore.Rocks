// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    public class DataSourceString
    {
        private readonly NameValueCollection _parameters = new NameValueCollection();

        private string _hash = string.Empty;

        private string _hostName = string.Empty;

        private string _path = string.Empty;

        private string _protocol = string.Empty;

        public DataSourceString()
        {
        }

        public DataSourceString([NotNull] string url)
        {
            Assert.ArgumentNotNull(url, nameof(url));

            Parse(url);
        }

        public DataSourceString([NotNull] NameValueCollection parameters)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            _parameters.Add(parameters);
        }

        public string Extension
        {
            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                var extensionStartIndex = _path.LastIndexOf(@".", StringComparison.Ordinal);
                _path = _path.Substring(0, extensionStartIndex) + @"." + value;
            }
        }

        [NotNull]
        public string Hash
        {
            get { return _hash; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _hash = value;
            }
        }

        public bool HasPath => !string.IsNullOrEmpty(_path);

        [NotNull]
        public string HostName
        {
            get { return _hostName; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));

                _hostName = value;
            }
        }

        [CanBeNull]
        public string this[[NotNull, Localizable(false)] string key]
        {
            get { return _parameters[key]; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                Append(key, value);
            }
        }

        [NotNull]
        public NameValueCollection Parameters => _parameters;

        [NotNull]
        public string Path
        {
            get { return _path; }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _path = value;
            }
        }

        [NotNull]
        public string Protocol
        {
            get
            {
                var protocol = _protocol;

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

                _protocol = value;
            }
        }

        [NotNull]
        public string Query
        {
            get { return GetQuery(); }

            set
            {
                Assert.ArgumentNotNull(value, nameof(value));
                _parameters.Clear();
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

            _parameters[key] = value;

            return GetUrl();
        }

        public void Append([NotNull] string key, [NotNull] string value)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            _parameters[key] = value;
        }

        public void Append([NotNull] NameValueCollection arguments)
        {
            Assert.ArgumentNotNull(arguments, nameof(arguments));

            foreach (string key in arguments.Keys)
            {
                Append(key, arguments[key]);
            }
        }

        public void Append([NotNull] DataSourceString url)
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

            result.Append(_path);

            if (_path.Length > 0 && _parameters.Count > 0)
            {
                result.Append('?');
            }

            result.Append(GetQuery());

            if (!string.IsNullOrEmpty(_hash))
            {
                result.Append('#');
                result.Append(_hash);
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

            _parameters.Remove(key);

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

            _parameters.Remove(key);
        }

        [NotNull]
        private string GetQuery()
        {
            var result = new StringBuilder();
            var first = true;
            foreach (string key in _parameters.Keys)
            {
                if (!first)
                {
                    result.Append('&');
                }

                result.Append(key);
                if (_parameters[key] != string.Empty)
                {
                    result.Append('=');
                    result.Append(_parameters[key]);
                }

                first = false;
            }

            return result.ToString();
        }

        private void Parse([NotNull] string url)
        {
            Debug.ArgumentNotNull(url, nameof(url));

            var startOfHashSection = url.IndexOf(@"#", StringComparison.Ordinal);

            if (startOfHashSection >= 0)
            {
                _hash = url.Mid(startOfHashSection + 1);
                url = url.Left(startOfHashSection);
            }

            var startOfParameterSection = url.IndexOf(@"?", StringComparison.Ordinal);
            var hasPathParameterSeparator = startOfParameterSection >= 0;
            var hasQuery = url.Contains(@"=");
            var hasPath = hasPathParameterSeparator || !hasQuery;

            if (hasPathParameterSeparator && !hasQuery)
            {
                _path = url.Left(startOfParameterSection);
                return;
            }

            if (!hasQuery)
            {
                _path = url;
                return;
            }

            if (!hasPath)
            {
                ParseQuery(url);
                return;
            }

            _path = url.Substring(0, startOfParameterSection);

            var parameterSection = url.Substring(startOfParameterSection + 1);

            ParseQuery(parameterSection);
        }

        private void ParseQuery([NotNull] string parameterSection)
        {
            Debug.ArgumentNotNull(parameterSection, nameof(parameterSection));

            var parameters = parameterSection.Split('&');

            foreach (var parameterNameValue in parameters)
            {
                var parameterNameValueArray = parameterNameValue.Split('=');

                if (parameterNameValueArray.Length == 1)
                {
                    _parameters.Add(parameterNameValueArray[0], string.Empty);
                }
                else
                {
                    _parameters.Add(parameterNameValueArray[0], parameterNameValueArray[1]);
                }
            }
        }
    }
}
