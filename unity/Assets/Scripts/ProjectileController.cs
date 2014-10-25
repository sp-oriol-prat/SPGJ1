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
		public int boomerangDamage;
	};
	static WeaponsParams _weaponsParams = null;

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
			GameController.me.RegisterProjectile(this, false);
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
				baseDamage = _weaponsParams.boomerangDamage;
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

	static public IEnumerator parseWeapons()
	{
		string url = "https://dl.dropboxusercontent.com/u/64292958/spgj1/weapons.txt";
		WWW www = new WWW(url);
		Debug.Log ("downloading... " + url);
		
		yield return www;
		//yield return new WaitForSeconds(2);
		JSONNode json = JSONNode.Parse(www.text);
		_weaponsParams = new WeaponsParams();
		_weaponsParams.boomerangDamage = json["boomerang_damage"].AsInt;
	}

	static public bool isParseDone()
	{
		return _weaponsParams != null;
	}
}
