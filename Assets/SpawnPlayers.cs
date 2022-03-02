using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject playerPrefab;

    public float minX;
    public float minZ;
    public float maxX;
    public float maxZ;

    private void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 1.5f, Random.Range(minZ, maxZ));
        //Vector3 randomPosition = new Vector3(-8, 0.3f, -7);
        // Debug.Log("player : " + PhotonNetwork.PlayerList[0].NickName + " player2 : " + PhotonNetwork.PlayerList[1].NickName);
        // MainUi.Players[0] = PhotonNetwork.PlayerList[0].NickName;
        // MainUi.Players[1] = PhotonNetwork.PlayerList[1].NickName;
        // Debug.Log("p[0] " + MainUi.Players[0] + "p[1]" + MainUi.Players[1]);
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity).name = PhotonNetwork.PlayerList[0].NickName;
            // MainUi.Players[0] = PhotonNetwork.PlayerList[0].NickName;
            // MainUi.Players[1] = PhotonNetwork.PlayerList[1].NickName;
            // Debug.Log("p[0]" + MainUi.Players[0]);
        }
        else
        {
            PhotonNetwork.Instantiate(playerPrefab.name, randomPosition, Quaternion.identity).name = PhotonNetwork.PlayerList[1].NickName;
            // MainUi.Players[0] = PhotonNetwork.PlayerList[0].NickName;
            // MainUi.Players[1] = PhotonNetwork.PlayerList[1].NickName;
            // Debug.Log("p[1]" + MainUi.Players[1]);
        }
    }
}