using UnityEngine;
using System.Collections;

public class EndMenu : MonoBehaviour 
{
	private UIWidget me;
	public UIButton ButtonStart;
	public UILabel label;

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

	public void OnRepeatLevel()
	{
		ButtonStart.GetComponentInChildren<UILabel> ().text = "Try again";
		label.text = "Game Over..";
	}

	public void OnEndGame()
	{
		//ButtonStart.GetComponentInChildren<UILabel> ().text = "Try again";
		//label.text = "Game Over..";
	}
}
