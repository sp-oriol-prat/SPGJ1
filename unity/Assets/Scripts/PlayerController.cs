using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private EState _state = EState.Idle;
	private Vector3 _positionOnPress;
	public float Friction = 0.8f;
	private ProgressBar _healthBar;
	private RadialBar _radialBar;

	public enum EState
	{
		Idle,
		//Press,
		Drag,
		Release
	}

	// Use this for initialization
	void Start () 
	{
		//Health
		_healthBar = GameController.me.InstantiateUI("HealthBar").GetComponent<ProgressBar>();
		GameController.me.SetWorldToUIPosition(transform.position, _healthBar.transform, new Vector2(0, -100));
		_healthBar.SetProgress(Random.value);
		//Stamina
		_radialBar = GameController.me.InstantiateUI("StaminaBar").GetComponent<RadialBar>();
		GameController.me.SetWorldToUIPosition(transform.position, _radialBar.transform, new Vector2(-140, -90));
		_radialBar.SetProgress(Random.value);
	}
	
	// Update is called once per frame
	void Update () 
	{
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
		if (State == EState.Idle)
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
