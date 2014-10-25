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
		public float delayTime;
		public SpawnData[] spawns;
	}

	class SpawnData
	{
		public float delayTime;
		public string[] enemiesId = new string[kNumStreets];
	}

	EnemyController.Data[] _enemiesData;
	WaveData[] _wavesData;
	
	bool _wavesDataReady = false;
	bool _enemiesDataReady = false;
	bool _isStarted = false;
	bool _canStart = false;

	float _lastWavetime = 0.0f;
	float _lastSpawnTime = 0.0f;

	int _currentWave = 0;
	int _currentSpawn = 0;

	int _aliveEnemiesCount = 0;
	
	enum WaveState {
		Spawning,
		WaitingDelay,
		WaitingLastEnemy,
		Finished
	};
	WaveState _waveState;

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
				_waveState = WaveState.WaitingDelay;
				_currentWave = 0;
				_lastWavetime = Time.fixedTime;
			}
		}

		switch (_waveState)
		{
		case WaveState.WaitingDelay:
			FixedUpdate_WaveState_WaitingDelay();
			break;
		case WaveState.WaitingLastEnemy:
			FixedUpdate_WaveState_WaitingLastEnemy();
			break;
		case WaveState.Spawning:
			FixedUpdate_WaveState_Spawning();
			break;
		case WaveState.Finished:
			FixedUpdate_WaveState_Finished();
			break;
		}
	}

	void FixedUpdate_WaveState_WaitingDelay()
	{
		if ( (Time.fixedTime - _lastWavetime) > _wavesData[_currentWave].delayTime )
		{
			_waveState = WaveState.Spawning;
			_lastSpawnTime = Time.fixedTime;
			_currentSpawn = 0;
		}
	}

	void FixedUpdate_WaveState_Spawning()
	{
		SpawnData spawn = _wavesData[_currentWave].spawns[_currentSpawn];
		if ( (Time.fixedTime - _lastSpawnTime) > spawn.delayTime )
		{
			doSpawn(spawn);

			_currentSpawn++;
			_lastSpawnTime = Time.fixedTime;

			if ( _currentSpawn == _wavesData[_currentWave].spawns.Length )
			{ // end of wave.
				_waveState = WaveState.WaitingLastEnemy;
			}
		}
	}

	void FixedUpdate_WaveState_WaitingLastEnemy()
	{
		if ( _aliveEnemiesCount == 0 )
		{
			_currentWave++;
			if ( _currentWave < _wavesData.Length )
			{
				_waveState = WaveState.WaitingDelay;
				_lastWavetime = Time.fixedTime;
			}
			else
			{
				// FINISH
				onFinishedEnemies();
			}
		}
	}

	void FixedUpdate_WaveState_Finished()
	{

	}

	void doSpawn(SpawnData spawn)
	{
		// for each street....
		for ( int i = 0; i < spawn.enemiesId.Length; i++ )
		{
			int street = i;
			if ( isValidEnemyId(spawn.enemiesId[i]) )
			{
				GameObject prefab = null;
				string enemyId = spawn.enemiesId[i];
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
					_aliveEnemiesCount++;
				}
			}
		}
	}


	IEnumerator parseEnemies()
	{
		string enemiesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/enemies.txt";
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
		string wavesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/waves.txt";
		WWW www = new WWW(wavesUrl);
		//Debug.Log ("downloading... " + wavesUrl);

		yield return www;
		JSONNode jWaves = JSONNode.Parse(www.text).AsArray;
		_wavesData = new WaveData[jWaves.Count];

		for (int i = 0; i < jWaves.Count; i++)
		{
			_wavesData[i] = parseWavesData(jWaves[i]);
		}

		_wavesDataReady = true;
	}

	WaveData parseWavesData(JSONNode jWave)
	{
		WaveData waveData = new WaveData();

		waveData.delayTime = jWave["delay_time"].AsFloat;
		JSONArray jSpawns = jWave["spawns"].AsArray;

		waveData.spawns = new SpawnData[jSpawns.Count];
		for (int i = 0; i < jSpawns.Count; i++)
		{
			SpawnData spawnData = waveData.spawns[i] = new SpawnData();
			JSONNode jSpawn = jSpawns[i];
			spawnData.delayTime = (float)jSpawn["delay_time"].AsFloat;
			
			JSONArray jEnemies = jSpawn["enemies"].AsArray;
			if ( jEnemies.Count != kNumStreets ){
				Debug.LogWarning("JSON: wave" + i + " num streets failed (" + jEnemies.Count + ")");
			}
			
			for (int j = 0; j < jEnemies.Count; j++)
			{
				spawnData.enemiesId[j] = jEnemies[j];
			}
		}

		return waveData;
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

	void onFinishedEnemies()
	{
	}

	void OnDrawGizmos()
	{
		foreach (Transform spt in spawnPoints) {
			Gizmos.DrawCube(spt.position, new Vector3(1,1,1) );
		}
	}

	public void onEnemyDied()
	{
		_aliveEnemiesCount--;
	}
}
