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

	void Start ()
	{
	
	}

	void Update ()
	{

	}

	public void init(float vel)
	{
		_velocity = vel;
		_state = EState.Moving;
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
	}


	void OnDrawGizmos ()
	{
		//Gizmos.DrawGUITexture(Rect(10, 10, 20, 20), myTexture);
	}
}
