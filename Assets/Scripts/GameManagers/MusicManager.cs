using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	[Header("Music")]
	public AudioClip MenuMusic;
	public AudioClip PeacefulMusic;
	public AudioClip InvasionMusic;

	[Header("Config")]
	public float TransitionDuration;

	public static MusicManager Instance;
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
		float startVolume = _audio.volume;
		float startTime = Time.time;

		while (Time.time - startTime < TransitionDuration)
		{
			_audio.volume = Mathf.Lerp(startVolume, targetVolume, (Time.time - startTime) / TransitionDuration);
			yield return null;
		}

		_audio.volume = targetVolume;
	}

	public void TransitionToMenu()
	{
		ReplaceCoroutine(ref _trackTransition, TransitionTrackCoroutine(MenuMusic));
	}

	public void TransitionToPeaceful()
	{
		ReplaceCoroutine(ref _trackTransition, TransitionTrackCoroutine(PeacefulMusic));
	}

	public void TransitionToInvasion()
	{
		ReplaceCoroutine(ref _trackTransition, TransitionTrackCoroutine(InvasionMusic));
	}

	private IEnumerator TransitionTrackCoroutine(AudioClip newTrack)
	{
		yield return TransitionToVolume(0);
		_audio.clip = newTrack;
		yield return TransitionToVolume(1);
	}
}