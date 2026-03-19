namespace BASE.Service.Core.Model
{
    public class GlobalConfig
    {
        private static AppSettings _appSettings = null;

        public static AppSettings AppSettings { get { return _appSettings; } }

        public static void InitConfig(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
    }
}
