using UnityEngine;
using System.Collections;

public class TestMenu : MonoBehaviour 
{
	private UIWidget me;
	public UIButton ButtonStart;
	public UILabel WaveLabel;

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

	public void Wave(int idWave)
	{
		WaveLabel.text = "Wave " + idWave;
		WaveLabel.transform.localScale = new Vector3(5, 1, 1);
		GoTweenConfig config = new GoTweenConfig();
		config.setEaseType(GoEaseType.ElasticOut);
		config.addTweenProperty( new ScaleTweenProperty(Vector3.one));
		GoTween tweenSpawn = new GoTween( WaveLabel.transform, 0.6f, config );
        Go.addTween( tweenSpawn );
	}

	public void Message(string msg)
	{
		WaveLabel.text = msg;
		WaveLabel.transform.localScale = new Vector3(5, 1, 1);
		GoTweenConfig config = new GoTweenConfig();
		config.setEaseType(GoEaseType.ElasticOut);
		config.addTweenProperty( new ScaleTweenProperty(Vector3.one));
		GoTween tweenSpawn = new GoTween( WaveLabel.transform, 0.6f, config );
		Go.addTween( tweenSpawn );
	}
}
