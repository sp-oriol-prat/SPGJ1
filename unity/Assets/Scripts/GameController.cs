using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

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
	//private EndMenu _endMenu;
	private TestMenu _testMenu;
	private Vector3 _positionPlayers;
	private EnemiesManager _enemiesManager;
	private List<ProjectileController> _projectiles;

	//public string[] _levelsNames;
	private static int _currentLevel = 0;
	private static bool _repeatLevel = false;


	private class LevelConfig
	{
		public string filename;
		public bool[] characters;
	};
	private static LevelConfig[] _levelConfigs;
	private static bool _levelsConfigParsed = false;
	public static bool isLevelsConfigParsed()
	{
		return _levelsConfigParsed;
	}

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
		//_endMenu = FindObjectOfType<EndMenu>();
		_testMenu = FindObjectOfType<TestMenu>();

		StartCoroutine(parseLevelsConfig());
		StartCoroutine(ProjectileController.parseWeapons());
		StartCoroutine(PlayerController.parseCharacters());

		if ( !_repeatLevel )
		{
			if ( _currentLevel == 0 )
			{
				_mainMenu.OnStartGame();
			}
			else if (_currentLevel == _levelConfigs.Length )
			{
				_mainMenu.OnEndedGame();
				_currentLevel = 0;
			}
			else
			{
				_mainMenu.OnNextLevel();
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
		readyToStart &= _levelsConfigParsed;

		Debug.Log ("readytostart: " + (readyToStart ? 1:0));
		if (readyToStart)
		{
			_mainMenu.Show(false);
			_testMenu.Show(true);

			//string levelName = _levelConfigs[_currentLevel].filename;

			/*if (levName == "level1.txt")
			{
				ShowPlayer(0, false);
				ShowPlayer(1, true);
				ShowPlayer(2, false);
			} 
			else if (levName == "level2.txt")
			{
				ShowPlayer(0, false);
				ShowPlayer(1, true);
				ShowPlayer(2, true);
			}
			else 
			{
				ShowPlayer(0, true);
				ShowPlayer(1, true);
				ShowPlayer(2, true);
			}
			*/
			LevelConfig lconfig = _levelConfigs[_currentLevel];
			ShowPlayer(0, lconfig.characters[0]);
			ShowPlayer(1, lconfig.characters[1]);
			ShowPlayer(2, lconfig.characters[2]);

			_enemiesManager.doCanStart();
		}

		_repeatLevel = false;
	}

	public void ShowPlayer(int i, bool flag)
	{
		if (flag)
		{
			Players[i].Enable();
			Players[i].InitParams();
		}
		Players[i].Show(flag);
	}

	public string getCurrentLevelName()
	{
		return _levelConfigs[_currentLevel].filename;
	}

	public void EndLevel()
	{
		_currentLevel++;
		if ( _currentLevel == _levelConfigs.Length )
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

		//TODO: Stop the Enemies from moving
		//EnemiesManager.Stop()
		StartCoroutine(ResetGameWithDelay());
	}

	private IEnumerator ResetGameWithDelay()
	{
		yield return new WaitForSeconds(1.0f);
		_repeatLevel = true;
		RestartGame();
	}


	public void EndGame()
	{
		//_testMenu.Show(false);
		//_endMenu.Show(true);
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
		RestartGame();
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

	static public IEnumerator parseLevelsConfig()
	{
		string v = GameObject.Find ("GameController").GetComponent<GameController> ().json_version;
		string url = "https://dl.dropboxusercontent.com/u/64292958/spgj1"+v+"/levels.config.txt";
		WWW www = new WWW(url);
		
		yield return www;
		JSONArray json = JSONNode.Parse(www.text).AsArray;
		_levelConfigs = new LevelConfig[json.Count];

		for ( int i = 0; i < json.Count; i++ )
		{
			LevelConfig config = _levelConfigs[i] = new LevelConfig();
			JSONNode jLevelConf = json[i];
			config.filename = jLevelConf["file"];

			config.characters = new bool[3];
			for ( int j = 0; j < 3; j++ ){ config.characters[j] = false; }
			JSONArray jCharacters = jLevelConf["characters"].AsArray;
			for ( int j = 0; j < jCharacters.Count; j++ )
			{
				config.characters[jCharacters[j].AsInt] = true;
			}
		}

		_levelsConfigParsed = true;
	}
}
