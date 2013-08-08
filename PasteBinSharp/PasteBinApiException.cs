using System;

namespace PasteBinSharp
{
    public class PasteBinApiException : Exception
    {
        public PasteBinApiException(string message)
            : base(message)
        {
        }
    }
}