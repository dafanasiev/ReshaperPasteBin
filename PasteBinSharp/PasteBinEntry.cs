using System;

namespace PasteBinSharp
{
    public class PasteBinEntry
    {
        // Set only by the API
        public string Key { get; internal set; }
        public Uri Url { get; internal set; }

        public string Title { get; set; }
        public string Text { get; set; }
        public string Format { get; set; }
        public bool Private { get; set; }
        public PasteBinExpiration Expiration { get; set; }
    }
}