using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

public class ColourScript : MonoBehaviour
{

    public Material demoCubeMaterial;

    // Start is called before the first frame update
    void Start()
    {
 
    }

    [PunRPC]
    private void SetCubeColour()
    {
        int colour = Random.Range(1, 5);
        if (colour == 1)
        {
            demoCubeMaterial.SetColor("_Color", Color.red);
        }
        else if (colour == 2)
        {
            demoCubeMaterial.SetColor("_Color", Color.green);
        }
        else if (colour == 3)
        {
            demoCubeMaterial.SetColor("_Color", Color.blue);
        }
        else if (colour == 4)
        {
            demoCubeMaterial.SetColor("_Color", Color.black);
        }

    }

    private void BroadcastCubeColour()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetCubeColour", RpcTarget.All);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
