using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDataType : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private MyCustomSerialization customSeralization = new MyCustomSerialization();
    [SerializeField]
    private bool sendAsTyped = true;

    private void Start()
    {
        //This only needs to go once, it registers the class if you are sending the class (MyCustomSerialization)
        //The letter represents the class and needs to be unique, W, V, Q, and P are being used for Vector2, Vector3, Quaternion, and Player
        PhotonPeer.RegisterType(typeof(MyCustomSerialization), (byte)'M', MyCustomSerialization.Serialize, MyCustomSerialization.Deserialize);
    }

    private void Update()
    {
        if (customSeralization.MyNumber != -1)
        {
            SendCustomSerialization(customSeralization, sendAsTyped);
            customSeralization.MyNumber = -1;
            customSeralization.MyString = string.Empty;
        }
    }

    private void SendCustomSerialization(MyCustomSerialization customSeralization, bool sendAsTyped)
    {
        if (!sendAsTyped)
            base.photonView.RPC("RPC_ReceiveMyCustomSerialization", RpcTarget.AllViaServer, MyCustomSerialization.Serialize(customSeralization));
        else
            base.photonView.RPC("RPC_TypedReceiveMyCustomSerialization", RpcTarget.AllViaServer, customSeralization);
    }

    [PunRPC]
    private void RPC_ReceiveMyCustomSerialization(byte[] datas)
    {
        MyCustomSerialization result = (MyCustomSerialization)MyCustomSerialization.Deserialize(datas);
        print("Received byte array: " + result.MyNumber + ", " + result.MyString);
    }

    [PunRPC]
    private void RPC_TypedReceiveMyCustomSerialization(MyCustomSerialization datas)
    {
        print("Received byte array: " + datas.MyNumber + ", " + datas.MyString);
    }
}
