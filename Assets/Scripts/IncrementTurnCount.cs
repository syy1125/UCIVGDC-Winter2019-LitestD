using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncrementTurnCount : MonoBehaviour
{
    public int turnCount = 0;

    public void Increment()
    {
        turnCount++;
        print("Turn " + turnCount.ToString());
    }
}
