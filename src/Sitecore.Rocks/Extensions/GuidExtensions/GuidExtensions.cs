// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Security.Cryptography;
using System.Text;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.GuidExtensions
{
    public static class GuidExtensions
    {
        [NotNull]
        public static string Format(this Guid guid)
        {
            return guid.ToString(@"B").ToUpperInvariant();
        }

        public static Guid Hash([NotNull] string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            var bytes = Encoding.UTF8.GetBytes(text);

            var hash = MD5.Create().ComputeHash(bytes);

            return new Guid(hash);
        }

        public static bool IsGuid([NotNull] this string text)
        {
            Assert.ArgumentNotNull(text, nameof(text));

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            Guid guid;
            return Guid.TryParse(text, out guid);
        }

        [NotNull]
        public static string ToShortId(this Guid guid)
        {
            var result = guid.ToString("B").ToUpperInvariant();

            return result.Substring(1, 8) + result.Substring(10, 4) + result.Substring(15, 4) + result.Substring(20, 4) + result.Substring(25, 12);
        }
    }
}
