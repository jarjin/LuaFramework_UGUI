using UnityEngine;
using LuaInterface;
using System.Collections;
using LuaFramework;

// 1.在LuaDLL.cs中class LuaDLL构造函数中，加入如下代码：[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)] public static extern int luaopen_mydemolibs(IntPtr L);
// 调用在libtolua.a或者tolua.bundle库中的入口函数luaopen_mydemolibs
// 2.在LuaManager.cs的OpenLibs()方法中，调用lua.OpenLibs(LuaDLL.tolua_mydemolibs);把我们的C代码中的函数注册到LuaState中
public class Test_luaInvokeC : Base {

//	string source=
//	@"
//		require 'lua_demo'
//		local v=lua_demo.add(10,21)
//		print(v)
//	";

	// Use this for initialization
	void Start () {
//		LuaState lua = new LuaState();
//		lua.Start();
//		lua.DoString(source, "Test_luaInvokeC.cs")

		// 间隔1000ms调用
		StartCoroutine(callLuaFunction());  
	}

	IEnumerator callLuaFunction(){  
		yield return new WaitForSeconds(1.0f);
		LuaFileUtils.Instance.AddSearchPath(AppConst.FrameworkRoot + "/ToLua/Examples/Test_luaInvokeC/?.lua");// 为毛必须指定到文件，指定到Test_luaInvokeC/目录下不行？
		LuaManager.DoFile("Test_luaInvokeC.lua");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
