using UnityEngine;
using System.Collections;

namespace LuaFramework {

    /// <summary>
    /// </summary>
    public class Main : MonoBehaviour {

        void Start()
        {
            AppFacade.Instance.StartUp();   //启动游戏
        }

        void Update()
        {
            Util.CallMethod("Game", "Update");     //初始化完成
        }
    }
}