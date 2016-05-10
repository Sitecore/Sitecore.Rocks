// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Text.RegularExpressions;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensions.StringExtensions
{
    public static class StringExtensions
    {
        [NotNull]
        public static string GetSafeCodeIdentifier([NotNull] this string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var chars = text.ToCharArray();

            for (var n = 1; n < chars.Length; n++)
            {
                var p = chars[n - 1];

                if (char.IsWhiteSpace(p))
                {
                    chars[n] = char.ToUpper(chars[n]);
                }
            }

            text = new string(chars);

            var result = Regex.Replace(text, @"\W", string.Empty).Replace(@" ", string.Empty);
            if (!char.IsLetter(result[0]))
            {
                result = @"_" + result;
            }

            return result;
        }
    }
}
