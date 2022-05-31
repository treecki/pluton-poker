using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;


public class MasterClientMonitor : MonoBehaviourPunCallbacks
{
    #region Types
    private class PlayerPing
    {
        public PlayerPing(Player player, int ping)
        {
            Player = player;
            pings.Add(ping);
        }

        public float LastUpdatedTime { get; private set; } = -1f;
        public readonly Player Player;
        private List<int> pings = new List<int>();
        private const int MAX_RECORDED_PINGS = 6;

        public int ReturnAveragePing()
        {
            //Not enough pings to consider an average
            if (pings.Count < MAX_RECORDED_PINGS)
            {
                return -1;
            }
            //Has enough pings to average
            else
            {
                int sum = pings.Sum();
                return Mathf.CeilToInt(sum / pings.Count);
            }
        }

        public void AddPing(int value)
        {
            if (pings.Count >= MAX_RECORDED_PINGS)
            {
                pings.RemoveAt(0);
            }
            pings.Add(value);
            LastUpdatedTime = Time.unscaledTime;
        }
    }
    #endregion

    #region Private
    private List<PlayerPing> playerPings = new List<PlayerPing>();
    //Next time to check for a high ping on master client
    private float nextCheckChangeMaster = 0f;
    //Number of times in a row the current master client has had a significantly higher ping
    private int consequtiveHighPingCount = 0;
    //True if received a master client change request. Only set on master client
    private bool pendingMasterChange = false;
    //Time a takeover request was sent. -1f if no takeover request is active
    private float takeoverRequestTime = -1f;
    //Next time to send a ping for this client
    private float nextSendPingTime = 0f;
    #endregion

    #region Const
    //How many times in a row the current masters ping must be significantly higher than others to forfeit master client.
    private const int HIGH_PING_TURNOVER_REQUIREMENT = 3;
    //Mimimum current master client pings must be higher than lowest pinging player to be considered significantly higher.
    private const int MIN_PING_DIFFERENCE = 50;
    //How often to check for lowest pings. Masterclient must have a high ping after HIGH_PING_TURNOVER_REQUIREMENT times.
    private const float PING_CHECK_INTERVAL = 5f;
    //Time master client has to grant a takeover request before it's taken forcefully.
    //It's preferred that the master grants the request to prevent multiple takeovers in a short duration,
    //but when master client is laggy or broken this may not be possible.
    private const float TAKEOVER_REQUEST_TIMEOUT = 3f;
    //How frequently to send this clients ping.
    private const float SEND_PING_INTERVAL = 5F;
    #endregion

    private void Update()
    {
        CheckSendPing();
        CheckChangeMaster();
        CheckTakeoverTimeout();
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //See if player pings, if so remove them
        int index = playerPings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            playerPings.RemoveAt(index);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        pendingMasterChange = false;
        takeoverRequestTime = -1f;
        consequtiveHighPingCount = 0;
    }

    private void CheckTakeoverTimeout()
    {
        if (takeoverRequestTime == -1f)
            return;

        if((Time.unscaledTime - takeoverRequestTime) > TAKEOVER_REQUEST_TIMEOUT)
        {
            takeoverRequestTime = -1f;
            SetNewMaster(PhotonNetwork.LocalPlayer);
        }
    }

    private void SetNewMaster(Player newMaster, bool resetHighPingCount = true)
    {
        if (resetHighPingCount)
            consequtiveHighPingCount = 0;

        PhotonNetwork.SetMasterClient(newMaster);
    }

    //Checks to change master if the current master is significantly more laggy than other players
    private void CheckChangeMaster()
    {
        //Network conditions not met.
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient)
            return;
        //Takeover already in progress.
        if (takeoverRequestTime != -1f)
            return;
        //Too recent since last check.
        if (Time.time < nextCheckChangeMaster)
            return;

        //Next time to check pings.
        nextCheckChangeMaster = Time.time + PING_CHECK_INTERVAL;

        /* Players should already be removed when leaving the room.
         * This is just an extra precautionary. */
        RemoveNullPlayers();

        //Get list of all players.
        Player[] players = PhotonNetwork.PlayerList;
        //If only one player.
        if (players.Length <= 1)
            return;

        int lowestAverageIndex = -1;
        int lowestAveragePing = -1;
        int masterPing = -1;
        int masterIndex = -1;

