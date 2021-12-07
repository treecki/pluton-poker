using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerIndicator : MonoBehaviour
{
    private Image indicatorImage;
    private PlayerObject playerObject;

    public Color idleColor;
    public Color turnColor;
    public Color foldColor;

    private void Awake()
    {
        indicatorImage = GetComponent<Image>();
        playerObject = GetComponentInParent<PlayerObject>();
    }

    private void Update()
    {
        if (playerObject.Player.canInput)
        {
            indicatorImage.color = turnColor;
        }
        else if (playerObject.Player.IsFolded)
        {
            indicatorImage.color = foldColor;
        }
        else
        {
            indicatorImage.color = idleColor;
        }
    }
}
