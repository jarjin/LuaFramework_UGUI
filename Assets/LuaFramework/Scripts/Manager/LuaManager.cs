using UnityEngine;
using System.Collections;
using LuaInterface;

namespace LuaFramework {
    public class LuaManager : Manager {
        private LuaState lua;
        private LuaLoader loader;

        // Use this for initialization
        void Awake() {
            loader = new LuaLoader();
            lua = new LuaState();
            LuaBinder.Bind(lua);
        }

        public void InitStart() {
            InitLuaPath();
            InitLuaBundle();
            this.lua.Start();    //启动LUAVM
        }

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
            lua.Dispose();
            loader = null;
        }
    }
}