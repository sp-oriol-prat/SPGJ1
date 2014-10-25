using UnityEngine;
using System.Collections;
using SimpleJSON;

public class EnemiesManager : MonoBehaviour
{
	public Transform[] spawnPoints;

	static int kNumStreets = 6;

	enum Element { Fire = 0, Ice = 1, Wind = 2 };
	class EnemyData {
		public string id;
		public int life;
		public int damage;
		public bool[] inmunity = new bool[3]; // Fire, Ice, Wind
	};

	GameObject _enemyPrefab_A;
	GameObject _enemyPrefab_B;
	GameObject _enemyPrefab_C;
	
	class WaveData
	{
		public float time;
		public string[] enemiesId = new string[kNumStreets];
	}

	EnemyData[] _enemiesData;
	WaveData[] _wavesData;

	int _currentWave;
	bool _dataReady;
	float _onStartTime;


	void Start()
	{
		_currentWave = 0;
		_onStartTime = 0;
		_dataReady = false;

		StartCoroutine(parseEnemies());
		StartCoroutine(parseWaves());

		_enemyPrefab_A = Resources.Load("Enemy_A") as GameObject;
		_enemyPrefab_B = Resources.Load("Enemy_B") as GameObject;
		_enemyPrefab_C = Resources.Load("Enemy_C") as GameObject;
	}
	

	void Update ()
	{
	}

	void FixedUpdate()
	{
		if (!_dataReady)
		{
			return;
		}

		if (_currentWave == _wavesData.Length)
		{
			return;
		}

		float t = Time.fixedTime;
		float dt = t - _onStartTime;
		Debug.Log("dt: " + dt);
		if ( dt > _wavesData[_currentWave].time )
		{
			spawnWave(_wavesData[_currentWave]);
			_currentWave++;
		}
	}
	
	IEnumerator parseEnemies()
	{
		string enemiesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/enemies.txt";
		WWW www = new WWW(enemiesUrl);
		Debug.Log ("downloading... " + enemiesUrl);

		yield return www;
		JSONNode jEnemies = JSONNode.Parse(www.text).AsArray;
		for (int i = 0; i < jEnemies.Count; i++)
		{
			JSONNode jEnemy = jEnemies[i];
			
			EnemyData eData = new EnemyData();
			eData.id = jEnemy["id"].ToString();
			eData.life = jEnemy["life"].AsInt;
			eData.damage = jEnemy["damage"].AsInt;
			int j = 0;
			foreach ( JSONNode inmunity in jEnemy["inmunities"].AsArray )
			{
				switch ( inmunity.ToString() )
				{
				case "fire":
					eData.inmunity[(int)Element.Fire.GetTypeCode()] = inmunity.AsBool;
					break;
					
				case "ice":
					eData.inmunity[(int)Element.Ice.GetTypeCode()] = inmunity.AsBool;
					break;
					
				case "wind":
					eData.inmunity[(int)Element.Wind.GetTypeCode()] = inmunity.AsBool;
					break;
				}
				
				j++;				
			}
		}
	}

	IEnumerator parseWaves()
	{
		string wavesUrl = "https://dl.dropboxusercontent.com/u/64292958/spgj1/waves.txt";
		WWW www = new WWW(wavesUrl);
		Debug.Log ("downloading... " + wavesUrl);

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

		_onStartTime = Time.fixedTime;
		_dataReady = true;
	}

	static bool isValidEnemyId(string s)
	{
		return s != " " && s != "";
	}

	void spawnWave(WaveData wave)
	{
		for ( int i = 0; i < wave.enemiesId.Length; i++ )
		{
			if ( isValidEnemyId(wave.enemiesId[i]) )
			{
				GameObject prefab = null;
				switch ( wave.enemiesId[i] )
				{
				case "A":	prefab = _enemyPrefab_A;	break;
				case "B":	prefab = _enemyPrefab_B;	break;
				case "C":	prefab = _enemyPrefab_C;	break;
				}

				if (wave.enemiesId[i] == "A")
				{
					prefab = _enemyPrefab_A;
				}
				else if ( wave.enemiesId[i] == "B")
				{
					prefab = _enemyPrefab_B;
				}
				else if ( wave.enemiesId[i] == "C" )
				{
					prefab = _enemyPrefab_C;
				}

				if (prefab)
				{
					GameObject go = (GameObject)GameObject.Instantiate(prefab, spawnPoints[i].position, Quaternion.identity);
					go.GetComponent<EnemyController>().init(2);
				}
			}
		}
	}

	void OnDrawGizmos()
	{
		foreach (Transform spt in spawnPoints) {
			Gizmos.DrawCube(spt.position, new Vector3(1,1,1) );
		}
	}
}
