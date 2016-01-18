using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleFramework.Common;
using SimpleFramework.Message;
using SuperSocket.SocketBase.Protocol;

namespace SimpleFramework.Utility {
    class SocketUtil {
        static SocketUtil socket;
        public static SocketUtil instance {
            get {
                if (socket == null)
                    socket = new SocketUtil();
                return socket;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public static void SendMessage(ClientSession session, Protocal protocal, ByteBuffer buffer) {
            byte[] message = buffer.ToBytes();
            using (MemoryStream ms = new MemoryStream()) {
                ms.Position = 0;
                BinaryWriter writer = new BinaryWriter(ms);
                ushort protocalId = (ushort)protocal;
                ushort msglen = (ushort)(message.Length + 2);
                writer.Write(msglen);
                writer.Write(protocalId); 
                writer.Write(message);
                writer.Flush();
                if (session != null && session.Connected) {
                    byte[] payload = ms.ToArray();
                    session.Send(payload, 0, payload.Length);
                } else {
                    Console.WriteLine("client.connected----->>false");
                }
            }
        }

        /// <summary>
        /// 客户端连接
        /// </summary>
        /// <param name="session"></param>
        public void OnSessionConnected(ClientSession session) {
            Console.WriteLine("OnSessionConnected--->>>" + session.RemoteEndPoint.Address);
        }

        /// <summary>
        /// 数据接收
        /// </summary>
        public void OnRequestReceived(ClientSession session, BinaryRequestInfo requestInfo) {
            ByteBuffer buffer = new ByteBuffer(requestInfo.Body);
            int commandId = buffer.ReadShort();
            Protocal c = (Protocal)commandId;
            string className = "SimpleFramework.Message." + c;
            Console.WriteLine("OnRequestReceived--->>>" + className);

            Type t = Type.GetType(className);
            IMessage obj = (IMessage)Activator.CreateInstance(t);
            if (obj != null) obj.OnMessage(session, buffer);
            obj = null; t = null;   //释放内存
        }
    }
}
