using UnityEngine;
using System.Collections;
using SimpleJSON;

public class ProjectileController : MonoBehaviour {

	private EState _state = EState.Moving;
	private float _timeCreation;
	public float TimeDuration = 2.0f;
	public float Force = 5000;
	private GameObject _particlesDead;
	public EProjectileType ProjectileType;
	private bool _isOnFire = false;
	private ParticleSystem _fx;
	private float _baseDamage;
	private CircleCollider2D _collider2D;
	private bool _isStopped = false;
	private float _stopMotionTime;
	private bool _firstUpdate = true;

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

	class WeaponsParams
	{
		public class PhysicsParams {
			public float force;
			public float mass;
			public float drag;
		};

		public int boomerangDamage;
		public float babosaGlueTime;
		public float babosaFrictionFactor;
		public float fireDamageBoost;
		public float fireGlueTime;
		public float fireStopVel;
		public PhysicsParams boomerangPhysics;
		public PhysicsParams firePhysics;
		public PhysicsParams babosaPhysics;
	};
	static WeaponsParams _weaponsParams = null;



	// Use this for initialization
	void Start () 
	{
		_collider2D = (CircleCollider2D)collider2D;
		GameController.me.RegisterProjectile(this, true);
		_timeCreation = Time.time;
		_particlesDead = Resources.Load ("ProjectileDead") as GameObject;
		_fx = GetComponentInChildren<ParticleSystem>();
	}

	public void applyParams()
	{
		switch ( ProjectileType )
		{
		case EProjectileType.Babosa:
			_baseDamage = 0;
			applyPhysicParams(_weaponsParams.babosaPhysics);
			break;
		case EProjectileType.Boomerang:
			_baseDamage = _weaponsParams.boomerangDamage;
			applyPhysicParams(_weaponsParams.boomerangPhysics);
			break;
		case EProjectileType.Fire:
			_baseDamage = 0;
			applyPhysicParams(_weaponsParams.firePhysics);
			break;
		}
	}
	
	public void Shot(Vector3 dir)
	{
		applyParams();
		rigidbody2D.AddForce(dir * Force);

		SoundManager soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		switch ( ProjectileType )
		{
		case EProjectileType.Boomerang:
			soundMgr.playSoundEffect(soundMgr.shootBoomerang);
			break;
		case EProjectileType.Fire:
			soundMgr.playSoundEffect(soundMgr.shootFire);
			break;
		case EProjectileType.Babosa:
			soundMgr.playSoundEffect(soundMgr.shootBabosa);
			break;
		}
	}

	
	void FixedUpdate()
	{
		//Mira si un boomerang entra dintre d'un foc
		if (ProjectileType == EProjectileType.Fire)
		{
			GameController.me.CheckProjectileOnRadius(transform.position + new Vector3(_collider2D.center.x, _collider2D.center.y, 0), 0.75f);
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

			//Mira si ha tocat amb la paret de darrera
			if (rigidbody2D.velocity.x < 0 && transform.position.x < GameController.me.GetPositionPlayers().x)
			{
				DestroyProjectile();
			}
			//Mira si s'ha parat (Nomes si es babosa o boomerang)
			if (ProjectileType != EProjectileType.Fire)
			{
				if(Time.time - _timeCreation > TimeDuration)
				{
					DestroyProjectile();
				}

				//Debug.Log ("Projectile: " + rigidbody2D.velocity.magnitude);
				if (Time.time > (_timeCreation+0.2f) && rigidbody2D.velocity.magnitude <= 1.5f)
				{
					DestroyProjectile();
				}
			}
			else // fire
			{

				if ( _isStopped )
				{
					if ( (Time.fixedTime - _stopMotionTime) > _weaponsParams.fireGlueTime )
					{
						DestroyProjectile();
					}
				}
				else
				{
					if ( rigidbody2D.velocity.magnitude <= _weaponsParams.fireStopVel && !_firstUpdate)
					{
						rigidbody2D.velocity = Vector2.zero;
						Debug.Log ("STOP!!: " + rigidbody2D.velocity.magnitude);
						_isStopped = true;
						_stopMotionTime = Time.fixedTime;
					}
				}
			}
			break;
		}
		_firstUpdate = false;
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
			//bool isFrontHit = rigidbody2D.velocity.x > 0.0f;
			bool isFrontHit = collision.contacts[0].normal.x < 0.0f;

			switch ( ProjectileType )
			{
			case EProjectileType.Babosa:
				enemy.Escupit(_weaponsParams.babosaFrictionFactor, _weaponsParams.babosaGlueTime);
				break;
			case EProjectileType.Boomerang:
				enemy.Hit(_baseDamage, isFrontHit, _isOnFire, _weaponsParams.fireDamageBoost);
				break;
			case EProjectileType.Fire:
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
			if ( value && !_isOnFire )
			{
				SoundManager soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
				soundMgr.playSoundEffect(soundMgr.shootFire);
			}
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

	static public IEnumerator parseWeapons()
	{
		string v = GameObject.Find ("GameController").GetComponent<GameController> ().json_version;
		string url = "https://dl.dropboxusercontent.com/u/64292958/spgj1"+v+"/weapons.txt";
		WWW www = new WWW(url);
		//Debug.Log ("downloading... " + url);
		
		yield return www;
		//yield return new WaitForSeconds(2);
		JSONNode json = JSONNode.Parse(www.text);
		_weaponsParams = new WeaponsParams();

		_weaponsParams.boomerangDamage = json["boomerang_damage"].AsInt;
		_weaponsParams.babosaGlueTime = json["babosa_glue_time"].AsFloat;
		_weaponsParams.babosaFrictionFactor = json["babosa_friction_factor"].AsFloat;
		_weaponsParams.fireDamageBoost = json["fire_damage_boost"].AsFloat;
		_weaponsParams.fireGlueTime = json["fire_glue_time"].AsFloat;
		_weaponsParams.fireStopVel = json["fire_stop_vel"].AsFloat;

		_weaponsParams.boomerangPhysics = parsePhysicsParams(json["boomerang_physics"]);
		_weaponsParams.babosaPhysics 	= parsePhysicsParams(json["babosa_physics"]);
		_weaponsParams.firePhysics 		= parsePhysicsParams(json["fire_physics"]);
	}

	static WeaponsParams.PhysicsParams parsePhysicsParams(JSONNode json)
	{
		WeaponsParams.PhysicsParams p = new WeaponsParams.PhysicsParams();
		p.force = json["force"].AsFloat;
		p.mass = json["mass"].AsFloat;
		p.drag = json["drag"].AsFloat;

		return p;
	}

	void applyPhysicParams(WeaponsParams.PhysicsParams p)
	{
		transform.rigidbody2D.mass = p.mass;
		transform.rigidbody2D.drag = p.drag;
		Force = p.force;
	}

	static public bool isParseDone()
	{
		return _weaponsParams != null;
	}
}
