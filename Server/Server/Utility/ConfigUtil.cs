using System;
using System.IO;
using System.Text;
using System.Configuration;
using SimpleFramework.Common;

namespace SimpleFramework.Utility {
    class ConfigUtil {
        public static void LoadConfig() {
            Const.RedisHost = GetValue("RedisHost");
            Const.RedisPort = Int32.Parse(GetValue("RedisPort"));
            Const.RedisSaveTime = Int32.Parse(GetValue("RedisSaveTime"));

            Const.WebUrl = GetValue("WebUrl");  //请求的URL
            Const.ZeromqUri = GetValue("ZeromqUri");    //ZeroMQ
        }

        public static string GetValue(string key) {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
