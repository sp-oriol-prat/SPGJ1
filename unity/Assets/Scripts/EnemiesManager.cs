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
				Debug.Log ("STARTEDD!!!");
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
		string enemiesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/enemies_v2.txt";
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
			eData.damage = jEnemy["damage"].AsInt;
			eData.walk_speed = (float)jEnemy["walk_speed"].AsInt / 100.0f;
			eData.attack_speed = (float)jEnemy["attack_speed"].AsInt / 100.0f;
			int j = 0;
			foreach ( JSONNode inmunity in jEnemy["inmunities"].AsArray )
			{
				switch ( inmunity.ToString() )
				{
				case "fire":
					eData.inmunity[(int)EnemyController.Element.Fire.GetTypeCode()] = inmunity.AsBool;
					break;
					
				case "ice":
					eData.inmunity[(int)EnemyController.Element.Ice.GetTypeCode()] = inmunity.AsBool;
					break;
					
				case "wind":
					eData.inmunity[(int)EnemyController.Element.Wind.GetTypeCode()] = inmunity.AsBool;
					break;
				}
				
				j++;				
			}

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
				string enemyId = jEnemies[j].ToString();
				enemyId = enemyId.Substring(1, enemyId.Length-2);
				_wavesData[i].enemiesId[j] = isValidEnemyId(enemyId) ? enemyId : "";
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
		return s != " " && s != "";
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
				}

				if (prefab)
				{
					GameObject go = (GameObject)GameObject.Instantiate(prefab, spawnPoints[i].position, Quaternion.identity);
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
