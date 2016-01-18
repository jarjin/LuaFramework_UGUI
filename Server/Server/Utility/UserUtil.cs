using System;
using System.Text;
using System.Collections.Generic;
using SimpleFramework.Common;

namespace SimpleFramework.Utility {
    public class UserUtil {
        /// <summary>
        /// 添加
        /// </summary>
        public static void Add(long uid, ClientSession session) {
            Const.users.Add(uid, session);
        }

        /// <summary>
        /// 是否存在
        /// </summary>
        public static bool Exist(long uid) {
            return Const.users.ContainsKey(uid);
        }

        /// <summary>
        /// 获取
        /// </summary>
        public static ClientSession Get(long uid) {
            Dictionary<long, ClientSession> users = Const.users;
            foreach (KeyValuePair<long, ClientSession> u in Const.users) {
                if (u.Key != uid) continue;
                return u.Value;
            }
            return null;
        }

        /// <summary>
        /// 移除
        /// </summary>
        public static void Remove(long uid) {
            Const.users.Remove(uid);
        }

        /// <summary>
        /// 退出
        /// </summary>
        public static void Quit(long uid) {
            Const.users.Remove(uid);
        }
    }
}
