// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Text.RegularExpressions;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensions.DateTimeExtensions
{
    public static class DateTimeExtensions
    {
        public const int IsoDateLength = 8;

        public const int IsoDateTimeLength = 15;

        public const int IsoDateTimeUtcLength = 16;

        public const int IsoDateUtcLength = 9;

        public const int IsoTimeLength = 6;

        public static DateTime FromIso([NotNull] string isoDateTime)
        {
            Assert.ArgumentNotNull(isoDateTime, nameof(isoDateTime));

            return FromIso(isoDateTime, DateTime.MinValue);
        }

        public static DateTime FromIso([CanBeNull] string isoDateTime, DateTime defaultValue)
        {
            if (isoDateTime == null)
            {
                return defaultValue;
            }

            try
            {
                bool isUtc;
                if (isoDateTime.Length > IsoDateTimeLength && isoDateTime[IsoDateTimeLength] == ':')
                {
                    var ticks = isoDateTime.Substring(IsoDateTimeLength + 1);

                    if (ticks.Length > 0)
                    {
                        isUtc = ticks.EndsWith(Constants.IsoDateTimeUtcMarker, StringComparison.InvariantCultureIgnoreCase);
                        if (isUtc)
                        {
                            ticks = ticks.Replace(Constants.IsoDateTimeUtcMarker, string.Empty);
                            return new DateTime(GetLong(ticks, 0), DateTimeKind.Utc);
                        }

                        return new DateTime(GetLong(ticks, 0));
                    }
                }

                var parts = GetIsoDateParts(isoDateTime, out isUtc);

                if (parts == null)
                {
                    return defaultValue;
                }

                if (parts.Length >= 6)
                {
                    return new DateTime(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], isUtc ? DateTimeKind.Utc : DateTimeKind.Unspecified);
                }

                if (parts.Length >= 3)
                {
                    return new DateTime(parts[0], parts[1], parts[2], 0, 0, 0, isUtc ? DateTimeKind.Utc : DateTimeKind.Unspecified);
                }
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }

            return defaultValue;
        }

        [NotNull]
        public static string ToIsoDate(DateTime datetime)
        {
            var result = datetime.ToString(@"yyyyMMddTHHmmss");

            if (datetime.Kind == DateTimeKind.Utc)
            {
                result = result + Constants.IsoDateTimeUtcMarker;
            }

            return result;
        }

        private static int GetInt([CanBeNull] object obj, int defaultValue)
        {
            if (obj == null)
            {
                return defaultValue;
            }

            if (obj is int)
            {
                return (int)obj;
            }

            try
            {
                return Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }

            return defaultValue;
        }

        [CanBeNull]
        private static int[] GetIsoDateParts([NotNull] string isoDate, out bool isUtc)
        {
            Debug.ArgumentNotNull(isoDate, nameof(isoDate));

            isUtc = false;
            if (isoDate.Length != IsoDateLength && isoDate.Length != IsoDateUtcLength && isoDate.Length != IsoDateTimeLength && isoDate.Length != IsoDateTimeUtcLength)
            {
                return null;
            }

            if (Regex.IsMatch(isoDate, @"[^0-9TZ]"))
            {
                return null;
            }

            int[] parts =
            {
                0,
                0,
                0,
                0,
                0,
                0
            };

            parts[0] = GetInt(isoDate.Substring(0, 4), 0);
            parts[1] = GetInt(isoDate.Substring(4, 2), 0);
            parts[2] = GetInt(isoDate.Substring(6, 2), 0);

            if (isoDate.Length > IsoDateLength && isoDate[IsoDateLength] == 'T')
            {
                parts[3] = GetInt(isoDate.Substring(9, 2), 0);
                parts[4] = GetInt(isoDate.Substring(11, 2), 0);
                parts[5] = GetInt(isoDate.Substring(13, 2), 0);
            }

            isUtc = isoDate.EndsWith(Constants.IsoDateTimeUtcMarker, StringComparison.InvariantCultureIgnoreCase);

            return parts;
        }

        private static long GetLong([CanBeNull] object obj, long defaultValue)
        {
            if (obj != null)
            {
                try
                {
                    return Convert.ToInt64(obj);
                }
                catch (Exception ex)
                {
                    AppHost.Output.LogException(ex);
                }
            }

            return defaultValue;
        }
    }
}
