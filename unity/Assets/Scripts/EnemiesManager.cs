using UnityEngine;
using System.Collections;
using SimpleJSON;

public class EnemiesManager : MonoBehaviour
{
	public Transform[] spawnPoints;

	static int kNumStreets = 6;

	GameObject _enemyPrefab_A;
	GameObject _enemyPrefab_B;
	GameObject _enemyPrefab_C;
	GameObject _enemyPrefab_D;
	GameObject _enemyPrefab_E;
	
	class WaveData
	{
		public float time;
		public string[] enemiesId = new string[kNumStreets];
	}

	EnemyController.Data[] _enemiesData;
	WaveData[] _wavesData;

	int _currentWave;
	bool _dataReady;
	float _startTime;
	bool _wavesDataReady = false;
	bool _enemiesDataReady = false;
	bool _isStarted = false;
	bool _canStart = false;


	void Start()
	{
		StartCoroutine(parseEnemies());
		StartCoroutine(parseWaves());

		_enemyPrefab_A = Resources.Load("Enemy_A") as GameObject;
		_enemyPrefab_B = Resources.Load("Enemy_B") as GameObject;
		_enemyPrefab_C = Resources.Load("Enemy_C") as GameObject;
		_enemyPrefab_D = Resources.Load("Enemy_D") as GameObject;
		_enemyPrefab_E = Resources.Load("Enemy_E") as GameObject;
	}

	public void doCanStart()
	{
		_canStart = true;
	}
	

	void Update ()
	{
	}

	void FixedUpdate()
	{
		if (!_isStarted)
		{
			bool canUpdate = _canStart && _enemiesDataReady & _wavesDataReady;
			if (!canUpdate)
			{
				return;
			}
			else
			{
				_isStarted = true;
				_startTime = Time.fixedTime;
			}
		}

		if (_currentWave == _wavesData.Length)
		{
			onFinishedEnemies();
			return;
		}

		float t = Time.fixedTime;
		float dt = t - _startTime;
		if ( dt > _wavesData[_currentWave].time )
		{
			spawnWave(_wavesData[_currentWave]);
			_currentWave++;
		}
	}
	
	IEnumerator parseEnemies()
	{
		string enemiesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/__enemies.txt";
		WWW www = new WWW(enemiesUrl);
		//Debug.Log ("downloading... " + enemiesUrl);

		yield return www;
		JSONNode jEnemies = JSONNode.Parse(www.text).AsArray;

		_enemiesData = new EnemyController.Data[jEnemies.Count];
		for (int i = 0; i < jEnemies.Count; i++)
		{
			JSONNode jEnemy = jEnemies[i];
			
			EnemyController.Data eData = new EnemyController.Data();
			eData.id = jEnemy["id"];
			eData.life = jEnemy["life"].AsInt;
			eData.walkSpeed = (float)jEnemy["walk_speed"].AsInt / 100.0f;
			eData.attackDamage = jEnemy["attack_damage"].AsInt;
			eData.attackSpeed = (float)jEnemy["attack_speed"].AsInt / 100.0f;
			eData.stopTimeOnHit = jEnemy["hit_stop_time"].AsFloat;
			eData.hasShield = jEnemy["shield"].AsInt == 1;
			eData.isElemental = jEnemy["elemental"].AsInt == 1;

			_enemiesData[i] = eData;
		}

		_enemiesDataReady = true;
	}

	IEnumerator parseWaves()
	{
		string wavesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/__waves.txt";
		WWW www = new WWW(wavesUrl);
		//Debug.Log ("downloading... " + wavesUrl);

		yield return www;
		JSONNode jWaves = JSONNode.Parse (www.text).AsArray;
		_wavesData = new WaveData[jWaves.Count];

		for (int i = 0; i < jWaves.Count; i++)
		{
			_wavesData[i] = new WaveData();
			JSONNode jWave = jWaves[i];
			_wavesData[i].time = (float)jWave["time"].AsFloat;

			JSONArray jEnemies = jWave["enemies"].AsArray;
			if ( jEnemies.Count != kNumStreets ){
				Debug.LogWarning("JSON: wave" + i + " num streets failed (" + jEnemies.Count + ")");
			}

			for (int j = 0; j < jEnemies.Count; j++)
			{
				_wavesData[i].enemiesId[j] = jEnemies[j];
			}
		}

		_wavesDataReady = true;
	}

	EnemyController.Data getEnemyDataById(string id)
	{
		for (int i = 0; i < _enemiesData.Length; i++)
		{
			if ( _enemiesData[i].id == id)
			{
				return _enemiesData[i];
			}
		}

		return null;
	}

	static bool isValidEnemyId(string s)
	{
		return s != "_";
	}

	void spawnWave(WaveData wave)
	{
		// for each street....
		for ( int i = 0; i < wave.enemiesId.Length; i++ )
		{
			int street = i;
			if ( isValidEnemyId(wave.enemiesId[i]) )
			{
				GameObject prefab = null;
				string enemyId = wave.enemiesId[i];
				switch ( enemyId )
				{
				case "A":	prefab = _enemyPrefab_A;	break;
				case "B":	prefab = _enemyPrefab_B;	break;
				case "C":	prefab = _enemyPrefab_C;	break;
				case "D":	prefab = _enemyPrefab_D;	break;
				case "E":	prefab = _enemyPrefab_E;	break;
				}

				if (prefab)
				{
					GameObject go = (GameObject)GameObject.Instantiate(prefab, spawnPoints[street].position, Quaternion.identity);
					go.GetComponent<EnemyController>().Init(getEnemyDataById(enemyId), street);
				}
			}
		}
	}

	void onFinishedEnemies()
	{
	}

	void OnDrawGizmos()
	{
		foreach (Transform spt in spawnPoints) {
			Gizmos.DrawCube(spt.position, new Vector3(1,1,1) );
		}
	}
}
