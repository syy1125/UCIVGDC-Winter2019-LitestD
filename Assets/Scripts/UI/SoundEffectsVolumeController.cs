using UnityEngine;

public class SoundEffectsVolumeController : MonoBehaviour
{
	private AudioSource _audio;

	private void Awake()
	{
		_audio = GetComponent<AudioSource>();
	}

	private void Start()
	{
		_audio.volume = OptionsMenu.GetEffectsVolume();
	}

	public void UpdateVolume()
	{
		_audio.volume = OptionsMenu.GetEffectsVolume();
	}
}