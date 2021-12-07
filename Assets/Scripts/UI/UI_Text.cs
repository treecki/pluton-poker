using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_Text : MonoBehaviour
{
    TMP_Text text;

    protected virtual void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public void SetText(string _text)
    {
        text.text = _text;
    }
}
