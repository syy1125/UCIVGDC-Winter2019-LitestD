using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PopulationProvider : MonoBehaviour
{
	public IntReference PopulationRef;
	public int ConstructionOrder { get; private set; }
	private static int _constructionCount;
	
	public readonly Stack<Sprite> Portraits = new Stack<Sprite>();
	public int ResidentCount => Portraits.Count;
	private int _capacity = -1;
	public int Capacity => _capacity >= 0 ? _capacity : _capacity = GetComponent<CapacityProvider>().CapacityDelta;

	private void Start()
	{
		ConstructionOrder = _constructionCount++;
	}

	public void RemoveResidents()
	{
		PopulationRef.value -= Portraits.Count;
		Portraits.Clear();
		GameManager.Instance.ResourceManager.PreventPopulationOverdraft();
	}

	private void OnDisable()
	{
		foreach (Sprite portrait in Portraits)
		{
			GameManager.Instance.ResourceManager.HousePopulation(portrait);
		}
	}
}