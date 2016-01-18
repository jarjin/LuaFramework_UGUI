using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFramework.Service {
    class HttpServer : HttpService {
        Thread thread;

        public HttpServer(int port) {
            host = "http://localhost:6688/";
        }

        public void Start() {
            thread = new Thread(new ThreadStart(Listen));
            thread.Start();
        }

        public new void Stop() {
            base.Stop();
            thread.Abort();
        }

        string AssetPath {
            get {
                string exePath = Environment.CurrentDirectory;
                exePath = exePath.Replace('\\', '/');
                exePath = exePath.Substring(0, exePath.IndexOf("/Server/"));
                return exePath + "/Assets/StreamingAssets";
            }
        }

        string GetMimeType(string file) {
            string extName = Path.GetExtension(file.ToLower());
            switch (extName) {
                case ".png": return "image/png";
                case ".jpg": return "image/jpeg";
                case ".txt": return "text/plain";
                default: return "application/octet-stream";
            }
        }

        public override void OnGetRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
            string url = request.Url.ToString().Remove(0, host.Length - 1);
            if (url.Contains("?v")) {
                url = url.Substring(0, url.IndexOf("?v"));
            }
            string filename = AssetPath + url;

            string responseString = string.Empty;
            if (url.EndsWith("/")) {
                responseString = "<html><body><h1>SimpleFramework WebServer 0.1.0</h1>";
                responseString += "Current Time: " + DateTime.Now.ToString() + "<br>";
                responseString += "url : " + request.Url + "<br>";
                responseString += "Asset Path: " + filename;

                goto label;
            } else {
                if (File.Exists(filename)) {
                    using (Stream fs = File.Open(filename, FileMode.Open)) {
                        response.ContentLength64 = fs.Length;
                        response.ContentType = GetMimeType(filename) + "; charset=UTF-8";

                        fs.CopyTo(response.OutputStream);
                        response.OutputStream.Flush();
                        response.Close(); return;
                    }
                } else {
                    responseString = "<h1>404</h1>";
                    goto label;
                }
            }
            label: 
                try {
                    response.ContentLength64 = Encoding.UTF8.GetByteCount(responseString);
                    response.ContentType = "text/html; charset=UTF-8";
                } finally {
                    Stream output = response.OutputStream;
                    StreamWriter writer = new StreamWriter(output);
                    writer.Write(responseString);
                    writer.Close();
                }
        }

        public override void OnPostRequest(HttpListenerRequest request, HttpListenerResponse response) {
            Console.WriteLine("POST request: {0}", request.Url);
        }
    }
}
