using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public GameObject nickNameUi;
    public GameObject lobbyUi;
    public GameObject roomUi;

    public TMP_InputField nickNameInput;
    public TMP_InputField roomCreateText;

    public TextMeshProUGUI playerCount;
    public TextMeshProUGUI nickNameText;
    public TextMeshProUGUI roomTitleText;
    public TextMeshProUGUI playerListText;
    public static TextMeshProUGUI nickNameTextStatic;

    public Button[] CellButton;
    public Button PreviousButton;
    public Button NextButton;

    //방
    public List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public GameObject playerImg;
    public Sprite playerReadyImg;
    public GameObject gameStart;
    public GameObject gameReady;

    //룸 유저 정보
    //public Image roomMasterImage;
    //public Image roomPlayerImage;
    public TextMeshProUGUI roomMasterText;
    public TextMeshProUGUI roomPlayerText;

    public PhotonView PV;

    //방리스트 갱신
    public void MyListClick(int num)
    {
        Debug.Log("Cell : " + CellButton[multiple + num]);
        Debug.Log("num : " + num + " maxPage : " + maxPage + " multiple : " + multiple + " myList : " + myList.Count);
        Debug.Log("[multiple + num] : " + (multiple + num) + " myList[multiple + num].Name : " + myList[multiple + num].Name);
        if (num == -2) --currentPage;
        else if (num == -1) ++currentPage;
        else PhotonNetwork.JoinRoom(myList[multiple + num].Name);
        MyListRenewal();
    }

    void MyListRenewal()
    {
        // 최대페이지
        maxPage = (myList.Count % CellButton.Length == 0) ? myList.Count / CellButton.Length : myList.Count / CellButton.Length + 1;

        // 이전, 다음버튼
        PreviousButton.interactable = (currentPage <= 1) ? false : true;
        NextButton.interactable = (currentPage >= maxPage) ? false : true;

        // 페이지에 맞는 리스트 대입
        multiple = (currentPage - 1) * CellButton.Length;
        for (int i = 0; i < CellButton.Length; i++)
        {
            CellButton[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellButton[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "";
            CellButton[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
        }
        Debug.Log("myList[] : " + myList[0] + " myList.count : " + myList.Count);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("dddd roomList : " + roomList.Count);
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            Debug.Log("dddd roomList[i].RemovedFromList : " + roomList[i].RemovedFromList);
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) 
                {
                    Debug.Log("myList 1 : " + myList.Count);
                    myList.Add(roomList[i]);
                    Debug.Log("myList 2 : " + myList.Count);
                    Debug.Log("roomList[i] : " + myList[i]);
                }
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }
    //방 리스트

    //방    
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomCreateText.text == "" ? "Room" + Random.Range(0, 100) : roomCreateText.text, new RoomOptions { MaxPlayers = 2 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnJoinedRoom()
    {
        roomUi.SetActive(true);
        lobbyUi.SetActive(false);
        if(PhotonNetwork.IsMasterClient)
        {
            //방장이면 게임시작
            gameStart.SetActive(true);
        }else{
            //플레이어면 게임준비
            gameReady.SetActive(true);
        }
        RoomRenewal();
    }

    public override void OnCreateRoomFailed(short returnCode, string message) { roomCreateText.text = ""; CreateRoom(); }

    public override void OnJoinRandomFailed(short returnCode, string message) { roomCreateText.text = ""; CreateRoom(); }

    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RoomRenewal();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
    }    

    void RoomRenewal()
    {
        playerListText.text = "";
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            playerListText.text += PhotonNetwork.PlayerList[i].NickName + ((i + 1 == PhotonNetwork.PlayerList.Length) ? "" : ", ");
        }

        roomMasterText.text = PhotonNetwork.PlayerList[0].NickName;
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            roomPlayerText.text = PhotonNetwork.PlayerList[1].NickName;
        }

        roomTitleText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }

    public void ClickRoomBackButton()
    {
        lobbyUi.SetActive(true);
        roomUi.SetActive(false);
    }

    public void ClickGameStart()
    {
        PV.RPC("EnterGame", RpcTarget.All);
    }


    public void ClickGameReady()
    {
        PV.RPC("ReadyGame", RpcTarget.All);
    }

    [PunRPC]
    void ReadyGame()
    {
        if(playerImg.GetComponent<Image>().sprite == null)
        {
            playerImg.GetComponent<Image>().sprite = playerReadyImg;
        }
        else
        {
            playerImg.GetComponent<Image>().sprite = null;
        }
    }

    [PunRPC]
    void EnterGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers && playerImg.GetComponent<Image>().sprite != null)
        {
            Debug.Log("게임시작이미지 이름 : " + playerImg.name);
            PhotonNetwork.LoadLevel("Game");
        }
    }
    //방

    private void Awake()
    {
        nickNameTextStatic = nickNameText;
        if(PhotonNetwork.LocalPlayer.NickName != "")
        {
            Debug.Log("PhotonNetwork.LocalPlayer.NickName : " + PhotonNetwork.LocalPlayer.NickName);
            nickNameUi.SetActive(false);
            lobbyUi.SetActive(true);
        }
    }

    private void Update()
    {
        playerCount.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " Lobby / " + PhotonNetwork.CountOfPlayers + " Connect";
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public void ConnectLobby()
    {
        PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
        nickNameText.text = PhotonNetwork.LocalPlayer.NickName;
        nickNameUi.SetActive(false);
        lobbyUi.SetActive(true);
        roomUi.SetActive(false);
        // myList.Clear();
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        nickNameUi.SetActive(true);
        lobbyUi.SetActive(false);
        roomUi.SetActive(false);
    }

    public void ClickLobbyBackButton()
    {
        lobbyUi.SetActive(false);
        nickNameUi.SetActive(true);
    }

    public void ClickBackButton()
    {
        Application.Quit();
    }
}