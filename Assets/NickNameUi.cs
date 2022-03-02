using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NickNameUi : MonoBehaviour
{
    public void ClickBackButton()
    {
        Application.Quit();
    }

    public void ClickConnectButton()
    {
        SceneManager.LoadScene("Lobby");
    }
}