﻿using UnityEngine;

[CreateAssetMenu(menuName = "References/Int")]
public class IntReference : ScriptableObject
{
	public int value;

	public static implicit operator int(IntReference ir)
	{
		return ir.value;
	}

	public override string ToString()
	{
		return value.ToString();
	}
}