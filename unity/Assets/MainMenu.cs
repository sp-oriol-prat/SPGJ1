using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	private UIPanel me;
	public UIButton ButtonStart;

	// Use this for initialization
	void Start () 
	{
		me = this.GetComponent<UIPanel>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Show(bool flag)
	{
		me.alpha = flag?1:0;
		ButtonStart.enabled = flag;
	}

	public void SetStartButton(string text, Color col)
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = text;
		ButtonStart.GetComponentInChildren<UIWidget> ().color = col;
	}
}
