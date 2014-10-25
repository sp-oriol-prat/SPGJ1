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

	public EState _state;
	public float _velocity = 1.0f;
	private int _health = 100;
	private SpriteRenderer _sprite;
	private float _timeChangeState;
	private float _timeIntermitent;
	private float kTimeIntermitent = 0.15f;

	void Start ()
	{
		_sprite = GetComponent<SpriteRenderer>();
	}

	public void Init(float vel)
	{
		_velocity = vel;
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
		_state = EState.Dying;
		_timeChangeState = Time.time;
		_timeIntermitent = 0;
	}

	void FixedUpdate_Moving()
	{
		Vector3 moveDir = new Vector3(-1,0,0);
		transform.position += moveDir * _velocity * Time.fixedDeltaTime;
	}

	void FixedUpdate_Attacking()
	{
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
