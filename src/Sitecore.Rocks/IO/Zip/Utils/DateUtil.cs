// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;

namespace Sitecore.Rocks.IO.Zip.Utils
{
    internal class DateUtil
    {
        public static int Pack(DateTime time)
        {
            var packedDate = (ushort)((time.Day & 0x0000001F) | ((time.Month << 5) & 0x000001E0) | (((time.Year - 1980) << 9) & 0x0000FE00));
            var packedTime = (ushort)((time.Second & 0x0000001F) | ((time.Minute << 5) & 0x000007E0) | ((time.Hour << 11) & 0x0000F800));
            return (int)((uint)(packedDate << 16) | packedTime);
        }

        public static DateTime Unpack(int packedDateTime)
        {
            var packedTime = (short)(packedDateTime & 0x0000ffff);
            var packedDate = (short)((packedDateTime & 0xffff0000) >> 16);

            var year = 1980 + ((packedDate & 0xFE00) >> 9);
            var month = (packedDate & 0x01E0) >> 5;
            var day = packedDate & 0x001F;

            var hour = (packedTime & 0xF800) >> 11;
            var minute = (packedTime & 0x07E0) >> 5;
            var second = packedTime & 0x001F;

            try
            {
                return new DateTime(year, month, day, hour, minute, second, 0);
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}
