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
		_audio.clip = MenuMusic;
		_audio.volume = 0;
		TransitionToVolume(1);
	}

	private Coroutine TransitionToVolume(float targetVolume)
	{
		StopCoroutine(_volumeTransition);
		return _volumeTransition = StartCoroutine(VolumeTransitionCoroutine(targetVolume));
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

		_audio.volume = 0;
	}

	public void TransitionToMenu()
	{
		StopCoroutine(_trackTransition);
		_trackTransition = StartCoroutine(TransitionTrackCoroutine(MenuMusic));
	}

	public void TransitionToPeaceful()
	{
		StopCoroutine(_trackTransition);
		_trackTransition = StartCoroutine(TransitionTrackCoroutine(PeacefulMusic));
	}

	public void TransitionToInvasion()
	{
		StopCoroutine(_trackTransition);
		_trackTransition = StartCoroutine(TransitionTrackCoroutine(InvasionMusic));
	}

	private IEnumerator TransitionTrackCoroutine(AudioClip newTrack)
	{
		yield return TransitionToVolume(0);
		_audio.clip = newTrack;
		yield return TransitionToVolume(1);
	}
}