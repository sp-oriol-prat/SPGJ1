using UnityEngine;
using System.Collections;

public class RadialBar : MonoBehaviour {

	public UISprite Bar;
	public UISprite Background;
	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
	}

	public void SetProgress(float value)
	{
		Bar.fillAmount = value;
	}
	
	public void SetProgress(float value, float valueMax)
	{
		Bar.fillAmount = value / valueMax;
	}
}
