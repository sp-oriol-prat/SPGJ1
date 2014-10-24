using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject PlayerRef;
	public Camera CameraRef;
	public static GameController me;
	private Plane _groundPlane;

	// Use this for initialization
	void Start () {
		me = this;
		_groundPlane = new Plane(Vector3.back, Vector3.zero);
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public Vector3 GetPosMouse3D
	{
		get
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float rayDistance;
			Vector3 posMouse3D = Vector3.zero;
			if (_groundPlane.Raycast(ray, out rayDistance))
			{
				posMouse3D = ray.GetPoint(rayDistance);
			}
			return posMouse3D;
		}
	}
}
