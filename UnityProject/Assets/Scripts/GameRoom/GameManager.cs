using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviourPunCallbacks
{

    public TMPro.TextMeshProUGUI playerListText;
    //public Material demoCubeMaterial;
    public string messages;
    public InputField inputField;
    public TMPro.TextMeshPro textToThrow;
    public GameObject buttonQuit;
    public GameObject buttonSend;
    public GameObject textBox;

    private Player localplayer;
    public Animator animator;
    private bool isReceiver = false;
    private bool isSender = false;


    private int i = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        string defaultName = string.Empty;
        InputField inputField = this.GetComponent<InputField>();
        localplayer = PhotonNetwork.LocalPlayer;
        Debug.Log("Local Player:" + localplayer.ToString());
       
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player otherPlayer)
    {
        Debug.Log("Player entered room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Player left room");
    }

    public void BroadcastCubeColour()
    {
        
        if(inputField.text != null)
        {
            messages = inputField.text;
            
        }
        
        this.photonView.RPC("ChatMessage", RpcTarget.AllViaServer,messages);
        

    }

    private string GeneratePlayerListString()
    {
        var outputString = "Players\n";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            outputString += player.NickName + "\n";
        }

        // Remove trailing slash
        outputString = outputString.Remove(outputString.Length - 1);

        //Debug.Log(outputString);
        return outputString;
    }
    
    
    // Update is called once per frame
    void Update()
    {

    }

    // Fixed update the player list every 3 seconds
    private void FixedUpdate()
    {
        if (i % 150 == 0)
        {
            playerListText.text = GeneratePlayerListString();
        }
        i += 1;
    }

    private void LoadAR()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Not master client");
            
        }
        
        PhotonNetwork.LoadLevel("GameRoom");
    }

    

    [PunRPC]
    void ChatMessage(string message, PhotonMessageInfo meta)
    {
        Debug.Log("Message from " + meta.Sender + ": " + message);
        if (meta.Sender.UserId != localplayer.UserId)
        {
            isReceiver = true;
            StartCoroutine(ReceiveTheMessage(message));          
        }
        else
        {
            isSender = true;
            StartCoroutine(ThrowMessage(messages));
        }
    }

    IEnumerator ThrowMessage(string toSend)
    {
        if (isSender == true)
        {
            buttonQuit.SetActive(false);
            buttonSend.SetActive(false);
            textBox.SetActive(false);
            animator.SetBool("throw", true);
            textToThrow.text = toSend;
            yield return new WaitForSeconds(3f);
            buttonQuit.SetActive(true);
            buttonSend.SetActive(true);
            textBox.SetActive(true);
            textToThrow.text = "";
            animator.SetBool("throw", false);
        }
        isSender = false;
        


    }
    IEnumerator ReceiveTheMessage(string received)
    {
        if (isReceiver == true)
        {
            buttonQuit.SetActive(false);
            buttonSend.SetActive(false);
            textBox.SetActive(false);
            animator.SetBool("receive", true);
            textToThrow.text = received;
            yield return new WaitForSeconds(3f);
            buttonQuit.SetActive(true);
            buttonSend.SetActive(true);
            textBox.SetActive(true);
            textToThrow.text = "";
            animator.SetBool("receive", false);
        }
        isReceiver = false;
    }

    public void UseAssets()
    {
        SceneManager.LoadScene("GameRoom");
        Debug.Log("Joined Gameroom");
    }
}
