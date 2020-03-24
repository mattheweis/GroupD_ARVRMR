using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{

    //public TMPro.TextMeshProUGUI DebugText;
    public GameObject controlPanel;
    //public GameObject progressLabel;
    private bool chooseMessage;
    private bool chooseSticker;
    public Animator animator;

    bool isConnecting;


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
    }

    void Start()
    {
        //progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        
        //Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Connect()
    {
        //progressLabel.SetActive(true);
        controlPanel.SetActive(false);
        animator.SetBool("StartAnimation",true);
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
            //animator.SetBool("StartAnimation", false);
            //PhotonNetwork.LoadLevel("ChoosingScene");
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            isConnecting = PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "0.0.0";
            //animator.SetBool("StartAnimation", false);
        }
        
    }

    public void ARDebugMode()
    {
        PhotonNetwork.LoadLevel("ARDemoScene");
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            Debug.Log("Connected to Master");
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No random room is available.");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
        
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Successfully joined a room. Attempting to load new scene.");
        Debug.Log("Attempting to load new scene.");
        PhotonNetwork.LoadLevel("ChoosingScene");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //progressLabel.SetActive(false);
        controlPanel.SetActive(true);
        Debug.Log("Disconnected.");
    }
    


}
