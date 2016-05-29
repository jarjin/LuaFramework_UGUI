﻿using System;
using LuaInterface;

public class ToLua_LuaInterface_LuaProperty
{
    public static string GetDefined =
@"		try
		{			
			LuaInterface.LuaProperty obj = (LuaInterface.LuaProperty)ToLua.CheckObject(L, 1, typeof(LuaInterface.LuaProperty));            
            return obj.Get(L);						
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}";

    public static string SetDefined =
@"		try
		{			
            LuaInterface.LuaProperty obj = (LuaInterface.LuaProperty)ToLua.CheckObject(L, 1, typeof(LuaInterface.LuaProperty));            
            return obj.Set(L);
        }
        catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}";


    [UseDefinedAttribute]
    public int Set(IntPtr L)
    {
        return 0;
    }

    [UseDefinedAttribute]
    public int Get(IntPtr L)
    {
        return 0;
    }
}
