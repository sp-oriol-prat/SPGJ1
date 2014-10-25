using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
	public enum EState
	{
		Moving,
		Attacking,
		Dying
	};

	public enum Element { Fire = 0, Ice = 1, Wind = 2 };
	public class Data
	{
		public string id;
		public int life;
		public float walkSpeed;
		public float attackDamage;
		public float attackSpeed;
		public float stopTimeOnHit;
		public int damageOnFrontHit;
		public int damageOnBackHit;
		public bool hasShield;
		public bool isElemental;

		public Data()
		{
		}

		public Data(Data d)
		{
			id = d.id;
			life = d.life;
			walkSpeed = d.walkSpeed;
			attackDamage = d.attackDamage;
			attackSpeed = d.attackSpeed;
			stopTimeOnHit = d.stopTimeOnHit;
			damageOnFrontHit = d.damageOnFrontHit;
			damageOnBackHit = d.damageOnBackHit;
			hasShield = d.hasShield;
			isElemental = d.isElemental;
		}
	};

	private Data _data;

	public EState _state;
	private float _velocity;
	private int _health;
	private SpriteRenderer _sprite;
	private float _timeChangeState;
	private float _timeIntermitent;
	private float kTimeIntermitent = 0.15f;

	private int _streetIndex;

	void Start ()
	{
		_sprite = GetComponent<SpriteRenderer>();
	}

	public void Init(Data data, int streetIndex)
	{
		_data = new Data(data);	
		_streetIndex = streetIndex;

		_velocity = _data.walkSpeed;
		_health = _data.life;		
		_state = EState.Moving;
	}

	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			_health = value;
			if (_health <= 0)
			{
				SetStateDie();
			}
		}
	}

	void FixedUpdate ()
	{
		switch (_state)
		{
		case EState.Moving:
			FixedUpdate_Moving();
			break;

		case EState.Attacking:
			FixedUpdate_Attacking();
			break;

		case EState.Dying:
			FixedUpdate_Dying();
			break;
		}
	}

	private void SetStateDie()
	{
		collider2D.enabled = false;
		_state = EState.Dying;
		_timeChangeState = Time.time;
		_timeIntermitent = 0;
	}

	private void SetStateAttacking()
	{
		_state = EState.Attacking;
		_timeChangeState = Time.time;
	}

	void FixedUpdate_Moving()
	{
		Vector3 moveDir = new Vector3(-1,0,0);
		transform.position += moveDir * _velocity * Time.fixedDeltaTime;
		if (transform.position.x < GameController.me.GetPositionPlayers().x)
		{
			SetStateAttacking();
		}
	}

	void FixedUpdate_Attacking()
	{
		if (Time.time > _timeChangeState + 1.0f)
		{
			//TODO: Attack animation and selecciona el player correcte per atacar (passant com a valor el tipus d'enemic del quer es tracta)
			PlayerController player = GameController.me.GetPlayer(0);
			if (player != null)
			{
				GameController.me.GetPlayer(0).Hit("A");
			}
			SetStateDie();
		}
	}

	void FixedUpdate_Dying()
	{
		_timeIntermitent -= Time.deltaTime;
		if (_timeIntermitent <= 0)
		{
			_timeIntermitent = kTimeIntermitent;
			_sprite.enabled = !_sprite.enabled;
		}
		if (Time.time > _timeChangeState + 2.0f)
		{
			Destroy (this.gameObject);
		}
	}

	public void Hit(ProjectileController.EProjectileType projectileType)
	{
		//TODO: Treure la vida que toqui
		Health = Health -100;
	}

	void OnDrawGizmos ()
	{
		//Gizmos.DrawGUITexture(Rect(10, 10, 20, 20), myTexture);
	}


}
