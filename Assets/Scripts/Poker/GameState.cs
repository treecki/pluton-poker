using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class GameState
{
    protected PokerStateMachine psm;

    public Action OnStateStart = delegate { };
    public Action OnStateEnd = delegate { };

    public GameState(PokerStateMachine _psm) : base()
    {
        psm = _psm;
    }

    public virtual void Run()
    {
        OnStateStart();
    }

    public virtual void ResetState()
    {
        OnStateEnd();
    }

}
