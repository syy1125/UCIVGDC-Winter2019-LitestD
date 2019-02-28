using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
	public int AttackStrength = 3;

	[Header("Effects")]
	public float MinPitch;
	public float MaxPitch;

	private AudioSource _audio;
	private AudioSource Audio => _audio ? _audio : _audio = GetComponent<AudioSource>();

	public void PlayAttackSound()
	{
		Audio.pitch = Random.Range(MinPitch, MaxPitch);
		Audio.Play();
	}
}