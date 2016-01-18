using System;
using System.Text;
using SimpleFramework.Timer;
using SimpleFramework.Utility;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase.Protocol;

namespace SimpleFramework {
    class GameServer : AppServer<ClientSession, BinaryRequestInfo> {

        public GameServer() : base(new DefaultReceiveFilterFactory<ClientReceiveFilter, BinaryRequestInfo>()) {
        }

        protected override bool Setup(IRootConfig rootConfig, IServerConfig config) {
            return base.Setup(rootConfig, config);
        }

        protected override void OnStarted() {
            base.OnStarted();
            ServerUtil.instance.Init();
            
            this.NewSessionConnected += new SessionHandler<ClientSession>(OnSessionConnected);
            this.NewRequestReceived += new RequestHandler<ClientSession, BinaryRequestInfo>(OnRequestReceived);
        }

        protected override void OnStopped() {
            base.OnStopped();
            ServerUtil.instance.Close();

            this.NewSessionConnected -= new SessionHandler<ClientSession>(OnSessionConnected);
            this.NewRequestReceived -= new RequestHandler<ClientSession, BinaryRequestInfo>(OnRequestReceived);
        }

        /// <summary>
        /// Session连接
        /// </summary>
        void OnSessionConnected(ClientSession session) {
            SocketUtil.instance.OnSessionConnected(session);
        }

        /// <summary>
        /// 数据接收
        /// </summary>
        void OnRequestReceived(ClientSession session, BinaryRequestInfo requestInfo) {
            SocketUtil.instance.OnRequestReceived(session, requestInfo);
        }
    }
}
