using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protobuilder {
    class Program {
        static List<string> paths = new List<string>();
        static List<string> files = new List<string>();

        /// <summary>
        /// 遍历目录及其子目录
        /// </summary>
        static void Recursive(string path) {
            string[] names = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);
            foreach (string filename in names) {
                string ext = Path.GetExtension(filename);
                if (ext.Equals(".meta")) continue;
                files.Add(filename.Replace('\\', '/'));
            }
            foreach (string dir in dirs) {
                paths.Add(dir.Replace('\\', '/'));
                Recursive(dir);
            }
        }

        static void ExecuteOne(string proc, string args, string dir) {
            Console.WriteLine(proc + " " + args);

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = proc;
            info.Arguments = args;
            info.WindowStyle = ProcessWindowStyle.Minimized;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }

        static void Main(string[] args) {
            files.Clear();
            string dir = System.Environment.CurrentDirectory;
            paths.Clear(); files.Clear(); Recursive(dir);

            string protoc = "d:/protobuf-2.4.1/src/protoc.exe";
            string protogen = "D:/protobuf-net/ProtoGen/protogen.exe";

            Console.WriteLine("Starting Build Proto File...");
            foreach (string f in files) {
                string name = Path.GetFileName(f);
                string ext = Path.GetExtension(f);
                string prefix = name.Replace(ext, string.Empty);

                if (!ext.Equals(".proto")) continue;

                //------编译bin----------
                string argstr = " --descriptor_set_out=" + prefix + ".bin --proto_path=\"./\" --include_imports " + name;
                ExecuteOne(protoc, argstr.ToLower(), dir);

                //------编译cs----------
                argstr = " -i:" + prefix + ".bin -o:" + prefix + "_pb.cs";
                ExecuteOne(protogen, argstr.ToLower(), dir);
            }
        }
    }
}
