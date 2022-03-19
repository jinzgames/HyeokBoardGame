using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static Dictionary<string, string> PlayerData = new Dictionary<string, string>();

    public static string playerid = null;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().AddOauthScope("profile").RequestServerAuthCode(false).Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        OnSignInButtonClicked();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnSignInButtonClicked()
    {
        Social.localUser.Authenticate((bool success) =>
        {
            if (success)
            {
                // PlayFabLogin();
                Debug.Log("Google Signed In");
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                Debug.Log("Server Auth Code: " + serverAuthCode);

                PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true
                }, (result) =>
                {
                    SceneManager.LoadScene("Lobby");
                    Debug.Log("Signed In as " + result.PlayFabId);
                    playerid = result.PlayFabId;
                }, OnPlayFabError);
            }
            else
            {
                Debug.Log("Google Failed to Authorize your login");
            }
        });
    }
    private void OnPlayFabError(PlayFabError obj)
    {
        Debug.Log("Playfab error");
    }

    //Playfab로그인
    // public void PlayFabLogin()
    // {
    //     var request = new LoginWithEmailAddressRequest { Email = Social.localUser.id + "@rand.com", Password = Social.localUser.id };
    //     PlayFabClientAPI.LoginWithEmailAddress(request, (result) => logtext.text = "플레이팹 로그인 성공\n" + Social.localUser.userName, (error) => PlayFabRegister());
    // }

    //Playfab회원가입
    // public void PlayFabRegister()
    // {
    //     var request = new RegisterPlayFabUserRequest { Email = Social.localUser.id + "@rand.com", Password = Social.localUser.id, Username = Social.localUser.userName };
    //     PlayFabClientAPI.RegisterPlayFabUser(request, (result) => { logtext.text = "플레이팹 회원가입 성공"; PlayFabLogin(); }, (error) => logtext.text = "플레이팹 회원가입 실패");
    // }

    //Playfab
    public void ClientGetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result =>
            {
                if (result.Data == null || !result.Data.ContainsKey("MonsterName")) Debug.Log("No MonsterName");
                else Debug.Log("MonsterName: " + result.Data["MonsterName"]);
            },
            error =>
            {
                Debug.Log("Got error getting titleData:");
                Debug.Log(error.GenerateErrorReport());
            }
        );
    }
}