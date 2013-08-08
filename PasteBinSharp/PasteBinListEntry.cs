using System;
using System.Xml.Linq;

namespace PasteBinSharp
{
    public class PasteBinListEntry
    {
        public string Key { get; internal set; }
        public Uri Url { get; internal set; }

        public string Title { get; internal set; }
        public string LongFormat { get; internal set; }
        public string ShortFormat { get; internal set; }
        public bool Private { get; internal set; }
        public DateTime CreationDate { get; internal set; }
        public DateTime? ExpirationDate { get; internal set; }
        public int Size { get; internal set; }
        public int Hits { get; internal set; }

        public static PasteBinListEntry Parse(XElement element)
        {
            var entry = new PasteBinListEntry();
            entry.Key = (string)element.Element("paste_key");
            entry.Url = new Uri((string)element.Element("paste_url"));
            entry.Title = (string)element.Element("paste_title");
            entry.LongFormat = (string)element.Element("paste_format_long");
            entry.ShortFormat = (string)element.Element("paste_format_short");

            entry.Size = (int)element.Element("paste_size");
            entry.Hits = (int)element.Element("paste_hits");

            long creationTicks = (long)element.Element("paste_date");
            entry.CreationDate = TimeStampToDate(creationTicks);

            long expireTicks = (long)element.Element("paste_expire_date");
            if (expireTicks > 0)
                entry.ExpirationDate = TimeStampToDate(expireTicks);
            else
                entry.ExpirationDate = null;

            return entry;
        }

        private static DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static DateTime TimeStampToDate(long timestamp)
        {
            return _unixEpoch.AddSeconds(timestamp);
        }
    }
}