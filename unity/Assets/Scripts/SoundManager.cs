using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public AudioClip shieldBlock;
	public AudioClip shootBoomerang;
	public AudioClip hit;
	public AudioClip shootBabosa;
	public AudioClip shootFire;

	public AudioSource _sourceSoundEffect;
	public AudioSource _sourceMusic;

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
	}

	public void playSoundEffect(AudioClip clip)
	{
		_sourceSoundEffect.audio.PlayOneShot(clip, 1.0f);
	}
}
