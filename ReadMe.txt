项目开源免费，求上面点星支持(star ^o^)

本框架工程基于Unity 5.0 + UGUI + tolua构建
服务器端基于VS2012及其以上版本。

有问题请加：ulua技术交流群 434341400

支持平台：PC/MAC/Android(armv7-a + Intel x86)/iOS(armv7 + arm64)/
	  WP8(SimpleFramework_WP_v0.1.1 (nlua))/

视频教程地址 http://pan.baidu.com/s/1gd8fG4N
游戏案例地址 http://www.ulua.org/showcase.html
框架详细介绍 http://doc.ulua.org/default.asp
tolua#地址： https://github.com/topameng/tolua
tolua#底层库 https://github.com/topameng/tolua_runtime

//-------------2016-02-13-------------
(1)重写资源管理器的异步加载模式，原来基于官方DEMO的版本在Lua跟c#同时请求同一份素材会存在加载BUG。

//-------------2016-01-31-------------
(1)简化框架加载流程。
(2)集成第三方库pblua\pbc\cjson\sproto等功能。
(3)整理部分框架代码。

//-------------2016-01-30-------------
(1)添加luajit2.1版本在ios下的32、64位编码器。
(2)修复加载Lua文件BUG。

//-------------2016-01-29-------------
(1)同步tolua #1.0.2版本。

//-------------2016-01-24-------------
(1)修复逻辑小bug，添加移除单击监听。

//-------------2016-01-23-------------
(1)完善了Lua的字节码模式AppConst.LuaByteMode、Lua的AssetBundle模式AppConst.LuaBundleMode的交叉使用。
(2)同步tolua #1.0.1版本。

//-------------2016-01-18-------------
(1)框架直接基于tolua#提供的luabundle功能，开关在AppConst.LuabundleMode。