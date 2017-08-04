using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 测试新建C#文件GdTest.cs，然后在以字符串形式调用的lua代码中，直接调用C#的静态函数gdPrint()
// 需要在CustomSettings.cs文件中添加要导出注册到lua的类型列表(GdTest),使用函数_GT(typeof(GdTest))
// 需要清除并重新生成C#文件所对应的wrap文件
public class GdTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public static void gdPrint()
	{
		Debug.Log("GdTest print OK!!!!!!!!!!");
	}

	// Update is called once per frame
	void Update () {
		
	}
}
