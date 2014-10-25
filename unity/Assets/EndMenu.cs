using UnityEngine;
using System.Collections;

public class EndMenu : MonoBehaviour 
{
	private UIWidget me;
	public UIButton ButtonStart;

	// Use this for initialization
	void Start () 
	{
		me = this.GetComponent<UIWidget>();
		Show (false);
	}

	public void Show(bool flag)
	{
		me.alpha = flag?1:0;
		ButtonStart.enabled = flag;
	}
}
