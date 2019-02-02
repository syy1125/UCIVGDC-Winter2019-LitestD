using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnCountManager : GameEventListener
{
    public IntReference turnCountRef;

    private void Start()
    {
        LoadTurnCount();
    }

    private void LoadTurnCount()
    {
        turnCountRef.value = 1;
    }

    public void IncrementTurnCount()
    {
        turnCountRef.value++;
    }
}
