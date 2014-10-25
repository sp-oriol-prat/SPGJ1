using UnityEngine;
using System.Collections;

public class DestroyDelay : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Destroy(this.gameObject, 1.0f);
	}
}
