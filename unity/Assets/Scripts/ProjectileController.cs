using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	private EState _state = EState.Moving;
	private Vector3 _positionOnOrigin;
	private float _timeCreation;
	public float TimeDuration = 2.0f;
	public float Force = 5000;
	private GameObject _particlesDead;

	public enum EState
	{
		Moving,
		Destroy
	}

	// Use this for initialization
	void Start () 
	{
		_timeCreation = Time.time;
		_positionOnOrigin = transform.position;
		_particlesDead = Resources.Load ("ProjectileDead") as GameObject;
	}
	
	public void Shot(Vector3 dir)
	{
		rigidbody2D.AddForce(dir * Force);
	}

	
	void FixedUpdate()
	{
		switch(State)
		{
		case EState.Moving:
			//Debug.Log ("VEL: " + rigidbody2D.velocity.magnitude);
			transform.Rotate (new Vector3(0, 0, Time.deltaTime*720));
			if(Time.time - _timeCreation > TimeDuration)
			{
				DestroyProjectile();
			}
			break;
		}
	}

	public void DestroyProjectile()
	{
		if (State != EState.Destroy)
		{
			State = EState.Destroy;
			Instantiate(_particlesDead, transform.position, Quaternion.identity);
			Destroy(gameObject);
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
		//GUI.Label(new Rect(10, 10, 400, 100), "Player: " + State);
	}
}
