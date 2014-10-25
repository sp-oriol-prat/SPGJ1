using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private EState _state = EState.Disabled;
	private Vector3 _positionOnPress;
	//private ProgressBar _healthBar;
	private float _stamina = 1.0f;
	private const float kLengthTirachinas = 2.0f;
	public ETypes PlayerType = ETypes.Paladin;
	public float StaminaCharge = 0.1f;
	public float StaminaSpend = 0.4f;
	private RadialBar _radialBar;
	private Animator _animator;
	private int _health;
	private float _timeChangeState;
	private float _timeIntermitent;
	private float kTimeIntermitent = 0.15f;
	private SpriteRenderer _sprite;

	public enum EState
	{
		Disabled,
		Idle,
		Drag,
		Release,
		Dying
	}

	public enum ETypes
	{
		Paladin,
		Mago,
		Babosa
	}

	// Use this for initialization
	void Start () 
	{
		//Health
		//_healthBar = GameController.me.InstantiateUI("HealthBar").GetComponent<ProgressBar>();
		//GameController.me.SetWorldToUIPosition(transform.position, _healthBar.transform, new Vector2(0, -100));
		//_healthBar.SetProgress(Random.value);
		//Stamina
		_radialBar = GameController.me.InstantiateUI("StaminaBar").GetComponent<RadialBar>();
		_radialBar.transform.localScale = Vector3.one *0.65f;
		GameController.me.SetWorldToUIPosition(transform.position, _radialBar.transform, new Vector2(-60, 0));
		_radialBar.SetProgress(Random.value);
		//Animator
		_animator = GetComponentInChildren<Animator>();
		_sprite = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Stamina<=1)
		{
			Stamina += StaminaCharge*Time.deltaTime;
		} else {
			Stamina = 1;
		}
		switch(State)
		{
		case EState.Drag:
			Vector3 positionOnDrag = GameController.me.GetPosMouse3D;
			Vector3 dirRaw = (_positionOnPress - positionOnDrag);
			//Limita la Força del Tirachinas
			if (dirRaw.magnitude > kLengthTirachinas)
			{
				dirRaw = dirRaw.normalized * kLengthTirachinas;
			}
			Vector3 dir = dirRaw.normalized;
			GameController.me.Arrow.transform.position = _positionOnPress;
			GameController.me.Tirachinas.transform.position = _positionOnPress;
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;;
			GameController.me.Tirachinas.transform.rotation = Quaternion.Euler (0, 0, angle);
			GameController.me.Arrow.transform.rotation = Quaternion.Euler (0, 0, angle);
			GameController.me.Arrow.transform.position += dir*2;
			Vector3 scale = GameController.me.Tirachinas.transform.localScale;
			scale.x = dirRaw.magnitude*1.2f;
			GameController.me.Tirachinas.transform.localScale = scale;
			break;
		case EState.Dying:
			StateDyingBehaviour();
			break;
		}
	}
	
	void OnMouseDown() 
	{
		//Mira tambe si te prou stamina
		if (State == EState.Idle && Stamina >= StaminaSpend)
		{
			GameController.me.Arrow.enabled = true;
			GameController.me.Tirachinas.enabled = true;
			State = EState.Drag;
			_positionOnPress = GameController.me.GetPosMouse3D;
			_animator.SetTrigger("AttackPrepare");
		}
	}
	
	void OnMouseUp() 
	{
		if (State == EState.Drag)
		{
			GameController.me.Arrow.enabled = false;
			GameController.me.Tirachinas.enabled = false;
			Vector3 positionOnRelease = GameController.me.GetPosMouse3D;
			Vector3 force = (_positionOnPress - positionOnRelease);
			//Limita la Força del Tirachinas
			if (force.magnitude > kLengthTirachinas)
			{
				force = force.normalized * kLengthTirachinas;
			}
			string path = "ProjectileBoomerang";
			switch(PlayerType)
			{
			case ETypes.Mago:
				path = "ProjectileFire";
				break;
			case ETypes.Babosa:
				path = "ProjectileBabosa";
				break;
			}
			GameObject projectileRes = Resources.Load (path) as GameObject;
			GameObject projectileGo = Instantiate(projectileRes, transform.position, Quaternion.identity) as GameObject;
			projectileGo.GetComponent<ProjectileController>().Shot(force);
			State = EState.Idle;
			_animator.SetTrigger("Attack");
			Stamina -= StaminaSpend;
		}
	}

	public void Enable()
	{
		State = EState.Idle;
	}
	
	public void Disable()
	{
		State = EState.Disabled;
	}

	private float Stamina
	{
		get
		{
			return _stamina;
		}
		set
		{
			_stamina = value;
			_radialBar.SetProgress(_stamina);
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

	public void Hit(string _typeEnemy)
	{
		Health = Health - 100;
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
	
	private void SetStateDie()
	{
		collider2D.enabled = false;
		_state = EState.Dying;
		_timeChangeState = Time.time;
		_timeIntermitent = 0;
	}
	
	private void StateDyingBehaviour()
	{
		_timeIntermitent -= Time.deltaTime;
		if (_timeIntermitent <= 0)
		{
			_timeIntermitent = kTimeIntermitent;
			_sprite.enabled = !_sprite.enabled;
		}
		if (Time.time > _timeChangeState + 2.0f)
		{
			GameController.me.EndGame();
			Destroy (this.gameObject);
		}
	}
}
