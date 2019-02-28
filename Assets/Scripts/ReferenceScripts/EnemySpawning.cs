using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemySpawning : ScriptableObject
{
    public AnimationCurve spawnCurve;
    public int maxTurn = 100;
    public int maxEnemiesToSpawn = 10;

    public int HowManyEnemiesToSpawn(int turnCount)
    {
        float time = turnCount > maxTurn ? 1 : (float)turnCount / maxTurn;
        int expectedNumberOfEnemies = Mathf.RoundToInt(maxEnemiesToSpawn * spawnCurve.Evaluate(time));
        return Mathf.Clamp(expectedNumberOfEnemies, 0, maxEnemiesToSpawn);
    }
}
