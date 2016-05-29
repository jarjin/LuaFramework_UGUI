﻿using UnityEngine;
using System.Collections;
using LuaInterface;

//两套协同勿交叉使用，类unity原生，大量使用效率低
public class TestCoroutine2 : LuaClient 
{
    string script =
    @"
        function CoExample()            
            WaitForSeconds(2)
            print('WaitForSeconds end time: '.. UnityEngine.Time.time)
            WaitForFixedUpdate()
            print('WaitForFixedUpdate end frameCount: '..UnityEngine.Time.frameCount)
            WaitForEndOfFrame()
            print('WaitForEndOfFrame end frameCount: '..UnityEngine.Time.frameCount)
            Yield(null)
            print('yield null end frameCount: '..UnityEngine.Time.frameCount)
            Yield(0)
            print('yield(0) end frameCime: '..UnityEngine.Time.frameCount)
            local www = UnityEngine.WWW('http://www.baidu.com')
            Yield(www)
            print('yield(www) end time: '.. UnityEngine.Time.time)
            local s = tolua.tolstring(www.bytes)
            print(s:sub(1, 128))
            print('coroutine over')
        end

        function TestCo()            
            StartCoroutine(CoExample)                                   
        end

        local coDelay = nil

        function Delay()
	        local c = 1

	        while true do
		        WaitForSeconds(1) 
		        print('Count: '..c)
		        c = c + 1
	        end
        end

        function StartDelay()
	        coDelay = StartCoroutine(Delay)            
        end

        function StopDelay()
	        StopCoroutine(coDelay)
        end
    ";

    protected override void OnLoadFinished()
    {
        base.OnLoadFinished();

        luaState.DoString(script);
        LuaFunction func = luaState.GetFunction("TestCo");
        func.Call();
        func.Dispose();
        func = null;
    }

    //屏蔽，例子不需要运行
    protected override void CallMain() { }

    void OnGUI()
    {
        if (GUI.Button(new Rect(50, 50, 120, 45), "Start Counter"))
        {
            LuaFunction func = luaState.GetFunction("StartDelay");
            func.Call();
            func.Dispose();
        }
        else if (GUI.Button(new Rect(50, 150, 120, 45), "Stop Counter"))
        {
            LuaFunction func = luaState.GetFunction("StopDelay");
            func.Call();
            func.Dispose();
        }
    }
}
