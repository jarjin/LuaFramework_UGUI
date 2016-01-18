using System;
using System.Text;
using SimpleFramework.Common;
using Sider;

namespace SimpleFramework.Utility {
    class RedisUtil {
        RedisClient client;
        static RedisUtil redis;

        static RedisUtil instance {
            get {
                if (redis == null)
                    redis = new RedisUtil();
                return redis;
            }
        }

        public RedisUtil() {
            int dbport = Const.RedisPort;
            string dbaddr = Const.RedisHost;
            client = new RedisClient(dbaddr, dbport); // custom host/port
        }

        /// <summary>
        /// 设置
        /// </summary>
        public static void Set(string key, string value) {
            instance.client.Set(key, value);
        }

        /// <summary>
        /// 获取
        /// </summary>
        public static string Get(string key) {
            return instance.client.Get(key);
        }

        /// <summary>
        /// 立即保存
        /// </summary>
        public static void SaveNow() {
            instance.client.BgSave();
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public static void Close() {
            if (instance.client != null) {
                instance.client.Dispose();
            }
        }
    }
}
