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
	private int _streetIndex;
	private SpriteRenderer _babosa;
	private Animator _animator;
	private GameObject _debugStatsGo;

	void Start ()
	{
		_sprite = GetComponent<SpriteRenderer>();
		if (_sprite ==null)
		{
			_sprite = transform.Find ("Inside").GetComponent<SpriteRenderer>();
		}
		_babosa = transform.Find("babosa_attack").GetComponent<SpriteRenderer>();
		_babosa.enabled = false;
		_animator = GetComponentInChildren<Animator>();


		// Enemy stats.
		_debugStatsGo = (GameObject)GameObject.Instantiate(Resources.Load("EnemyStats"), Vector3.zero, Quaternion.identity);
		_debugStatsGo.transform.parent = transform;
		_debugStatsGo.transform.localPosition = new Vector3(-5.10f, 0.69f, 0.0f);
	}

	public void Init (Data data, int streetIndex)
	{
		_data = new Data (data);	
		_streetIndex = streetIndex;

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
			if (_health <= 0) {
					SetStateDie ();
			}
		}
	}

	void FixedUpdate ()
	{
			switch (_state) {
		case EState.Moving:
			FixedUpdate_Moving ();
			break;

		case EState.Attacking:
			FixedUpdate_Attacking ();
			break;

		case EState.Dying:
			FixedUpdate_Dying ();
			break;
		}

		updateDebugState ();
	}

	private void SetStateDie ()
	{
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
		transform.position += moveDir * _velocity * Time.fixedDeltaTime;
		if (transform.position.x < GameController.me.GetPositionPlayers ().x) {
				SetStateAttacking ();
		}
	}

	void FixedUpdate_Attacking ()
	{
		if (Time.time > _timeChangeState + 1.0f) {
			//TODO: Attack animation and selecciona el player correcte per atacar (passant com a valor el tipus d'enemic del quer es tracta)
			PlayerController player = GameController.me.GetPlayer (0);
			if (player != null) {
				GameController.me.GetPlayer (_streetIndex).Hit (_data.id);
			}
			SetStateDie ();
		}
	}

	void FixedUpdate_Dying ()
	{
		_timeIntermitent -= Time.deltaTime;
		if (_timeIntermitent <= 0) {
			_timeIntermitent = kTimeIntermitent;
			_sprite.enabled = !_sprite.enabled;
		}
		if (Time.time > _timeChangeState + 2.0f) {
			Destroy (this.gameObject);
		}
	}

	public void Hit (float baseDamage, bool isFrontHit, bool isFireHit)
	{
		if (_animator != null)
		{
			_animator.SetTrigger("Hit");
		}
		float damage = baseDamage;
		if (_data.hasShield && isFrontHit || _data.isElemental && !isFireHit)
		{
			damage = 0.0f;
		}

		if (damage != 0.0f)
		{
			if (isFireHit)
			{
				const float FireBoostDamage = 1.5f;
				damage *= FireBoostDamage;
			}
		}
		Debug.Log ("damage(" + damage + ") " + _health + " -> " + (_health - damage));
		_health -= damage;

		const float injuredFactor = 0.4f;
		bool isDead = _health <= 0.0f;				
		bool isInjured = _health <= _data.life * injuredFactor;

		if (isDead)
		{
			SetStateDie();
		}
		else if ( isInjured )
		{
			//onInjured();
		}
	}

	public void Escupit()
	{
		Debug.Log ("Escupit!!!");
		//TODO: Aqui hauria de disminuïr la velocitat!!
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
			string textStr = "live(" + _health + ")";
			tm.text = textStr;
		}
	}
}
