using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    protected GameState currentState;
    public GameState CurrentState { get { return currentState; } }

    public virtual void SetState(GameState inputState)
    {
        if (currentState != null)
        {
            currentState.ResetState();
        }
        currentState = inputState;
    }
}
