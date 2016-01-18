using System;
using System.Text;
using System.Threading.Tasks;
using SimpleFramework.Common;
using SimpleFramework.Utility;

namespace SimpleFramework.Timer {
    class RedisTimer {
        System.Timers.Timer timer = null;

        public RedisTimer() {
            timer = new System.Timers.Timer(Const.RedisSaveTime * 60 * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimeUpdate);
            timer.AutoReset = true;
        }

        /// <summary>
        /// 启动计时器
        /// </summary>
        public void Start() {
            timer.Enabled = true; //是否触发Elapsed事件
            timer.Start();
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop() {
            timer.Close();
        }

        /// <summary>
        /// 计时器更新
        /// </summary>
        private void OnTimeUpdate(object sender, System.Timers.ElapsedEventArgs e) {
            RedisUtil.SaveNow();
            Console.WriteLine("OnSaveNow--->>>" + Const.RedisSaveTime);
        }
    }
}
