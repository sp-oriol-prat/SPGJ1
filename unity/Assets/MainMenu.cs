using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour 
{
	private UIPanel me;
	public UIButton ButtonStart;
	public UILabel SubLabel;

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

	public void OnStartGame()
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = "Start!";
		SubLabel.enabled = false;
	}

	public void OnNextLevel()
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = "Next level!";
		//ButtonStart.GetComponentInChildren<UIWidget> ().color = new Color(0.1f, 1.0f, 0.1f);
		SubLabel.enabled = false;
	}

	public void OnRepeatLevel()
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = "Try again";
		//ButtonStart.GetComponentInChildren<UIWidget> ().color = new Color(1.0f, 0.2f, 0.2f);
		SubLabel.enabled = true;
		SubLabel.text = "Game Over...";
	}

	public void OnEndedGame()
	{
		ButtonStart.GetComponentInChildren<UILabel>().text = "Play Again!";
		//ButtonStart.GetComponentInChildren<UIWidget> ().color = new Color(0.0f, 1.0f, 1.0f);
		SubLabel.enabled = true;
		SubLabel.text = "Game Completed!";
	}
}
