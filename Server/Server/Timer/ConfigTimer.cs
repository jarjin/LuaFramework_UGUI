using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleFramework.Utility;

namespace SimpleFramework.Timer {
    class ConfigTimer {
        int time = 30;
        System.Timers.Timer timer = null;

        public ConfigTimer() {
            ConfigUtil.LoadConfig();    //上来读取一次

            timer = new System.Timers.Timer(time * 60 * 1000);
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
            ConfigUtil.LoadConfig();
            Console.WriteLine("OnConfigNow--->>>" + time);
        }
    }
}
