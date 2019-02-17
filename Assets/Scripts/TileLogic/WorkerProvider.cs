﻿using System.Collections.Generic;
using UnityEngine;

public class WorkerProvider : MonoBehaviour
{
	public IntReference PopulationRef;
	public IntReference WorkerCountRef;

	public string JobName;

	public int AssignedCount
	{
		get { return WorkerPortraits.Count; }
	}

	public int Capacity { get; private set; }
	public Stack<Sprite> WorkerPortraits { get; private set; }

	private void Start()
	{
		Capacity = GetComponent<CapacityProvider>().CapacityRef;
		WorkerPortraits = new Stack<Sprite>();
	}

	public void AssignWorker(Sprite portrait)
	{
		WorkerCountRef.value += 1;
		WorkerPortraits.Push(portrait);
	}

	public Sprite UnassignWorker()
	{
		WorkerCountRef.value -= 1;
		return WorkerPortraits.Pop();
	}

	public void KillWorkers()
	{
		WorkerCountRef.value -= WorkerPortraits.Count;
		PopulationRef.value -= WorkerPortraits.Count;
	}
}