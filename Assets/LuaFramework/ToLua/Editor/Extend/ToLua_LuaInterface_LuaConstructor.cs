﻿using System;
using LuaInterface;

public class ToLua_LuaInterface_LuaConstructor
{
    public static string CallDefined =
@"		try
		{			
			LuaInterface.LuaConstructor obj = (LuaInterface.LuaConstructor)ToLua.CheckObject(L, 1, typeof(LuaInterface.LuaConstructor));            
			return obj.Call(L);						
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}";

    public static string DestroyDefined =
@"		try
		{
			ToLua.CheckArgsCount(L, 1);
			LuaInterface.LuaConstructor obj = (LuaInterface.LuaConstructor)ToLua.CheckObject(L, 1, typeof(LuaInterface.LuaConstructor));
			obj.Destroy();
            ToLua.Destroy(L);
			return 0;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}";

    [UseDefinedAttribute]
    public void Destroy()
    {

    }

    [UseDefinedAttribute]
    public int Call(IntPtr L)
    {
        return 0;
    }
}
