using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CardFaces : ScriptableObject
{
    public Sprite[] cardFaces;

    public Sprite emptyCard;

    public string precursorString;

    public Sprite GetCardSprite(SUIT _suit, VALUE _value)
    {
        string vString = _value.ToString();
        string sString = _suit.ToString();
        string name = precursorString + sString + vString.Substring(1);

        Sprite cardFace = FindCardByName(name);
        if (cardFace)
        {
            return cardFace;
        }
        else
        {
            return emptyCard;
        }
    }

    public Sprite GetCardBack()
    {
        string name = precursorString + "Back";
        Sprite cardFace = FindCardByName(name);
        if (cardFace)
        {
            return cardFace;
        }
        else
        {
            return emptyCard;
        }
    }

    private Sprite FindCardByName(string name)
    {
        foreach (Sprite cardFace in cardFaces)
        {
            if (cardFace.name.Contains(name))
            {
                return cardFace;
            }
        }

        return null;
    }
}
