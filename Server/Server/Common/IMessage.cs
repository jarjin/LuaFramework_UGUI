using System;

namespace SimpleFramework.Common {
    public interface IMessage {
        void OnMessage(ClientSession session, ByteBuffer buffer);
    }
}
