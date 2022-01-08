using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomCustomPropertyGenerator : MonoBehaviour
{
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    private void Start()
    {
        SetCustomNumber();
    }

    private void SetCustomNumber()
    {
        System.Random rnd = new System.Random();
        int result = rnd.Next(0, 99);

        text.text = result.ToString();

        _myCustomProperties["RandomNumber"] = result;
        //PhotonNetwork.LocalPlayer.CustomProperties = _myCustomProperties;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }


    public void OnClickButton()
    {
        SetCustomNumber();
    }
}
