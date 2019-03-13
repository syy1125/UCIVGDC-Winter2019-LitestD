﻿using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
	[FormerlySerializedAs("tutorialToggle")]
	public Toggle TutorialToggle;
	public Slider MusicVolumeSlider;
	public Slider EffectsVolumeSlider;

	private const string SHOW_TUTORIAL_KEY = "ShowTutorial";
	private const string MUSIC_VOLUME_KEY = "MusicVolume";
	private const string EFFECTS_VOLUME_KEY = "EffectsVolume";

	private void Awake()
	{
		ToggleTutorial(PlayerPrefs.GetInt(SHOW_TUTORIAL_KEY, 1) == 1);
		SetMusicVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1));
		SetEffectsVolume(PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY, 1));
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
	}
}