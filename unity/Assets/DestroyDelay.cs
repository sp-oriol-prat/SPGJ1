using UnityEngine;
using System.Collections;

public class DestroyDelay : MonoBehaviour {

	public float Time = 1.0f;

	// Use this for initialization
	void Start () 
	{
		Destroy(this.gameObject, Time);
	}
}
