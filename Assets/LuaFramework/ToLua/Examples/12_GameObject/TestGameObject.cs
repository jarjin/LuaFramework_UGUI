﻿using UnityEngine;
using System.Collections;
using LuaInterface;

public class TestGameObject: MonoBehaviour
{
    private string script =
        @"                                     
            local Color = require 'UnityEngine.Color'
            local GameObject = UnityEngine.GameObject           
            local ParticleSystem = UnityEngine.ParticleSystem                        

            local go = GameObject('go')
            go:AddComponent(typeof(ParticleSystem))
            local node = go.transform
            node.position = Vector3.one      
            print('gameObject is: '..tostring(go))                 
            GameObject.Destroy(go, 2)                  
            print('delay destroy gameobject is: '..go.name)                                           
        ";

    LuaState lua = null;

    void Start()
    {        
        lua = new LuaState();
        lua.LogGC = true;
        lua.Start();
        LuaBinder.Bind(lua);
        lua.DoString(script, "TestGameObject.cs");                     
    }

    void Update()
    {
        lua.CheckTop();
        lua.Collect();        
    }

    void OnApplicationQuit()
    {        
        lua.Dispose();
        lua = null;   
    }
}
