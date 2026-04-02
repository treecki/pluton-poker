using System;

[Serializable]
public class PokerActionCommand
{
    // This is the betting command payload we send across Photon: client -> authority as a
    // request, then authority -> all clients again once that action has been validated/applied.
    public PokerActionType ActionType;
    public int ActorNumber;
    public int PlayerId;
    public float Amount;
    public string RequestedAtUtc;

    public static PokerActionCommand Create(PokerActionType actionType, int actorNumber, int playerId, float amount = 0f)
    {
        return new PokerActionCommand
        {
            ActionType = actionType,
            ActorNumber = actorNumber,
            PlayerId = playerId,
            Amount = amount,
            RequestedAtUtc = DateTime.UtcNow.ToString("o")
        };
    }
}

public enum PokerActionType
{
    Fold = 0,
    Check = 1,
    Call = 2,
    Raise = 3,
    AutoFold = 4
}
