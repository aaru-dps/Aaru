using System;
using DiscImageChef.Helpers;

namespace DiscImageChef.Filesystems.ISO9660
{
    public partial class ISO9660
    {
        static DateTime? DecodeIsoDateTime(byte[] timestamp)
        {
            switch(timestamp?.Length)
            {
                case 7:  return DecodeIsoDateTime(Marshal.ByteArrayToStructureLittleEndian<IsoTimestamp>(timestamp));
                case 17: return DateHandlers.Iso9660ToDateTime(timestamp);
                default: return null;
            }
        }

        static DateTime? DecodeIsoDateTime(IsoTimestamp timestamp)
        {
            try
            {
                DateTime date = new DateTime(timestamp.Years + 1900, timestamp.Month, timestamp.Day, timestamp.Hour,
                                             timestamp.Minute, timestamp.Second, DateTimeKind.Unspecified);

                date = date.AddMinutes(timestamp.GmtOffset * 15);

                return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById("GMT"));
            }
            catch(Exception)
            {
                // ISO says timestamp can be unspecified
                return null;
            }
        }

        static DateTime? DecodeHighSierraDateTime(HighSierraTimestamp timestamp)
        {
            try
            {
                DateTime date = new DateTime(timestamp.Years + 1900, timestamp.Month, timestamp.Day, timestamp.Hour,
                                             timestamp.Minute, timestamp.Second, DateTimeKind.Unspecified);

                return TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById("GMT"));
            }
            catch(Exception)
            {
                // ISO says timestamp can be unspecified, suppose same for High Sierra
                return null;
            }
        }
    }
}