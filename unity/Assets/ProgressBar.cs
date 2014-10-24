﻿using UnityEngine;
using System.Collections;

public class ProgressBar : MonoBehaviour {

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
		Bar.width = Mathf.Clamp((int)(value * (float)Background.width), 0, Background.width);
	}
	
	public void SetProgress(float value, float valueMax)
	{
		Bar.width = Mathf.Clamp((int)((value / valueMax) * (float)Background.width), 0, Background.width);
	}
}
