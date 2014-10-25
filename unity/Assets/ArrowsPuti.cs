using UnityEngine;
using System.Collections;

public class ArrowsPuti : MonoBehaviour {

	private bool _enabled = false;
	private SpriteRenderer[] _arrows;
	private float _timeStateChange;
	private int _id = 0;
	private const float kTimeChange = 0.5f;
	private const float kAlphaDisabled = 0f;
	private const float kAlphaEnabledOff = 0.0f;
	private const float kAlphaEnabledOn = 1.0f;


	// Use this for initialization
	void Start () 
	{
		_arrows = new SpriteRenderer[3];
		for (int i=0; i<3; i++)
		{
			_arrows[i] = transform.Find ("arrow_puti" + i).GetComponent<SpriteRenderer>();
		}
		_enabled = true;
		Enable = false;
	}

	public bool Enable
	{
		get{
			return _enabled;
		}
		set{
			if (value != _enabled)
			{
				_enabled = value;
				if (!_enabled)
				{
					Alpha (kAlphaDisabled);
				} else {
					_timeStateChange = kTimeChange;
					_id = 0;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_enabled)
		{
			_timeStateChange -= Time.deltaTime;
			if (_timeStateChange<= 0)
			{
				_timeStateChange = kTimeChange;
				_id = (_id+1)%4;
				Alpha(kAlphaEnabledOff);
				if (_id<3)
				{
					_arrows[_id].color = new Color(1, 1, 1, kAlphaEnabledOn);
				}
			}
		}
	}

	private void Alpha(float alpha)
	{
		for (int i=0; i<3; i++)
		{
			_arrows[i].color = new Color(1, 1, 1, alpha);
		}
	}
}
