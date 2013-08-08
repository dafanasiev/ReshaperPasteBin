using JetBrains.Annotations;
using JetBrains.Application;
using JetBrains.Application.Communication;
using JetBrains.Application.DataContext;
using JetBrains.Application.Settings;
using PasteBinSharp;


namespace Pastebin
{
    [ShellComponent]
    public class PastebinService
    {
        private readonly WebProxySettingsReader myProxySettingsReader;
        private readonly ISettingsStore mySettingsStore;

        public PastebinService(WebProxySettingsReader proxySettingsReader, ISettingsStore settingsStore)
        {
            myProxySettingsReader = proxySettingsReader;
            mySettingsStore = settingsStore;
        }

        public PasteBinClient GetClient([NotNull] IDataContext context)
        {
            var boundSettings = mySettingsStore.BindToContextTransient(ContextRange.Smart((lt, _) => context));

            //var proxy = myProxySettingsReader.GetProxySettings(boundSettings);    //TODO: надо научиться работать и с прокси сервером
            var settings = boundSettings.GetKey<PastebinSettings>(SettingsOptimization.DoMeSlowly);

            var client = new PasteBinClient(settings.ApiKey);
            
            return client;
        }
    }
}