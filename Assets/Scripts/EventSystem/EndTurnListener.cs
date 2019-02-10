using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTurnListener : GameEventListener
{
    public GameEvent updateUIEvent;
    public List<GameEvent> phases;

    public void ExecutePhases()
    {
        foreach (GameEvent phase in phases)
        {
            phase.Raise();
        }
        updateUIEvent.Raise();
    }
}