        foreach (Player player in players)
        {
            int pingsIndex = playerPings.FindIndex(x => x.Player == player);
            //Not found in players pings.
            if (pingsIndex == -1)
                continue;

            //If player being checked is this client.
            if (player == PhotonNetwork.LocalPlayer)
            {
                //If this client hasn't sent a ping in awhile don't even try to takeover.
                if ((Time.unscaledTime - playerPings[pingsIndex].LastUpdatedTime) >= (SEND_PING_INTERVAL * 2))
                    return;
            }

            //Get average.
            int averagePing = playerPings[pingsIndex].ReturnAveragePing();
            //If average isn't -1 then enough pings have been sent to calculate an average.
            if (averagePing != -1)
            {
                /* If average ping is lowest than the current lowest or
                 * the lowest average index is -1 (which means unset) then
                 * set as new lowest average. */
                if (averagePing < lowestAveragePing || lowestAverageIndex == -1)
                {
                    lowestAveragePing = averagePing;
                    lowestAverageIndex = pingsIndex;
                }
                //If player being checked is master client.
                if (playerPings[pingsIndex].Player.IsMasterClient)
                {
                    masterIndex = pingsIndex;

                    /* If master client hasn't send a ping within a reasonable time then
                     * set the master clients ping unrealistically high to force a high
                     * consequtive ping count. */
                    if ((Time.unscaledTime - playerPings[masterIndex].LastUpdatedTime) >= (SEND_PING_INTERVAL * 2))
                        masterPing = 999999999;
                    //Otherwise set to average ping.
                    else
                        masterPing = averagePing;
                }
            }
        }

        //If the lowest ping index couldn't be found.
        if (lowestAverageIndex == -1)
            return;
        //Master index couldn't be found.
        if (masterIndex == -1)
            return;
        /* If the lowest ping index isn't this client then
         * don't proceed further, let the lowest pinging player
         * try the take over when the time comes. */
        if (playerPings[lowestAverageIndex].Player != PhotonNetwork.LocalPlayer)
            return;

        /* If here this client is the lowest pinging player. */

        float masterPingDifference = (masterPing - lowestAveragePing);
        //If master ping is difference is high enough to change master client.
        if (masterPingDifference > MIN_PING_DIFFERENCE)
            consequtiveHighPingCount++;
        //master ping not too much higher.
        else
            consequtiveHighPingCount = 0;

        //If high ping 3 times in a row then request setting a new master.
        if (consequtiveHighPingCount >= HIGH_PING_TURNOVER_REQUIREMENT)
        {
            takeoverRequestTime = Time.unscaledTime;
            base.photonView.RPC("RPC_RequestMasterClient", RpcTarget.MasterClient, playerPings[lowestAverageIndex].Player);
        }
    }

    // Sends ping to all players including self.
    private void CheckSendPing()
    {
        if (Time.unscaledTime < nextSendPingTime)
            return;

        nextSendPingTime = Time.unscaledTime + SEND_PING_INTERVAL;

        base.photonView.RPC("RPC_ReceivePing", RpcTarget.All, PhotonNetwork.GetPing());
    }


    // Receives a ping for the specified player.
    [PunRPC]
    private void RPC_ReceivePing(Player player, int ping)
    {
        int index = playerPings.FindIndex(x => x.Player == player);
        if (index == -1)
            playerPings.Add(new PlayerPing(player, ping));
        else
            playerPings[index].AddPing(ping);
    }


    // Removes null players from player pings list.
    private void RemoveNullPlayers()
    {
        for (int i = 0; i < playerPings.Count; i++)
        {
            //If null player remove from list and decrease i.
            if (playerPings[i].Player == null)
            {
                playerPings.RemoveAt(i);
                i--;
            }
        }
    }


    // Request releasing the master client to the requestor.
    [PunRPC]
    private void RPC_RequestMasterClient(Player requestor)
    {
        //A change is already pending.
        if (pendingMasterChange)
            return;
        //If not master.
        if (!PhotonNetwork.IsMasterClient)
            return;

        pendingMasterChange = true;
        //RPC to allow change.
        base.photonView.RPC("RPC_MasterClientGranted", requestor);
    }


    // Received on a player which may takeover as master client.
    [PunRPC]
    private void RPC_MasterClientGranted()
    {
        //Set new master as self.
        SetNewMaster(PhotonNetwork.LocalPlayer);
    }

    // Called when the game gains or loses focus.
    private void OnApplicationPause(bool pause)
    {
        if (pause)
            LocallyHandOffMasterClient();
    }

    // Called when the game gains or loses focus.
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
            LocallyHandOffMasterClient();
    }


    // Hands off master client to the lowest pinging player at this time.
    private void LocallyHandOffMasterClient()
    {
        //Conditions where this does not apply.
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
            return;
        //Only player.
        if (PhotonNetwork.PlayerList.Length <= 1)
            return;

        //Other index which isn't this client.
        int otherIndex = -1;

        int lowestIndex = -1;
        int lowestPing = -1;
        for (int i = 0; i < playerPings.Count; i++)
        {
            //Skip self.
            if (playerPings[i].Player == PhotonNetwork.LocalPlayer)
                continue;

            otherIndex = i;

            int average = playerPings[i].ReturnAveragePing();
            //If new lowest or lowest isnt yet set.
            if (average < lowestPing || lowestIndex == -1)
            {
                lowestIndex = i;
                lowestPing = average;
            }
        }

        //If the lowest ping was found.
        if (lowestIndex != -1)
        {
            SetNewMaster(playerPings[lowestIndex].Player);
        }
        //Lowest ping not found. Maybe not enough data is collected.
        else
        {
            /* Can only proceed if an index of another player was found.
             * If so send to the last player checked which wasnt self. */
            if (otherIndex != -1)
                SetNewMaster(playerPings[otherIndex].Player);
        }
    }
}

