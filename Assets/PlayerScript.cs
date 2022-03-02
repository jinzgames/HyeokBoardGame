using UnityEngine;
using Photon.Pun;
using System;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class PlayerScript : MonoBehaviour, IPunObservable
{
    [Header("Player")]
    public float speed = 5f;
    private float health = 4;
    private float allHealth = 4;
    private Rigidbody characterRigidbody;

    [Header("Map")]
    private string nowPosition = string.Empty;
    private Transform[] blockArr;
    public static bool go = false;
    private int substringIndex;
    private int bombPower = 120;
    public ParticleSystem desertEffect;
    public ParticleSystem bloodEffect;
    public Slider hpBarSlider;
    private Shader transparent;

    [Header("Photon")]
    PhotonView view;
    private Vector3 curPos;

    void Start()
    {
        characterRigidbody = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        blockArr = GameObject.Find("Environment").GetComponentsInChildren<Transform>();
        blockArr = blockArr.OrderBy(go => go.name).ToArray();
        transparent = Shader.Find("Transparent/Diffuse");

        Debug.Log("아이디 : " + view.ViewID);
    }

    void Update()
    {
        if (view.IsMine)
        {
            hpBarSlider.value = health / allHealth;

            //체력이 1보다 아래면 죽음 낙사하면 죽음 > 처음 위치로
            if (health < 1 || transform.position.y < -10)
            {
                transform.position = new Vector3(-8, 0.3f, -7);
                health = allHealth;
            }

            //Move();

            if (go)
            {
                go = false;
                GoBlock();
            }
        }
        else if ((transform.position - curPos).sqrMagnitude >= 100)
        {
            transform.position = curPos;
        }
        else
        {
            transform.position = curPos;
            // transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 5);
        }
    }

    // void Move()
    // {
    //     float inputX = Input.GetAxis("Horizontal");
    //     float inputZ = Input.GetAxis("Vertical");
    //     // -1 ~ 1

    //     float fallSpeed = characterRigidbody.velocity.y; 

    //     Vector3 velocity = new Vector3(inputX, 0, inputZ);
    //     velocity *= speed;
    //     velocity.y = fallSpeed;
    //     characterRigidbody.velocity = velocity;
    // }

    //블럭이동
    void GoBlock()
    {
        int blockNum = int.Parse(nowPosition.Substring(0, 2));

        if (nowPosition != string.Empty)
        {
            Debug.Log("blockNum : " + blockNum);
            if (blockNum > 42)
            {
                view.RPC("Winner", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            }
            else if (nowPosition.Substring(2, 2).Equals("바닥"))
            {
                substringIndex = nowPosition.IndexOf("바닥");
            }
            //transform.position = Vector3.Slerp(transform.position, blockArr[int.Parse(nowPosition.Substring(0, substringIndex))].transform.position + new Vector3(0, 0.3f, 0), 1f);
            transform.position = Vector3.Slerp(transform.position, blockArr[int.Parse(nowPosition.Substring(0, substringIndex)) - 1 + MainUi.diceNum].transform.position + new Vector3(0, 0.3f, 0), 1f);
        }
    }

    [PunRPC]
    void Winner(string nickName)
    {
        MainUi.idTextStatic.text = nickName;
        MainUi.endUiStatic.SetActive(true);
    }

    [PunRPC]
    void BombRemove(GameObject bomb)
    {
        if (bomb != null)
        {
            Destroy(bomb);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hpBarSlider.value);
            stream.SendNext(transform.position);
        }
        else
        {
            hpBarSlider.value = (float)stream.ReceiveNext();
            curPos = (Vector3)stream.ReceiveNext();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.transform.tag)
        {
            case "Bomb":    //폭탄
                BombMove(nowPosition);
                Destroy(GameObject.Find(collision.transform.name));
                nowPosition = collision.transform.parent.name;
                break;
            case "Thorn":   //가시
                Blood(1);
                nowPosition = collision.transform.parent.name;
                break;
            case "End":     //목표지점
                MainUi.idTextStatic.text = CreateAndJoinRooms.nickNameTextStatic.text;
                MainUi.endUiStatic.SetActive(true);
                nowPosition = collision.transform.parent.name;
                break;
            case "Water":   //물
                WaterMove(nowPosition);
                nowPosition = collision.transform.name;
                break;
            case "Spear":   //창 함정
                Blood(1);
                nowPosition = collision.transform.parent.name;
                break;
            case "Trab":    //낙사 함정
                nowPosition = collision.transform.parent.name;
                break;
            case "Ladder":  //사다리
                int y = int.Parse(collision.transform.name.Substring(collision.transform.name.Length - 1, 1));
                transform.position = collision.transform.GetChild(0).position + new Vector3(0, y, 0);
                break;
            case "Desert":  //모래
                ParticleSystem desertIns = Instantiate(desertEffect, (collision.transform.position - new Vector3(0, 0.5f, 0)), Quaternion.Euler(new Vector3(-90, 0, 0)));
                StartCoroutine(DesertShader(collision.gameObject, 3f));
                Destroy(desertIns, 3f);
                nowPosition = collision.transform.name;
                break;
            default:
                nowPosition = collision.transform.name;
                break;
        }
    }
    IEnumerator DesertShader(GameObject desert, float second)
    {
        yield return new WaitForSeconds(second);
        Color c = desert.GetComponent<Renderer>().material.color;
        float i = second;
        while (i > 0)
        {
            i--;
            float f = i / second;
            c.a = f;
            desert.GetComponent<Renderer>().material.color = c;
            yield return new WaitForSeconds(0.02f);
        }
        desert.GetComponent<BoxCollider>().enabled = false;
    }

    void Blood(float second)
    {
        health--;
        ParticleSystem bloodIns = Instantiate(bloodEffect, transform.position, Quaternion.Euler(new Vector3(0, 0, 0)));
        Destroy(bloodIns, second);
    }

    //물 흐름
    void WaterMove(string name)
    {
        float x = blockArr[int.Parse(name.Substring(0, 2)) - 1].transform.position.x - blockArr[int.Parse(name.Substring(0, 2)) - 2].transform.position.x;
        float z = blockArr[int.Parse(name.Substring(0, 2)) - 1].transform.position.z - blockArr[int.Parse(name.Substring(0, 2)) - 2].transform.position.z;
        if (x == 0)
        {
            if (z > 0)
            {
                characterRigidbody.velocity = new Vector3(0, 1.5f, -0.1f);
            }
            else
            {
                characterRigidbody.velocity = new Vector3(0, 1.5f, 0.1f);
            }
        }
        else if (z == 0)
        {
            if (x > 0)
            {
                characterRigidbody.velocity = new Vector3(-0.1f, 1.5f, 0);
            }
            else
            {
                characterRigidbody.velocity = new Vector3(0.1f, 1.5f, 0);
            }
        }
    }

    //폭탄
    void BombMove(string name)
    {
        float x = blockArr[int.Parse(name.Substring(0, 2)) - 1].transform.position.x - blockArr[int.Parse(name.Substring(0, 2)) - 2].transform.position.x;
        float z = blockArr[int.Parse(name.Substring(0, 2)) - 1].transform.position.z - blockArr[int.Parse(name.Substring(0, 2)) - 2].transform.position.z;
        if (x == 0)
        {
            if (z > 0)  //가로
            {
                characterRigidbody.AddForce(new Vector3(0, 1f, -1f) * bombPower);
            }
            else
            {
                characterRigidbody.AddForce(new Vector3(0, 1f, 1f) * bombPower);
            }
        }
        else if (z == 0)    //세로
        {
            if (x > 0)
            {
                characterRigidbody.AddForce(new Vector3(-1f, 1f, 0) * bombPower);
            }
            else
            {
                //아직안씀
                characterRigidbody.AddForce(new Vector3(1f, 1f, 0) * bombPower);
            }
        }
    }
}