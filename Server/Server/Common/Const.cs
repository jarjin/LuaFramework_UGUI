using System;
using System.Text;
using System.Collections.Generic;

namespace SimpleFramework.Common {
    class Const {
        public static int RedisPort;        //Redis Port
        public static string RedisHost;     //Redis Address
        public static int RedisSaveTime;    //Save Time

        public static string WebUrl;        //HttpUrl
        public static string ZeromqUri;     //ZeromqUri;

        public static Dictionary<long, ClientSession> users;
    }
}
