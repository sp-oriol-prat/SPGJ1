﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

	public string json_version = "/";

	private Camera _cameraRef;
	private Camera _cameraUI;
	public static GameController me;
	private Plane _groundPlane;
	private SpriteRenderer _tirachinas;
	private SpriteRenderer _arrow;
	private UIRoot _uiRoot;
	public PlayerController[] Players;
	private MainMenu _mainMenu;
	private EndMenu _endMenu;
	private TestMenu _testMenu;
	private Vector3 _positionPlayers;
	private EnemiesManager _enemiesManager;
	private List<ProjectileController> _projectiles;

	public string[] _levelsNames;
	private static int _currentLevel = 0;
	private static bool _repeatLevel = false;

	void Awake()
	{
		_projectiles = new List<ProjectileController>();
	}
	// Use this for initialization
	void Start () 
	{
		me = this;
		_cameraUI = GameObject.Find ("CameraUI").camera;
		_groundPlane = new Plane(Vector3.back, Vector3.zero);
		_tirachinas = GameObject.Find("Tirachinas").GetComponent<SpriteRenderer>();
		_arrow = GameObject.Find("Arrow").GetComponent<SpriteRenderer>();
		_uiRoot = GameObject.Find("UI Root").GetComponent<UIRoot>();
		_enemiesManager = GameObject.Find ("EnemyManager").GetComponent<EnemiesManager> ();
		_tirachinas.enabled = false;
		_arrow.enabled = false;
		_positionPlayers = Players[0].transform.position;
		_positionPlayers.x += 1;
		_mainMenu = FindObjectOfType<MainMenu>();
		_endMenu = FindObjectOfType<EndMenu>();
		_testMenu = FindObjectOfType<TestMenu>();

		StartCoroutine(ProjectileController.parseWeapons());
		StartCoroutine(PlayerController.parseCharacters());

		if ( !_repeatLevel )
		{
			if ( _currentLevel > 0 )
			{
				_mainMenu.OnNextLevel();
			}
			else
			{
				_mainMenu.OnStartGame();
			}
		}
		else
		{
			_mainMenu.OnRepeatLevel();
		}
	}

	public Camera CameraRef
	{
		get
		{
			if (_cameraRef == null)
			{
				_cameraRef = GameObject.Find ("CameraMain").camera;
			}
			return _cameraRef;
		}
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
		Vector3 pos = CameraRef.camera.WorldToViewportPoint(pos3D);
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
			Ray ray = CameraRef.ScreenPointToRay(Input.mousePosition);
			float rayDistance;
			Vector3 posMouse3D = Vector3.zero;
			if (_groundPlane.Raycast(ray, out rayDistance))
			{
				posMouse3D = ray.GetPoint(rayDistance);
			}
			return posMouse3D;
		}
	}

	public PlayerController GetPlayer(int indexRow)
	{
		return Players[Mathf.FloorToInt((float)((float)indexRow*0.5f))] ;
	}

	public Vector3 GetPositionPlayers()
	{
		return _positionPlayers;
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
		Debug.Log ("StartGame");
		_testMenu.Message("");

		bool readyToStart = true;
		readyToStart &= ProjectileController.isParseDone();
		readyToStart &= PlayerController.isParseDone();

		Debug.Log ("readytostart: " + (readyToStart ? 1:0));
		if (readyToStart)
		{
			/*if ( _currentLevel == 0 )
			{
				_mainMenu.Show(false);
			}*/
			_mainMenu.Show(false);
			_testMenu.Show(true);

			for (int i=0; i<Players.Length; i++)
			{
				if ( _currentLevel >= i )
				{
					Players[i].Enable();
					Players[i].InitParams();
				}
				else
				{
					Players[i].enabled = false;
				}
			}
			
			_enemiesManager.doCanStart();
		}

		_repeatLevel = false;
	}

	public string getCurrentLevelName()
	{
		return _levelsNames[_currentLevel];
	}

	public void EndLevel()
	{
		_currentLevel++;
		if ( _currentLevel == _levelsNames.Length )
		{
			Debug.Log("END GAME!");
			EndGame();
		}
		else
		{
			Debug.Log("NEXT LEVEL!");
			RestartGame();
		}
	}

	public void OnLosedLevel()
	{
		Debug.Log ("==LOSER GAMEER==");

		//_testMenu.Show(false);
		//_endMenu.Show(true);
		//_endMenu.OnRepeatLevel()
			//;
		for (int i=0; i<Players.Length; i++)
		{
			if (Players[i] != null)
			{
				Players[i].Disable();
			}
		}

		RestartGame();

		_repeatLevel = true;

		//TODO: Stop the Enemies from moving
		//EnemiesManager.Stop()
		//StartCoroutine(EndGameDelayed());
	}

	public void EndGame()
	{
		_testMenu.Show(false);
		_endMenu.Show(true);
		for (int i=0; i<Players.Length; i++)
		{
			if (Players[i] != null)
			{
				Players[i].Disable();
			}
		}
		//TODO: Stop the Enemies from moving
		//EnemiesManager.Stop()
		StartCoroutine(EndGameDelayed());
	}

	private IEnumerator EndGameDelayed()
	{
		yield return new WaitForSeconds(1.0f);
	}
	
	public void RestartGame()
	{
		Application.LoadLevel(Application.loadedLevelName);
	}

	public void CheckProjectileOnRadius(Vector3 pos, float radius)
	{
		//De moment nomes mira si son els boomerangs
		for(int i=0; i<_projectiles.Count; i++)
		{
			if (_projectiles[i].ProjectileType == ProjectileController.EProjectileType.Boomerang && !_projectiles[i].IsOnFire)
			{
				float dist = Vector3.Distance(pos, _projectiles[i].transform.position);
				if (dist < radius)
				{
					_projectiles[i].IsOnFire = true;
				}
			}
		}
	}

	public void RegisterProjectile(ProjectileController projectile, bool flag)
	{
		if (_projectiles == null)
		{
			_projectiles = new List<ProjectileController>();
		}

		if (flag)
		{
			_projectiles.Add(projectile);
		} else {
			_projectiles.Remove(projectile);
		}
	}

	public void onStartWave(int numWave, bool isLast)
	{
		_testMenu.Wave(numWave);
	}

	public void showWaveMessage(string msg)
	{
		_testMenu.Message(msg);
	}
}
