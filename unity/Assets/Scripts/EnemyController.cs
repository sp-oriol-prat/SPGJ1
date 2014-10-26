using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
	public enum EState
	{
		Moving,
		Attacking,
		Dying,
		Idle
	};

	public enum Element
	{
		Fire = 0,
		Ice = 1,
		Wind = 2
	};

	public class Data
	{
		public string id;
		public int life;
		public float walkSpeed;
		public float attackDamage;
		public float attackSpeed;
		public float stopTimeOnHit;
		public bool hasShield;
		public bool isElemental;

		public Data ()
		{
		}

		public Data (Data d)
		{
			id = d.id;
			life = d.life;
			walkSpeed = d.walkSpeed;
			attackDamage = d.attackDamage;
			attackSpeed = d.attackSpeed;
			stopTimeOnHit = d.stopTimeOnHit;
			hasShield = d.hasShield;
			isElemental = d.isElemental;
		}
	};

	private Data _data;
	public EState _state;
	private float _velocity;
	private float _health;
	private SpriteRenderer _sprite;
	private float _timeChangeState;
	private float _timeIntermitent;
	private float kTimeIntermitent = 0.15f;
	private float kTimeIntermitentTotal = 0.9f;
	private int _streetIndex;
	private SpriteRenderer _babosa;
	private Animator _animator;
	private GameObject _debugStatsGo;
	private float _babosaFriction = 1.0f;
	private float _babosaDuration;
	private float _babosaStartTime;
	private float _previousXPos = 0.0f;
	private GameObject _fxShield;

	void Start ()
	{
		_fxShield = Resources.Load ("FxShield") as GameObject;
		_sprite = GetComponent<SpriteRenderer>();
		if (_sprite ==null)
		{
			_sprite = transform.Find ("Inside").GetComponent<SpriteRenderer>();
		}
		_babosa = transform.Find("babosa_attack").GetComponent<SpriteRenderer>();
		_babosa.enabled = false;
		_animator = GetComponentInChildren<Animator>();


		// Enemy stats.
		/*_debugStatsGo = (GameObject)GameObject.Instantiate(Resources.Load("EnemyStats"), Vector3.zero, Quaternion.identity);
		_debugStatsGo.transform.parent = transform;
		_debugStatsGo.transform.localPosition = new Vector3(-5.10f, 0.69f, 0.0f);
		*/

		_previousXPos = transform.position.x;
	}

	public void Init (Data data, int streetIndex)
	{
		_data = new Data (data);	
		_streetIndex = streetIndex;
		PlayerController player = GameController.me.GetPlayer(_streetIndex);
		_velocity = _data.walkSpeed;
		_health = _data.life;		
		_state = EState.Moving;
	}

	public float Health {
		get {
			return _health;
		}
		set {
			_health = value;
			if (_health <= 0)
			{
				SetStateDie ();
			} else {
				_sprite.color = new Color(1, 0.25f, 0.25f, 1);
				StartCoroutine(GoBackToNormalColor());
			}
		}
	}

	private IEnumerator GoBackToNormalColor()
	{
		yield return new WaitForSeconds(0.6f);
		_sprite.color = new Color(1, 1, 1, 1);
	}

	void FixedUpdate ()
	{
		switch (_state)
		{
		case EState.Moving:
			FixedUpdate_Moving ();
			break;

		case EState.Attacking:
			FixedUpdate_Attacking ();
			break;

		case EState.Dying:
			FixedUpdate_Dying ();
			break;

		case EState.Idle:
			break;
		}

		updateDebugState();

		// update babosa
		if ( _babosa.enabled && (Time.fixedTime - _babosaStartTime > _babosaDuration))
		{
			_babosa.enabled = false;
			_babosaFriction = 1.0f;
		}
	}

	private void SetStateDie ()
	{
		_sprite.color = new Color(1, 0.25f, 0.25f, 1);
		collider2D.enabled = false;
		_state = EState.Dying;
		_timeChangeState = Time.time;
		_timeIntermitent = 0;
		_babosa.enabled = false;

		GameObject.Find ("EnemyManager").GetComponent<EnemiesManager>().onEnemyDied();
	}

	private void SetStateAttacking ()
	{
		_state = EState.Attacking;
		_timeChangeState = Time.time;
	}

	void FixedUpdate_Moving ()
	{
		Vector3 moveDir = new Vector3 (-1, 0, 0);
		transform.position += moveDir * _velocity * Time.fixedDeltaTime * _babosaFriction;
		if (transform.position.x < GameController.me.GetPositionPlayers().x)
		{
			SetStateAttacking ();
		}
	}

	void FixedUpdate_Attacking ()
	{
		if (Time.time > _timeChangeState + 1.0f) {
			//TODO: Attack animation and selecciona el player correcte per atacar (passant com a valor el tipus d'enemic del quer es tracta)
			PlayerController player = GameController.me.GetPlayer(_streetIndex);
			if (player != null) {
				player.Hit(_data.id);
				if ( player.Health <= 0.0f )
				{
					_state = EState.Idle;
				}
			}
			//SetStateDie ();
		}
	}

	void FixedUpdate_Dying ()
	{
		_timeIntermitent -= Time.deltaTime;
		if (_timeIntermitent <= 0) {
			_timeIntermitent = kTimeIntermitent;
			_sprite.enabled = !_sprite.enabled;
		}
		if (Time.time > _timeChangeState + kTimeIntermitentTotal) 
		{
			Destroy (this.gameObject);
		}
	}

	public void Hit (float baseDamage, bool isFrontHit, bool isFireHit, float fireBoostDamage)
	{
		SoundManager soundMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		Debug.Log("front_hit: " + (isFrontHit ? 1 : 0));

		float damage = baseDamage;
		bool preventedByShield = _data.hasShield && isFrontHit;
		bool preventedByElement = _data.isElemental && !isFireHit;
		if (preventedByShield || preventedByElement)
		{
			damage = 0.0f;
		}
        if (preventedByShield)
		{
			Instantiate(_fxShield, transform.position, Quaternion.identity);
        }

		if (damage != 0.0f)
		{
			if (isFireHit)
			{
				damage *= fireBoostDamage;
			}
		}
		//Debug.Log ("damage(" + damage + ") " + _health + " -> " + (_health - damage));
		if (_animator != null)
		{
			if ( damage > 0.0f )
			{
				_animator.SetTrigger("Hit");
			}
			else if ( _data.id == "E" && preventedByShield )
			{
				_animator.SetTrigger("HitShield");
			}
		}

		if ( preventedByShield )
		{
			soundMgr.playSoundEffect(soundMgr.shieldBlock);
		}
		else if ( damage > 0.0f )
		{
			soundMgr.playSoundEffect (soundMgr.hit);
		}

		if ( damage > 0.0f )
		{
			Health -= damage;
		}
	}

	public void Escupit(float babosaFriction, float babosaDuration)
	{
		_babosaFriction = babosaFriction;
		_babosaDuration = babosaDuration;
		_babosaStartTime = Time.fixedTime;

		if (_babosa != null)
		{
			_babosa.enabled = true;
		}
	}

	void updateDebugState()
	{
		TextMesh tm = GetComponentInChildren<TextMesh>();
		if (tm)
		{
			float velx = -(transform.position.x - _previousXPos)/Time.fixedDeltaTime;
			_previousXPos = transform.position.x;
			string textStr = "<3:" + _health + " vel:" + velx.ToString("0.00") + " shield: " + (_data.hasShield ? 1 : 0);
			tm.text = textStr;
		}
	}
}
