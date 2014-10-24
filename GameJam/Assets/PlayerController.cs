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
		Release,
		Moving
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

	void FixedUpdate()
	{
		switch(State)
		{
		case EState.Moving:
			//Debug.Log ("VEL: " + rigidbody2D.velocity.magnitude);
			if(rigidbody2D.velocity.magnitude <= 0.25f)
			{
				rigidbody2D.velocity = Vector3.zero;
				rigidbody2D.angularVelocity = 0;
				State = EState.Idle;
			}
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
			Vector3 force = (_positionOnPress - positionOnRelease)*5000;
			//Debug.Log ("ADD0: " + force.ToString() + " Sleep: " + rigidbody2D.IsSleeping());

			rigidbody2D.AddForce(force);
			//Debug.Log ("ADD1: " + force.ToString() + " Sleep: " + rigidbody2D.IsSleeping());
			State = EState.Moving;
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
