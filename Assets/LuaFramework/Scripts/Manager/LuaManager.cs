using UnityEngine;
using System.Collections;
using LuaInterface;

namespace LuaFramework {
    public class LuaManager : Manager {
        private LuaState lua;
        private LuaLoader loader;

        private LuaFunction updateFunc = null;
        private LuaFunction lateUpdateFunc = null;
        private LuaFunction fixedUpdateFunc = null;

        public LuaEvent UpdateEvent {
            get;
            private set;
        }

        public LuaEvent LateUpdateEvent {
            get;
            private set;
        }

        public LuaEvent FixedUpdateEvent {
            get;
            private set;
        }

        // Use this for initialization
        void Awake() {
            loader = new LuaLoader();
            lua = new LuaState();
            this.InitLuaLibrary();
            LuaBinder.Bind(lua);
            LuaCoroutine.Register(lua, this);
        }

        public void InitStart() {
            InitLuaPath();
            InitLuaBundle();
            this.lua.Start();    //启动LUAVM

            lua.DoFile("Main.lua");

            updateFunc = lua.GetFunction("Update");
            lateUpdateFunc = lua.GetFunction("LateUpdate");
            fixedUpdateFunc = lua.GetFunction("FixedUpdate");

            LuaFunction main = lua.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;

            UpdateEvent = GetEvent("UpdateBeat");
            LateUpdateEvent = GetEvent("LateUpdateBeat");
            FixedUpdateEvent = GetEvent("FixedUpdateBeat");     
        }
        
        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        void InitLuaLibrary() {
            lua.OpenLibs(LuaDLL.luaopen_pb);      
            lua.OpenLibs(LuaDLL.luaopen_sproto_core);
            lua.OpenLibs(LuaDLL.luaopen_protobuf_c);
            lua.OpenLibs(LuaDLL.luaopen_lpeg);
            lua.OpenLibs(LuaDLL.luaopen_cjson);
            lua.OpenLibs(LuaDLL.luaopen_cjson_safe);
            lua.OpenLibs(LuaDLL.luaopen_bit);
            lua.OpenLibs(LuaDLL.luaopen_socket_core);
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        void InitLuaPath() {
            if (AppConst.DebugMode) {
                string rootPath = AppConst.FrameworkRoot;
                lua.AddSearchPath(rootPath + "/Lua");
                lua.AddSearchPath(rootPath + "/ToLua/Lua");
            } else {
                lua.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        void InitLuaBundle() {
            if (loader.beZip) {
                loader.AddBundle("Lua/Lua.unity3d");
                loader.AddBundle("Lua/Lua_math.unity3d");
                loader.AddBundle("Lua/Lua_system.unity3d");
                loader.AddBundle("Lua/Lua_u3d.unity3d");
                loader.AddBundle("Lua/Lua_Common.unity3d");
                loader.AddBundle("Lua/Lua_Logic.unity3d");
                loader.AddBundle("Lua/Lua_View.unity3d");
                loader.AddBundle("Lua/Lua_Controller.unity3d");
                loader.AddBundle("Lua/Lua_Misc.unity3d");
            }
        }

        void Update() {
            if (updateFunc != null) {
                updateFunc.BeginPCall(TracePCall.Ignore);
                updateFunc.Push(Time.deltaTime);
                updateFunc.Push(Time.unscaledDeltaTime);
                updateFunc.PCall();
                updateFunc.EndPCall();
            }
            lua.Collect();
#if UNITY_EDITOR
            lua.CheckTop();
#endif
        }

        void LateUpdate() {
            if (lateUpdateFunc != null) {
                lateUpdateFunc.BeginPCall(TracePCall.Ignore);
                lateUpdateFunc.PCall();
                lateUpdateFunc.EndPCall();
            }
        }

        void FixedUpdate() {
            if (fixedUpdateFunc != null) {
                fixedUpdateFunc.BeginPCall(TracePCall.Ignore);
                fixedUpdateFunc.Push(Time.fixedDeltaTime);
                fixedUpdateFunc.PCall();
                fixedUpdateFunc.EndPCall();
            }
        }

        LuaEvent GetEvent(string name) {
            LuaTable table = lua.GetTable(name);
            LuaEvent e = new LuaEvent(table);
            table.Dispose();
            table = null;
            return e;
        }

        void SafeRelease(ref LuaFunction luaRef) {
            if (luaRef != null) {
                luaRef.Dispose();
                luaRef = null;
            }
        }

        public object[] DoFile(string filename) {
            return lua.DoFile(filename);
        }

        // Update is called once per frame
        public object[] CallFunction(string funcName, params object[] args) {
            LuaFunction func = lua.GetFunction(funcName);
            if (func != null) {
                return func.Call(args);
            }
            return null;
        }

        public void LuaGC() {
            lua.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        public void Close() {
            SafeRelease(ref updateFunc);
            SafeRelease(ref lateUpdateFunc);
            SafeRelease(ref fixedUpdateFunc);

            if (UpdateEvent != null) {
                UpdateEvent.Dispose();
                UpdateEvent = null;
            }

            if (LateUpdateEvent != null) {
                LateUpdateEvent.Dispose();
                LateUpdateEvent = null;
            }

            if (FixedUpdateEvent != null) {
                FixedUpdateEvent.Dispose();
                FixedUpdateEvent = null;
            }
            lua.Dispose();
            lua = null;
            loader = null;
        }
    }
}