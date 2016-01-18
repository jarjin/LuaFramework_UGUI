using System;

namespace SimpleFramework.Message {
    public enum Protocal {
        //Buildin Table
        Connect           = 101,	//连接服务器
        Exception         = 102,	//异常掉线
        Disconnect        = 103,	//正常断线   
        Login 			  = 104,	//登录游戏
        Quit              = 105,	//离开游戏
        ServerTime        = 106,	//服务器时间
        HeartBeat         = 107,	//心跳数据

        //Socket Request Table
        Chat              = 1001,   //User Chat
        Friend            = 1002,   //Friend
        Mail              = 1003,   //Mail
        PK                = 1004,   //PK
        Join              = 1005,   //ComeIn
        PayBack           = 1006,   //支付消息

        //Webclient Request Table
        Register          = 2001,   //注册请求
        UserInfo          = 2002,   //用户信息
        Buy               = 2003,   //购买
        Sell              = 2004,   //卖掉
        RoleList          = 2005,   //角色列表
        ServerList        = 2006,   //服务器列表
    }

    /// <summary>
    /// 协议类型
    /// </summary>
    public enum ProtocalType : byte { 
        BINARY = 0,     //二进制
        PB_LUA = 1,     //pblua
        PBC    = 2,     //pbc
        SPROTO = 3,     //sproto
    }
}
