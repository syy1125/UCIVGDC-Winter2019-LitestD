using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;
	
	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("A duplicate instance of GameManager is attempting to initialize. Destroying it.");
			Destroy(gameObject);
		}
	}
}
