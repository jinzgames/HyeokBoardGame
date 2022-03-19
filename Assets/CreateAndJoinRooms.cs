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
    public TextMeshProUGUI logText;
    public static Dictionary<string, string> playerData = new Dictionary<string, string>();
    public static int playerWin = 0;
    public TextMeshProUGUI playerDataText;

    //신규유저 공용데이터 입력
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
                    //Stat (통계)
                    if (v.Key.Equals("Stat_Win"))
                    {
                        li.Add(new StatisticUpdate { StatisticName = v.Key, Value = int.Parse(v.Value) });
                        // NewSetStat(v.Key, int.Parse(v.Value));
                    }
                    //PlayerData_
                    else
                    {
                        //플레이어데이터 타이틀
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

    //플레이어 데이터(타이틀) 수정
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

    //통계저장
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
            Debug.Log("값 저장됨");
        },
        (error) =>
        {
            Debug.Log("값 저장 실패");
            Debug.LogError(error.GenerateErrorReport());
        });
    }

    //win 저장
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
            Debug.Log("값 저장됨");
        },
        (error) =>
        {
            Debug.Log("값 저장 실패");
        });
    }

    public void GetWin()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
        (result) =>
        {
            Debug.Log("불러온 값 : " + result.Statistics[0].StatisticName + " : " + result.Statistics[0].Value + " \n");
            playerWin = result.Statistics[0].Value;

            // foreach (var stat in result.Statistics)
            // {
            //     if (stat.StatisticName == "Stat_Win")
            //     {
            //         Debug.Log("불러온 값 : " + stat.StatisticName + " : " + stat.Value + " \n");
            //         playerWin = stat.Value;
            //     }
            //     else
            //     {
            //         Debug.Log("stat.StatisticName : " + stat.StatisticName);
            //     }
            // }
        },
        (error) => { Debug.Log("불러오기 실패"); });
    }

    //처음 들오면 체크
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
                playerDataText.text = "환영합니다";
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
        playerDataText.text = "환영합니다";

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
            CellButton[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].Name : "빈방";
            CellButton[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = (multiple + i < myList.Count) ? myList[multiple + i].PlayerCount + "/" + myList[multiple + i].MaxPlayers : "";
            //방장 이름 표시하기
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
    //방 리스트

    //방    
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
        playerDataText.text = "2명의 플레이어가 필요합니다";
        if (PhotonNetwork.IsMasterClient)
        {
            //방장이면 게임시작
            gameStart.SetActive(true);
        }
        else
        {
            //플레이어면 게임준비
            gameReady.SetActive(true);
        }
        RoomRenewal();
    }

    [PunRPC]
    public void RoomRefresh()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //방장이면 게임시작
            gameStart.SetActive(true);
        }
        else
        {
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

        roomTitleText.text = PhotonNetwork.CurrentRoom.PublishUserId + PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
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
            Debug.Log("게임시작이미지 이름 : " + playerImg.name);
            PhotonNetwork.LoadLevel("Game");
        }
    }
    //방

    //방검색 참가
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    //닉네임 설정하고 로비접속
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
            playerDataText.text = "환영합니다";
            // myList.Clear();
        }
        else
        {
            playerDataText.text = "5글자 이상 닉네임을 입력해주세요";
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
            Debug.Log("닉네임설정 오류 : " + ex.Message);
        }
    }

    public void Disconnect() => PhotonNetwork.Disconnect();

    public override void OnDisconnected(DisconnectCause cause)
    {
        nickNameUi.SetActive(true);
        lobbyUi.SetActive(false);
        roomUi.SetActive(false);
    }

    //로비에서 뒤로가기 > 닉네임입력창
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
        //win 저장
        SetWin();

        //PlayerData 저장
        SetUserData(playerData);

        Application.Quit();
    }
}