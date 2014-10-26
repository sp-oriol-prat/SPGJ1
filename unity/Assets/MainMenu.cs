using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	private UIPanel me;
	public UIButton ButtonStart;
	public UILabel TitleLabel;

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

	public void OnNextLevel()
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = "Next level!";
		ButtonStart.GetComponentInChildren<UIWidget> ().color = new Color(0.1f, 1.0f, 0.1f);
		TitleLabel.enabled = false;
	}
}
