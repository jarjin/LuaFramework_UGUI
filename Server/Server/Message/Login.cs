using System;
using System.Collections.Generic;
using login;
using SimpleFramework.Common;
using SimpleFramework.Utility;
using Sproto;
using SprotoType;

namespace SimpleFramework.Message {
    class Login : IMessage {
        public void OnMessage(ClientSession session, ByteBuffer buffer) {
            byte b = buffer.ReadByte();
            ProtocalType type = (ProtocalType)b;    //协议类型
            switch (type) {
                case ProtocalType.BINARY: 
                    OnBinaryMessage(session, buffer);
                break;
                case ProtocalType.PB_LUA:
                    OnPbLuaMessage(session, buffer);
                break;
                case ProtocalType.PBC:
                    OnPbcMessage(session, buffer);
                break;
                case ProtocalType.SPROTO:
                    OnSprotoMessage(session, buffer);
                break;
            }
        }

        /// <summary>
        /// 二进制消息
        /// </summary>
        void OnBinaryMessage(ClientSession session, ByteBuffer buffer) {
            string str = buffer.ReadString();
            session.uid = buffer.ReadInt();

            ByteBuffer newBuffer = new ByteBuffer();
            newBuffer.WriteByte((byte)ProtocalType.BINARY);
            newBuffer.WriteString(str);
            SocketUtil.SendMessage(session, Protocal.Login, newBuffer);

            UserUtil.Add(session.uid, session);
            Console.WriteLine("OnBinaryMessage--->>>" + str + session.uid);
        }

        /// <summary>
        /// pblua消息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="buffer"></param>
        void OnPbLuaMessage(ClientSession session, ByteBuffer buffer) {
            LoginRequest request = ProtoUtil.GetMessage<LoginRequest>(buffer);
            Console.WriteLine("OnPbLuaMessage id=>>" + request.id + " name:>>>" + request.name + " email:>>" + request.email);
            buffer.Close(); buffer = null;

            LoginResponse response = new LoginResponse();
            response.id = 100; //排队人数
            byte[] data = ProtoUtil.SetMessage<LoginResponse>(response);

            ByteBuffer newBuffer = new ByteBuffer();
            newBuffer.WriteByte((byte)ProtocalType.PB_LUA);
            newBuffer.WriteBytes(data); //添加数据

            SocketUtil.SendMessage(session, Protocal.Login, newBuffer);
        }

        /// <summary>
        /// pbc消息
        /// </summary>
        void OnPbcMessage(ClientSession session, ByteBuffer buffer) {
            tutorial.Person request = ProtoUtil.GetMessage<tutorial.Person>(buffer);
            Console.WriteLine("OnPbcMessage id=>>" + request.id + " name:>>>" + request.name);
            buffer.Close(); buffer = null;

            byte[] data = ProtoUtil.SetMessage<tutorial.Person>(request);

            ByteBuffer newBuffer = new ByteBuffer();
            newBuffer.WriteByte((byte)ProtocalType.PBC);
            newBuffer.WriteBytes(data); //添加数据

            SocketUtil.SendMessage(session, Protocal.Login, newBuffer);
        }

        /// <summary>
        /// sproto消息
        /// </summary>
        void OnSprotoMessage(ClientSession session, ByteBuffer buffer) {
            byte[] data = buffer.ReadBytes();
            //-------------------------------------------------------------
            SprotoPack spack = new SprotoPack();
            byte[] pack_data = spack.pack(data);             // pack
            byte[] unpack_data = spack.unpack(pack_data);     // unpack

            AddressBook addr = new AddressBook(unpack_data);   // decode
            Console.WriteLine("OnSprotoMessage id=>>" + addr.person.Count);
            buffer.Close(); buffer = null;

            //-------------------------------------------------------------
            ByteBuffer newBuffer = new ByteBuffer();
            newBuffer.WriteByte((byte)ProtocalType.SPROTO);
            newBuffer.WriteBytes(data); //添加数据
            SocketUtil.SendMessage(session, Protocal.Login, newBuffer);
        }
    }
}
