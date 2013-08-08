using System;
using System.Xml.Linq;

namespace PasteBinSharp
{
    public class PasteBinUserDetails
    {
        public string Name { get; set; }
        public string Format { get; set; }
        public string Expiration { get; set; }
        public Uri AvatarUrl { get; set; }
        public int Watching { get; set; }
        public int Watchers { get; set; }
        public int TotalPastes { get; set; }
        public bool Private { get; set; }
        public Uri WebsiteUrl { get; set; }
        public string EmailAddress { get; set; }
        public string Location { get; set; }
        public int AccountType { get; set; }
        public string Biography { get; set; }

        public static PasteBinUserDetails Parse(XElement element)
        {
            var details = new PasteBinUserDetails();
            details.Name = (string)element.Element("user_name");
            details.Format = (string)element.Element("user_format_short");
            details.Expiration = (string)element.Element("user_expiration");
            string avatarUrl = (string)element.Element("user_avatar_url");
            if (!string.IsNullOrEmpty(avatarUrl))
                details.AvatarUrl = new Uri(avatarUrl);
            details.Watching = (int)element.Element("user_watching");
            details.Watchers = (int)element.Element("user_watchers");
            details.TotalPastes = (int)element.Element("user_total_pastes");
            details.Private = (bool)element.Element("user_private");
            string websiteUrl = (string)element.Element("user_website");
            if (!string.IsNullOrEmpty(websiteUrl))
                details.WebsiteUrl = new Uri(websiteUrl);
            details.EmailAddress = (string)element.Element("user_email");
            details.Location = (string)element.Element("user_location");
            details.AccountType = (int)element.Element("user_account_type");
            details.Biography = (string)element.Element("user_biography");
            return details;
        }
    }
}
