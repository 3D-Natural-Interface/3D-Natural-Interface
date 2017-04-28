using UnityEngine;
using System.Collections;

public class PhotonInit : MonoBehaviour
{
    public string version = "v1.0";
    public SystemInit systemInit;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(version);
    }
    void Enable()
    {
        PhotonNetwork.ConnectUsingSettings(version);
    }
    void OnJoinedLobby()
    {
        Debug.Log("Entered lobby!");
        PhotonNetwork.JoinRandomRoom();
    }

    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("No rooms !");
        PhotonNetwork.CreateRoom("MyRoom");
    }

    void OnJoinedRoom()
    {
        Debug.Log("Entered room!");
        CreatePlayer();
    }

    void CreatePlayer()
    {
        PhotonNetwork.Instantiate("User", new Vector3(0, 0, 0), Quaternion.identity, 0);
        systemInit.SendMessage("setConnection", true);
    }
    /*void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }*/
}
