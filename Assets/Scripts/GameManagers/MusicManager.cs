using System;
using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public static MusicManager Instance;

	[Header("Music")]
	public AudioClip MenuMusic;
	public AudioClip PeacefulMusic;
	public AudioClip InvasionMusic;

	[Header("Config")]
	public float TransitionDuration;

	private float _musicVolume;
	private AudioSource _audio;

	private Coroutine _volumeTransition;
	private Coroutine _trackTransition;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (this != Instance)
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		_audio = GetComponent<AudioSource>();
		_audio.volume = 0;
		TransitionToVolume(1);
	}

	private Coroutine ReplaceCoroutine(ref Coroutine coroutine, IEnumerator enumerator)
	{
		if (coroutine != null) StopCoroutine(coroutine);
		return coroutine = StartCoroutine(enumerator);
	}

	private Coroutine TransitionToVolume(float targetVolume)
	{
		return ReplaceCoroutine(ref _volumeTransition, VolumeTransitionCoroutine(targetVolume));
	}

	private IEnumerator VolumeTransitionCoroutine(float targetVolume)
	{
		float startVolume = _musicVolume < 1E-10 ? 0 : _audio.volume / _musicVolume;
		float startTime = Time.time;

		while (Time.time - startTime < TransitionDuration)
		{
			_audio.volume =
				Mathf.Lerp(startVolume, targetVolume, (Time.time - startTime) / TransitionDuration)
				* _musicVolume;
			yield return null;
		}

		_audio.volume = targetVolume * _musicVolume;
	}

	public void TransitionToTrack(AudioClip newTrack)
	{
		ReplaceCoroutine(ref _trackTransition, TransitionTrackCoroutine(newTrack));
	}

	private IEnumerator TransitionTrackCoroutine(AudioClip newTrack)
	{
		yield return TransitionToVolume(0);
		_audio.clip = newTrack;
		_audio.Play();
		yield return TransitionToVolume(1);
	}

	public void SetMusicVolume(float volume)
	{
		_musicVolume = volume;
		_audio.volume = volume;
	}
}