﻿using UnityEngine;
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

		public PhysicsParams boomerangPhysics;
		public PhysicsParams firePhysics;
		public PhysicsParams babosaPhysics;

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

	public void applyParams()
	{
		Debug.Log ("Apply physics!");
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
			enemy.Hit(_baseDamage, isFrontHit, _isOnFire);
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
		_weaponsParams.babosaGlueTime = json["babosa_glue_time"].AsFloat;

		_weaponsParams.boomerangPhysics = parsePhysicsParams(json["boomerang_physics"]);
		_weaponsParams.babosaPhysics 	= parsePhysicsParams(json["fire_physics"]);
		_weaponsParams.firePhysics 		= parsePhysicsParams(json["babosa_physics"]);
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
