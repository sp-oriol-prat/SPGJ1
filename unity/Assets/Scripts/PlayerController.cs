using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private EState _state = EState.Disabled;
	private Vector3 _positionOnPress;
	public float Friction = 0.8f;
	//private ProgressBar _healthBar;
	private float _stamina = 1.0f;
	public float StaminaCharge = 0.1f;
	public float StaminaSpend = 0.4f;
	private RadialBar _radialBar;
	private Animator _animator;

	public enum EState
	{
		Disabled,
		Idle,
		Drag,
		Release
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
		GameController.me.SetWorldToUIPosition(transform.position, _radialBar.transform, new Vector2(-140, -90));
		_radialBar.SetProgress(Random.value);
		//Animator
		_animator = GetComponentInChildren<Animator>();
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
			Vector3 dir = dirRaw.normalized;
			GameController.me.Arrow.transform.position = _positionOnPress;
			GameController.me.Tirachinas.transform.position = _positionOnPress;
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;;
			GameController.me.Tirachinas.transform.rotation = Quaternion.Euler (0, 0, angle);
			GameController.me.Arrow.transform.rotation = Quaternion.Euler (0, 0, angle);
			GameController.me.Arrow.transform.position += dir*2;
			Vector3 scale = GameController.me.Tirachinas.transform.localScale;
			scale.x = dirRaw.magnitude*0.2f;
			GameController.me.Tirachinas.transform.localScale = scale;
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
			GameObject projectileRes = Resources.Load ("Projectile") as GameObject;
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
}
