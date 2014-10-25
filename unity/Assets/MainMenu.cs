using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	private UIWidget me;
	public UIButton ButtonStart;

	// Use this for initialization
	void Start () 
	{
		me = this.GetComponent<UIWidget>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Show(bool flag)
	{
		me.alpha = flag?1:0;
		ButtonStart.enabled = flag;
	}
}
