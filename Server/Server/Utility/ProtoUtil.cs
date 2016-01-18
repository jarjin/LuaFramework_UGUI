using System;
using System.IO;
using System.Text;
using ProtoBuf;
using SimpleFramework.Common;

namespace SimpleFramework.Utility {
    public class ProtoUtil {

        /// <summary>
        /// 获取消息
        /// </summary>
        public static T GetMessage<T>(ByteBuffer buffer) {
            byte[] data = buffer.ReadBytes();
            using (var stream = new MemoryStream(data)) {
                return (T)Serializer.Deserialize<T>(stream);
            } 
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public static byte[] SetMessage<T>(T obj) {
            using (var stream = new MemoryStream()) {
                Serializer.Serialize(stream, obj);
                stream.Flush();
                return stream.ToArray();
            }
        }
    }
}
