using System;
using System.IO;
using System.Net;
using System.Text;
using SimpleFramework.Common;

namespace SimpleFramework.Utility {
    class HttpUtil {
        static string url = string.Empty;

        /// <summary>
        /// 发送HTTP请求
        /// </summary>
        public static string SendRequest(string data) {
            url = Const.WebUrl;
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "post";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/x-www-form-urlencoded";
            byte[] buffer = encoding.GetBytes(data);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("UTF-8"))) {
                return reader.ReadToEnd();
            }
        }
    }
}
