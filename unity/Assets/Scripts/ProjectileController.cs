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
	private ParticleSystem _fx;

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
		GameController.me.RegisterProjectile(this, true);
		_timeCreation = Time.time;
		_particlesDead = Resources.Load ("ProjectileDead") as GameObject;
		_fx = GetComponentInChildren<ParticleSystem>();
	}
	
	public void Shot(Vector3 dir)
	{
		rigidbody2D.AddForce(dir * Force);
	}

	
	void FixedUpdate()
	{
		//Mira si un boomerang entra dintre d'un foc
		if (ProjectileType == EProjectileType.Fire)
		{
			GameController.me.CheckProjectileOnRadius(transform.position, 0.6f);
		}
		switch(State)
		{
		case EState.Moving:
			//Debug.Log ("VEL: " + rigidbody2D.velocity.magnitude);
			//Diferent behaviour per type
			switch(ProjectileType)
			{
			case EProjectileType.Boomerang:
				transform.Rotate (new Vector3(0, 0, Time.deltaTime*720));
				break;
			case EProjectileType.Babosa:
				if (rigidbody2D.velocity.magnitude>0.1f)
				{
					float angle = Mathf.Atan2(rigidbody2D.velocity.y, rigidbody2D.velocity.x) * Mathf.Rad2Deg;
					transform.rotation = Quaternion.Euler (0, 0, angle);
				}
				break;
			}
			if(Time.time - _timeCreation > TimeDuration)
			{
				DestroyProjectile();
			}
			//Mira si ha tocat amb la paret de darrera
			if (rigidbody2D.velocity.x < 0 && transform.position.x < GameController.me.GetPositionPlayers().x)
			{
				DestroyProjectile();
			}
			//Mira si s'ha parat (Nomes si es babosa o boomerang)
			if (ProjectileType != EProjectileType.Fire)
			{
				//Debug.Log ("Projectile: " + rigidbody2D.velocity.magnitude);
				if (Time.time > (_timeCreation+0.2f) && rigidbody2D.velocity.magnitude <= 1.5f)
				{
					DestroyProjectile();
				}
			}
			break;
		}
	}

	public void DestroyProjectile()
	{
		if (State != EState.Destroy)
		{
			//Debug.Log ("Projectile Destroyed!");
			GameController.me.RegisterProjectile(this, false);
			State = EState.Destroy;
			if (ProjectileType == EProjectileType.Boomerang)
			{
				Instantiate(_particlesDead, transform.position, Quaternion.identity);
			}
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
				baseDamage = 0;
				enemy.Escupit();
				break;
			case EProjectileType.Boomerang:
				baseDamage = 1;
				enemy.Hit(baseDamage, isFrontHit, _isOnFire);
				break;
			case EProjectileType.Fire:
				baseDamage = 1;
				break;
			}
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
			Debug.Log("Boomerang on Fire");
			_isOnFire = value;
			if (_fx != null)
			{
				_fx.Play();
			}
		}
	}

	void OnGUI()
	{
		//GUI.Label(new Rect(10, 10, 400, 100), "Player: " + State);
	}
}
