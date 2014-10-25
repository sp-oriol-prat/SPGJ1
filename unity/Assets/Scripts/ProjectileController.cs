using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	private EState _state = EState.Moving;
	private float _timeCreation;
	public float TimeDuration = 2.0f;
	public float Force = 5000;
	private GameObject _particlesDead;
	public EProjectileType ProjectileType;
	private bool _isOnFire = false;

	public enum EProjectileType
	{
		Boomerang,
		Fire,
		Babosa
	}

	public enum EState
	{
		Moving,
		Destroy
	}

	// Use this for initialization
	void Start () 
	{
		_timeCreation = Time.time;
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

	void OnCollisionEnter2D(Collision2D collision) 
	{
		EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
		if (enemy != null)
		{
			bool isFrontHit = rigidbody2D.velocity.x > 0.0f;

			int baseDamage = 1;
			switch ( ProjectileType )
			{
			case EProjectileType.Babosa:
				baseDamage = 1;
				break;
			case EProjectileType.Boomerang:
				baseDamage = 1;
				break;
			case EProjectileType.Fire:
				baseDamage = 1;
				break;
			}

			enemy.Hit(baseDamage, isFrontHit, _isOnFire);
			DestroyProjectile();
		}
	}

	public bool IsOnFire
	{
		get 
		{
			return _isOnFire;
		}
		set
		{
			_isOnFire = value;
		}
	}

	void OnGUI()
	{
		//GUI.Label(new Rect(10, 10, 400, 100), "Player: " + State);
	}
}
