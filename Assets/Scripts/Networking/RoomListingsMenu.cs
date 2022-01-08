using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomListingsMenu : MonoBehaviourPunCallbacks
{
    private Transform content;

    [SerializeField]
    [Tooltip("")]
    private RoomListing roomListingPrefab;
    private List<RoomListing> listings = new List<RoomListing>();

    private LobbyCanvases lobbyCanvases;

    private void Awake()
    {
        lobbyCanvases = GetComponentInParent<LobbyCanvases>();
        content = GetComponentInChildren<ScrollRect>().content;
    }

    public override void OnJoinedRoom()
    {
        lobbyCanvases.CurrRoomCanvas.Show();
        content.DestroyChildren();
        listings.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach(RoomInfo roomInfo in roomList)
        {
            if (roomInfo.RemovedFromList)
            {
                int index = listings.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
                if (index != -1)
                {
                    Destroy(listings[index].gameObject);
                    listings.RemoveAt(index);
                }
            }
            else
            {
                int index = listings.FindIndex(x => x.RoomInfo.Name == roomInfo.Name);
                if (index == -1)
                {
                    RoomListing roomListing = Instantiate(roomListingPrefab, content);
                    if (roomListing)
                    {
                        roomListing.SetRoomInfo(roomInfo);
                        listings.Add(roomListing);
                    }
                }
                else
                {
                    //Modify current listing here
                }

            }
        }
    }
}
