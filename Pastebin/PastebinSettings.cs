using JetBrains.Application.Communication;
using JetBrains.Application.Settings;


namespace Pastebin
{
    /// <summary>
    /// settings.
    /// </summary>
    [SettingsKey(typeof(InternetSettings), "Pastebin settings")]
    public class PastebinSettings
    {
        [SettingsEntry("", "Pastebin API key")]
        public string ApiKey { get; set; }
    }

}