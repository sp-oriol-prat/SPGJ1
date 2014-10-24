using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private EState _state = EState.Idle;
	private Vector3 _positionOnPress;
	public float Friction = 0.8f;

	public enum EState
	{
		Idle,
		//Press,
		Drag,
		Release
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch(State)
		{
		case EState.Drag:
			//rigidbody2D.MovePosition(GameController.me.GetPosMouse3D);
			break;
		}
	}
	
	void OnMouseDown() 
	{
		if (State == EState.Idle)
		{
			State = EState.Drag;
			_positionOnPress = GameController.me.GetPosMouse3D;
		}
	}
	
	void OnMouseUp() 
	{
		if (State == EState.Drag)
		{
			Vector3 positionOnRelease = GameController.me.GetPosMouse3D;
			Vector3 force = (_positionOnPress - positionOnRelease);
			GameObject projectileRes = Resources.Load ("Projectile") as GameObject;
			GameObject projectileGo = Instantiate(projectileRes, transform.position, Quaternion.identity) as GameObject;
			projectileGo.GetComponent<ProjectileController>().Shot(force);
			State = EState.Idle;
		}
	}

	public EState State
	{
		get
		{
			return _state;
		}
		set
		{
			_state = value;
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(10, 10, 400, 100), "Player: " + State);
	}
}
