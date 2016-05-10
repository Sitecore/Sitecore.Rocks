// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Text;

namespace Sitecore.Rocks.UI.LayoutDesigners.Properties.Parameters
{
    [TypeConverter(typeof(ParameterDictionaryConverter))]
    public class ParameterDictionary
    {
        private static readonly ParameterDictionary empty = new ParameterDictionary("");

        public ParameterDictionary([NotNull] string urlString)
        {
            Assert.ArgumentNotNull(urlString, nameof(urlString));
            Parameters = new Dictionary<string, string>();

            var url = new UrlString(urlString);

            foreach (string key in url.Parameters.Keys)
            {
                if (key != null)
                {
                    Parameters[key] = url.Parameters[key];
                }
            }
        }

        public ParameterDictionary([NotNull] Dictionary<string, string> parameters)
        {
            Assert.ArgumentNotNull(parameters, nameof(parameters));

            Parameters = new Dictionary<string, string>(parameters);
        }

        [NotNull]
        public static ParameterDictionary Empty => empty;

        [NotNull]
        public Dictionary<string, string> Parameters { get; set; }

        public override string ToString()
        {
            var urlString = new UrlString();

            foreach (var pair in Parameters)
            {
                urlString.Parameters[pair.Key] = pair.Value;
            }

            return urlString.ToString();
        }
    }
}
