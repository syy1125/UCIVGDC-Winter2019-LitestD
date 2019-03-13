using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	[Header("Components")]
	[FormerlySerializedAs("tutorialToggle")]
	public Toggle TutorialToggle;
	public Slider MusicVolumeSlider;
	public Slider EffectsVolumeSlider;

	[Header("Events")]
	public GameEvent EffectsVolumeChangedEvent;

	public const string SHOW_TUTORIAL_KEY = "ShowTutorial";
	public const string MUSIC_VOLUME_KEY = "MusicVolume";
	public const string EFFECTS_VOLUME_KEY = "EffectsVolume";

	private void Awake()
	{
		ToggleTutorial(PlayerPrefs.GetInt(SHOW_TUTORIAL_KEY, 1) == 1);
		SetMusicVolume(GetMusicVolume());
		SetEffectsVolume(GetEffectsVolume());
	}

	public static float GetMusicVolume()
	{
		return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1);
	}

	public static float GetEffectsVolume()
	{
		return PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 1);
	}

	public void ToggleTutorial(bool isOn)
	{
		PlayerPrefs.SetInt(SHOW_TUTORIAL_KEY, isOn ? 1 : 0);
		TutorialToggle.isOn = isOn;
	}

	public void SetMusicVolume(float volume)
	{
		PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
		MusicVolumeSlider.value = volume;
		MusicManager.Instance.SetMusicVolume(volume);
	}

	public void SetEffectsVolume(float volume)
	{
		PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, volume);
		EffectsVolumeSlider.value = volume;
		EffectsVolumeChangedEvent.Raise();
	}
}