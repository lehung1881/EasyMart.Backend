using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    public class GlobalConfig
    {
        static AppSettings _appSettings = null;

        public static AppSettings AppSettings { get { return _appSettings; } }

        public static void InitConfig(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }
    }
}
