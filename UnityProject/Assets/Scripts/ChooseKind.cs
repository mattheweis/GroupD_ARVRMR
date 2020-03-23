using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
public class ChooseKind : MonoBehaviour
{
    // Start is called before the first frame update
   public void ChooseMessages()
    {
        PhotonNetwork.LoadLevel("GameRoom");
    }
    public void ChooseStickers()
    {
        PhotonNetwork.LoadLevel("ARDemoScene");
        //Debug.Log("Loading the sticker scene(Not implemented yet)");
    }

}
