using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	private Camera _cameraRef;
	private Camera _cameraUI;
	public static GameController me;
	private Plane _groundPlane;
	private SpriteRenderer _tirachinas;
	private SpriteRenderer _arrow;
	private UIRoot _uiRoot;
	private PlayerController[] _players;
	private MainMenu _mainMenu;
	private EnemiesManager _enemiesManager;

	// Use this for initialization
	void Start () 
	{
		me = this;
		_cameraRef = GameObject.Find ("CameraMain").camera;
		_cameraUI = GameObject.Find ("CameraUI").camera;
		_groundPlane = new Plane(Vector3.back, Vector3.zero);
		_tirachinas = GameObject.Find("Tirachinas").GetComponent<SpriteRenderer>();
		_arrow = GameObject.Find("Arrow").GetComponent<SpriteRenderer>();
		_uiRoot = GameObject.Find("UI Root").GetComponent<UIRoot>();
		_enemiesManager = GameObject.Find ("EnemyManager").GetComponent<EnemiesManager> ();
		_tirachinas.enabled = false;
		_arrow.enabled = false;
		_players = FindObjectsOfType<PlayerController>();
		_mainMenu = FindObjectOfType<MainMenu>();
	}

	public GameObject InstantiateUI(string path)
	{
		GameObject go = Resources.Load("UI/" + path) as GameObject;
		return NGUITools.AddChild(_uiRoot.gameObject, go);
	}
	
	public void SetWorldToUIPosition(Vector3 pos3D, Transform transUI)
	{
		SetWorldToUIPosition(pos3D, transUI, Vector2.zero);
	}

	public void SetWorldToUIPosition(Vector3 pos3D, Transform transUI, Vector2 offset)
	{
		Vector3 pos = _cameraRef.camera.WorldToViewportPoint(pos3D);
		if (_cameraUI == null)
		{
			_cameraUI = GameObject.Find("UICamera").camera;
		}
		transUI.position = _cameraUI.ViewportToWorldPoint(pos);
		//transform.localPosition += Offset2D;
		pos = transUI.localPosition;
		pos.x = Mathf.RoundToInt(pos.x) + offset.x;
		pos.y = Mathf.RoundToInt(pos.y) + offset.y;
		pos.z = 0f;
		transUI.transform.localPosition = pos;
	}

	public Vector3 GetPosMouse3D
	{
		get
		{
			Ray ray = _cameraRef.ScreenPointToRay(Input.mousePosition);
			float rayDistance;
			Vector3 posMouse3D = Vector3.zero;
			if (_groundPlane.Raycast(ray, out rayDistance))
			{
				posMouse3D = ray.GetPoint(rayDistance);
			}
			return posMouse3D;
		}
	}

	public SpriteRenderer Tirachinas
	{
		get
		{
			return _tirachinas;
		}
	}
	
	public SpriteRenderer Arrow
	{
		get
		{
			return _arrow;
		}
	}

	public void StartGame()
	{
		_mainMenu.Show(false);
		for (int i=0; i<_players.Length; i++)
		{
			_players[i].Enable();
		}

		_enemiesManager.doCanStart();
	}
}
