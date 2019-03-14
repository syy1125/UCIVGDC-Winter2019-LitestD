using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemySpawning : ScriptableObject
{
    public List<int> enemiesToSpawn;

    public int HowManyEnemiesToSpawn(int turnCount)
    {
        if (turnCount < enemiesToSpawn.Count)
        {
            return enemiesToSpawn[turnCount];
        }
        else
        {
            return enemiesToSpawn[enemiesToSpawn.Count - 1];
        }
    }
}
