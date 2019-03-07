using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCountManager : GameEventListener
{
    public IntReference turnCount;

    private void Awake()
    {
        turnCount.value = 0;
    }

    public void IncrementTurnCount()
    {
        turnCount.value++;
    }
}
