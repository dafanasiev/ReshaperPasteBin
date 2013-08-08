using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace PasteBinSharp
{
    public class PasteBinClient
    {
        #region Constants
        
        private const string ApiPostUrl = "http://pastebin.com/api/api_post.php";
        private const string ApiLoginUrl = "http://pastebin.com/api/api_login.php";

        #endregion

        #region Private data

        private readonly string _apiDevKey;
        private string _userName;
        private string _apiUserKey;

        #endregion

        #region Constructor

        public PasteBinClient(string apiDevKey)
        {
            if (string.IsNullOrEmpty(apiDevKey))
                throw new ArgumentNullException("apiDevKey");
            _apiDevKey = apiDevKey;
        }

        #endregion

        #region Public members

        public string UserName
        {
            get { return _userName; }
        }

        public void Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");

            var parameters = GetBaseParameters();
            parameters[ApiParameters.UserName] = userName;
            parameters[ApiParameters.UserPassword] = password;

            string resp = SendPasteBinRequest(ApiLoginUrl, parameters);

            _userName = userName;
            _apiUserKey = resp;
        }

        public void Logout()
        {
            _userName = null;
            _apiUserKey = null;
        }

        public string Paste(PasteBinEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");
            if (string.IsNullOrEmpty(entry.Text))
                throw new ArgumentException("The paste text must be set", "entry");

            var parameters = GetBaseParameters();
            parameters[ApiParameters.Option] = ApiOptions.Paste;
            parameters[ApiParameters.PasteCode] = entry.Text;
            SetIfNotEmpty(parameters, ApiParameters.PasteName, entry.Title);
            SetIfNotEmpty(parameters, ApiParameters.PasteFormat, entry.Format);
            SetIfNotEmpty(parameters, ApiParameters.PastePrivate, entry.Private ? "1" : "0");
            SetIfNotEmpty(parameters, ApiParameters.PasteExpireDate, FormatExpireDate(entry.Expiration));
            SetIfNotEmpty(parameters, ApiParameters.UserKey, _apiUserKey);

            var url = SendPasteBinRequest(ApiPostUrl, parameters);
            entry.Url = new Uri(url);
            entry.Key = ExtractKey(entry.Url);
            return url;
        }

        public IEnumerable<PasteBinListEntry> GetEntries(int maxResults = 0)
        {
            EnsureLoggedIn();
            var parameters = GetBaseParameters();
            parameters[ApiParameters.Option] = ApiOptions.List;
            parameters[ApiParameters.UserKey] = _apiUserKey;
            if (maxResults > 0)
                parameters[ApiParameters.ResultsLimit] = maxResults.ToString();

            string result = SendPasteBinRequest(ApiPostUrl, parameters);
            return ParseEntryListing(result);
        }

        public void Delete(string entryKey)
        {
            EnsureLoggedIn();
            var parameters = GetBaseParameters();
            parameters[ApiParameters.Option] = ApiOptions.Delete;
            parameters[ApiParameters.UserKey] = _apiUserKey;
            parameters[ApiParameters.PasteKey] = entryKey;

            SendPasteBinRequest(ApiPostUrl, parameters);
        }

        public PasteBinUserDetails GetUserDetails()
        {
            EnsureLoggedIn();
            var parameters = GetBaseParameters();
            parameters[ApiParameters.Option] = ApiOptions.UserDetails;
            parameters[ApiParameters.UserKey] = _apiUserKey;
            
            string result = SendPasteBinRequest(ApiPostUrl, parameters);
            XElement element = XElement.Parse(result);
            return PasteBinUserDetails.Parse(element);
        }

        #endregion

        #region Private implementation

        private static string SendPasteBinRequest(string url, NameValueCollection parameters)
        {
            using (WebClient client = new WebClient())
            {
                byte[] bytes = client.UploadValues(url, parameters);
                string resp = GetResponseText(bytes);
                if (resp.StartsWith("Bad API request"))
                    throw new PasteBinApiException(resp);
                return resp;
            }
        }

        private void EnsureLoggedIn()
        {
            if (string.IsNullOrEmpty(_apiUserKey))
                throw new InvalidOperationException("Must be logged in to list posts");
        }

        private static string FormatExpireDate(PasteBinExpiration expiration)
        {
            switch (expiration)
            {
                case PasteBinExpiration.Never:
                    return "N";
                case PasteBinExpiration.TenMinutes:
                    return "10M";
                case PasteBinExpiration.OneHour:
                    return "1H";
                case PasteBinExpiration.OneDay:
                    return "1D";
                case PasteBinExpiration.OneMonth:
                    return "1M";
                default:
                    throw new ArgumentException("Invalid expiration date");
            }
        }

        private static void SetIfNotEmpty(NameValueCollection parameters, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
                parameters[name] = value;
        }

        private NameValueCollection GetBaseParameters()
        {
            var parameters = new NameValueCollection();
            parameters[ApiParameters.DevKey] = _apiDevKey;
            
            return parameters;
        }

        private static string GetResponseText(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            using (var reader = new StreamReader(ms))
            {
                return reader.ReadToEnd();
            }
        }

        private static string ExtractKey(Uri url)
        {
            string path = url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
            return Path.GetFileName(path);
        }

        private static IEnumerable<PasteBinListEntry> ParseEntryListing(string listing)
        {
            if (listing == "No pastes found.")
                yield break;
            string xml = string.Format("<root>{0}</root>", listing);
            var root = XElement.Parse(xml);

            var pastes = root.Elements("paste");
            foreach (var p in pastes)
            {
                yield return PasteBinListEntry.Parse(p);
            }
        }

        private static class ApiParameters
        {
            public const string DevKey = "api_dev_key";
            public const string UserKey = "api_user_key";
            public const string Option = "api_option";
            public const string UserName = "api_user_name";
            public const string UserPassword = "api_user_password";
            public const string PasteKey = "api_paste_key";
            public const string PasteCode = "api_paste_code";
            public const string PasteName = "api_paste_name";
            public const string PastePrivate = "api_paste_private";
            public const string PasteFormat = "api_paste_format";
            public const string PasteExpireDate = "api_paste_expire_date";
            public const string ResultsLimit = "api_results_limit";
        }

        private static class ApiOptions
        {
            public const string Paste = "paste";
            public const string List = "list";
            public const string Delete = "delete";
            public const string UserDetails = "userdetails";
        }

        #endregion
    }
}
