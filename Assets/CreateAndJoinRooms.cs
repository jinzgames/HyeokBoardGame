using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System;
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

    public Button[] CellButton;
    public Button PreviousButton;
    public Button NextButton;

    //��
    public List<RoomInfo> myList = new List<RoomInfo>();
    int currentPage = 1, maxPage, multiple;

    public GameObject playerImg;
    public Sprite playerReadyImg;
    public GameObject gameStart;
    public GameObject gameReady;

    //�� ���� ����
    //public Image roomMasterImage;
    //public Image roomPlayerImage;
    public TextMeshProUGUI roomMasterText;
    public TextMeshProUGUI roomPlayerText;
    public PhotonView PV;
    public TextMeshProUGUI logText;
    public static Dictionary<string, string> playerData = new Dictionary<string, string>();
    public static int playerWin = 0;
    public TextMeshProUGUI playerDataText;

    //�ű����� ���뵥���� �Է�
    public void ClientGetTitleData()
    {
        List<StatisticUpdate> li = new List<StatisticUpdate>();
        Dictionary<string, string> di = new Dictionary<string, string>();

        logText.text = "new player";
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result =>
            {
                foreach (var v in result.Data)
                {
                    //Stat (���)
                    if (v.Key.Equals("Stat_Win"))
                    {
                        li.Add(new StatisticUpdate { StatisticName = v.Key, Value = int.Parse(v.Value) });
                        // NewSetStat(v.Key, int.Parse(v.Value));
                    }
                    //PlayerData_
                    else
                    {
                        //�÷��̾���� Ÿ��Ʋ
                        di.Add(v.Key, v.Value);
                    }
                }
                NewSetStat(li);
                SetUserData(di);
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
            }
        );
    }

    // userStatisticsData

    //�÷��̾� ������(Ÿ��Ʋ) ����
    public static void SetUserData(Dictionary<string, string> data)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = data,
            Permission = UserDataPermission.Public
        },
        result =>
        {
            Debug.Log("Successfully updated user data");
        },
        error =>
        {
            Debug.Log("Got error setting user data Ancestor to Arthur");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    //�������
    public void NewSetStat(List<StatisticUpdate> li)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = li
            // Statistics = new List<StatisticUpdate>
            // {
            //     new StatisticUpdate {StatisticName = name, Value = value}
            // }
        },
        (result) =>
        {
            Debug.Log("�� �����");
        },
        (error) =>
        {
            Debug.Log("�� ���� ����");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    //win ����
    public static void SetWin()
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate {StatisticName = "Stat_Win", Value = playerWin}
            }
        },
        (result) =>
        {
            Debug.Log("�� �����");
        },
        (error) =>
        {
            Debug.Log("�� ���� ����");
        });
    }

    public void GetWin()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
        (result) =>
        {
            Debug.Log("�ҷ��� �� : " + result.Statistics[0].StatisticName + " : " + result.Statistics[0].Value + " \n");
            playerWin = result.Statistics[0].Value;

            // foreach (var stat in result.Statistics)
            // {
            //     if (stat.StatisticName == "Stat_Win")
            //     {
            //         Debug.Log("�ҷ��� �� : " + stat.StatisticName + " : " + stat.Value + " \n");
            //         playerWin = stat.Value;
            //     }
            //     else
            //     {
            //         Debug.Log("stat.StatisticName : " + stat.StatisticName);
            //     }
            // }
        },
        (error) => { Debug.Log("�ҷ����� ����"); });
    }

    //ó�� ����� üũ
    void GetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
        result =>
        {
            logText.text = "result.Data.Count : " + result.Data.Count;
            if (result.Data.Count < 1)
            {
                logText.text = "result.Data.Count2 : " + result.Data.Count;
                ClientGetTitleData();
                playerDataText.text = "ȯ���մϴ�";
            }
            else
            {
                foreach (var v in result.Data)
                {
                    playerData.Add(v.Key, v.Value.Value);
                }

                playerDataText.text = "NickName : " + playerData["PlayerData_NickName"] + " Lose : " + playerData["PlayerData_Lose"] + " Money : " + playerData["PlayerData_Money"];
                // logText.text = "v.Value : " + playerData["PlayerData_NickName"];
                PhotonNetwork.LocalPlayer.NickName = playerData["PlayerData_NickName"];
                nickNameText.text = PhotonNetwork.LocalPlayer.NickName;
                nickNameUi.SetActive(false);
                lobbyUi.SetActive(true);
            }
        }, (error) =>
        {
            Debug.Log("Got error retrieving user data:");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    void SetUserData(string name)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {
                    "PlayerData_NickName", name
                }
            },
            Permission = UserDataPermission.Public
        },
        result => Debug.Log("Successfully updated user data"),
        error =>
        {
            Debug.Log("Got error setting user data Ancestor to Arthur");
            Debug.Log(error.GenerateErrorReport());
        });
    }

    private void Awake()
    {
        playerDataText.text = "ȯ���մϴ�";

        if (PhotonNetwork.LocalPlayer.NickName != "")
        {
            Debug.Log("PhotonNetwork.LocalPlayer.NickName : " + PhotonNetwork.LocalPlayer.NickName);
            nickNameText.text = PhotonNetwork.LocalPlayer.NickName;
            nickNameUi.SetActive(false);
            lobbyUi.SetActive(true);
        }
        else
        {
            GetUserData();
        }
    }

    private void Update()
    {
        playerCount.text = (PhotonNetwork.CountOfPlayers - PhotonNetwork.CountOfPlayersInRooms) + " Lobby / " + PhotonNetwork.CountOfPlayers + " Connect";
    }

    //�渮��Ʈ ����
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
        // �ִ�������
        maxPage = (myList.Count % CellButton.Length == 0) ? myList.Count / CellButton.Length : myList.Count / CellButton.Length + 1;

        // ����, ������ư
        PreviousButton.interactable = (currentPage <= 1) ? false : true;
        NextButton.interactable = (currentPage >= maxPage) ? false : true;

        // �������� �´� ����Ʈ ����
        multiple = (currentPage - 1) * CellButton.Length;
        for (int i = 0; i < CellButton.Length; i++)
        {
            CellButton[i].interactable = (multiple + i < myList.Count) ? true : false;
            CellButton[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "���";
            CellButton[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
            //���� �̸� ǥ���ϱ�
            // CellButton[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i]. : "";
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
    //�� ����Ʈ

    //��    
    public void CreateRoom() => PhotonNetwork.CreateRoom(roomCreateText.text == "" ? "Room" + UnityEngine.Random.Range(0, 100) : roomCreateText.text, new RoomOptions { MaxPlayers = 2 });

    public void JoinRandomRoom() => PhotonNetwork.JoinRandomRoom();

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnLeftRoom()
    {
        playerDataText.text = "";
        if (!PhotonNetwork.IsMasterClient)
        {
            playerImg.GetComponent<Image>().sprite = null;
        }
    }

    public override void OnJoinedRoom()
    {
        roomUi.SetActive(true);
        lobbyUi.SetActive(false);
        playerDataText.text = "2���� �÷��̾ �ʿ��մϴ�";
        if (PhotonNetwork.IsMasterClient)
        {
            //�����̸� ���ӽ���
            gameStart.SetActive(true);
        }
        else
        {
            //�÷��̾�� �����غ�
            gameReady.SetActive(true);
        }
        RoomRenewal();
    }

    [PunRPC]
    public void RoomRefresh()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //�����̸� ���ӽ���
            gameStart.SetActive(true);
        }
        else
        {
            //�÷��̾�� �����غ�
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
        // RoomRenewal();
        // OnJoinedRoom();
        PV.RPC("RoomRefresh", RpcTarget.All);
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

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            playerImg.GetComponent<Image>().sprite = null;
            roomPlayerText.text = "";
        }

        roomTitleText.text = PhotonNetwork.CurrentRoom.PublishUserId + PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "�� / " + PhotonNetwork.CurrentRoom.MaxPlayers + "�ִ�";
    }

    // public void ClickRoomResetButton()
    // {
    //     RoomRenewal();
    // }

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
        if (playerImg.GetComponent<Image>().sprite == null)
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
            PhotonNetwork.CurrentRoom.IsVisible = false;
            Debug.Log("���ӽ����̹��� �̸� : " + playerImg.name);
            PhotonNetwork.LoadLevel("Game");
        }
    }
    //��

    //��˻� ����
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    //�г��� �����ϰ� �κ�����
    public void ConnectLobby()
    {
        if (nickNameInput.text != null && nickNameInput.text != string.Empty && nickNameInput.text.Length > 5)
        {
            PhotonNetwork.LocalPlayer.NickName = nickNameInput.text;
            nickNameText.text = PhotonNetwork.LocalPlayer.NickName;
            SetUserData(nickNameText.text);
            nickNameUi.SetActive(false);
            lobbyUi.SetActive(true);
            roomUi.SetActive(false);
            playerDataText.text = "ȯ���մϴ�";
            // myList.Clear();
        }
        else
        {
            playerDataText.text = "5���� �̻� �г����� �Է����ּ���";
        }
    }

    public void Connect() => PhotonNetwork.ConnectUsingSettings();
    public override void OnConnectedToMaster() => PhotonNetwork.JoinLobby();
    public override void OnJoinedLobby()
    {
        try
        {
            nickNameText.text = PhotonNetwork.LocalPlayer.NickName;
            nickNameUi.SetActive(false);
            lobbyUi.SetActive(true);
            roomUi.SetActive(false);
            myList.Clear();
        }
        catch (Exception ex)
        {
            Debug.Log("�г��Ӽ��� ���� : " + ex.Message);
        }
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        nickNameUi.SetActive(true);
        lobbyUi.SetActive(false);
        roomUi.SetActive(false);
    }

    //�κ񿡼� �ڷΰ��� > �г����Է�â
    public void ClickLobbyBackButton()
    {
        lobbyUi.SetActive(false);
        nickNameUi.SetActive(true);
    }

    public void ClickAskButton()
    {
        SceneManager.LoadScene("Ad");
    }

    public void ClickBackButton()
    {
        //win ����
        SetWin();

        //PlayerData ����
        SetUserData(playerData);

        Application.Quit();
    }
}