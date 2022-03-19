using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainUi : MonoBehaviour
{
    public Sprite[] dice;
    public static int diceNum;

    public GameObject diceUi;
    public GameObject endUi;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI turnPlayerText;
    private string turnPlayer;

    public static TextMeshProUGUI idTextStatic;
    public static GameObject endUiStatic;
    public Button goButton;
    public PhotonView PV;
    public static string winner;
    public static string[] Players;

    public CreateAndJoinRooms createAndJoinRooms;

    private void Start()
    {
        endUiStatic = endUi;
        idTextStatic = idText;
        turnPlayerText.text = PhotonNetwork.PlayerList[0].NickName;

        if (PhotonNetwork.LocalPlayer.NickName != PhotonNetwork.PlayerList[0].NickName)
        {
            goButton.interactable = false;
        }
    }

    public void ClickGoButton()
    {
        PlayerScript.go = true;
        diceNum = Random.Range(1, 6);
        Debug.Log("주사위 : " + diceNum);
        diceUi.GetComponent<Image>().sprite = dice[diceNum - 1];

        goButton.interactable = false;
        PV.RPC("Delay", RpcTarget.All, 1f);    //~초 딜레이
    }

    public void ClickBackButton()
    {
        // PhotonNetwork.LoadLevel("Lobby");
        SceneManager.LoadScene("Lobby");
        PhotonNetwork.LeaveRoom();
    }

    IEnumerator Turn(float second)
    {
        //~초 딜레이
        yield return new WaitForSeconds(second);
        if (turnPlayerText.text == PhotonNetwork.PlayerList[0].NickName)
        {
            turnPlayerText.text = PhotonNetwork.PlayerList[1].NickName;
            if (PhotonNetwork.LocalPlayer.NickName == PhotonNetwork.PlayerList[1].NickName)
            {
                goButton.interactable = true;
            }
        }
        else
        {
            turnPlayerText.text = PhotonNetwork.PlayerList[0].NickName;
            if (PhotonNetwork.LocalPlayer.NickName == PhotonNetwork.PlayerList[0].NickName)
            {
                goButton.interactable = true;
            }
        }
    }

    [PunRPC]
    void Delay(float second)
    {
        StartCoroutine(Turn(second));
    }
}