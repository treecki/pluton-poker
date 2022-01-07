using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    private Transform content;

    [SerializeField]
    private PlayerListing playerListingPrefab;
    private List<PlayerListing> listings = new List<PlayerListing>();

    private void Awake()
    {
        content = GetComponentInChildren<ScrollRect>().content;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        PlayerListing playerListing = Instantiate(playerListingPrefab, content);

        if (playerListing)
        {
            playerListing.SetPlayerInfo(newPlayer);
            listings.Add(playerListing);
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        int index = listings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(listings[index].gameObject);
            listings.RemoveAt(index);
        }
    }
}
