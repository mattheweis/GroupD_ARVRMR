using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;

using Photon.Pun;
using Photon.Realtime;

public class ARScriptManager : MonoBehaviourPunCallbacks
{

    public ARSessionOrigin sessionOrigin;
    
    public ARRaycastManager raycastManager;
    public ARReferencePointManager refPointManager;

    public GameObject debugPrefab;

    public GameObject sticker0;
    public GameObject sticker1;
    
    /* Detects if the user has placed the anchor yet */
    private bool canSet = false;
    private bool anchorSet = false;

    /* Position and object for the anchor placement indicator */
    private Pose placementPose;
    public GameObject placementIndicator;

    /* Touch detection */
    private bool startedTouching;

    public AudioSource audioSource;

    public AudioClip placementClip;
    public AudioClip successClip;
    public AudioClip failureClip;

    /* Debugging */
    public TMPro.TextMeshProUGUI debugText;
    public TMPro.TextMeshProUGUI photonDebugText;
    public TMPro.TextMeshProUGUI coordinateDebugText;
    public TMPro.TextMeshProUGUI currentCoordinateDebugText;


    private GameObject commonRefPointObject;

    private Vector3 originPoint;
    private Quaternion originRotation;

    void Start()
    {
        //audioSource.PlayOneShot(successClip);
        debugText.text = "Waiting";
        photonDebugText.text = "Waiting";
        originPoint = transform.position;
        originRotation = transform.rotation;
    }

    [PunRPC]
    void PlaceObject(float poseX, float poseY, float poseZ)
    {
        photonDebugText.text = "PUN RPC was called";
        Pose objectPose = PackPose(new List<float> { poseX, poseY, poseZ });
        coordinateDebugText.text = coordinateDebugText.text + "\nto -> " + objectPose.position.x.ToString() + ", " + objectPose.position.y.ToString() + ", " + objectPose.position.z.ToString();
        int objectId = 0;
        if (anchorSet)
        {
            if (objectId == 0)
            {
                Instantiate(sticker0, objectPose.position, objectPose.rotation);
            }
            else if (objectId == 1)
            {
                Instantiate(sticker1, objectPose.position, objectPose.rotation);
            }
            audioSource.PlayOneShot(placementClip);
        }
        else
        {
            audioSource.PlayOneShot(failureClip);
        }
    }

    Pose PackPose(List<float> fl)
    {
        Pose newPose;
        newPose.position.x = fl[0];
        newPose.position.y = fl[1];
        newPose.position.z = fl[2];
        newPose.rotation = originRotation;
        return newPose;
    }

    List<float> UnpackPose(Pose pose)
    {
        return new List<float> { pose.position.x, pose.position.y, pose.position.z };
    }

    Pose DetermineWorldPose(Pose localPose)
    {
        Pose newPose;
        newPose.position = localPose.InverseTransformPosition(originPoint);
        newPose.rotation = originRotation;
        return newPose;
    }

    Pose DetermineLocalPose(Pose worldPose, GameObject referencePoint)
    {
        Pose newPose;
        newPose.position = worldPose.InverseTransformPosition(referencePoint.transform.position);
        newPose.rotation = originRotation;
        return newPose;
    }
    
    void Update()
    {
        UpdateAnchorTarget();
        if (!anchorSet)
        { 
            if (UserBeganTouching() && canSet && !anchorSet)
            {
                sessionOrigin.transform.position = placementPose.position;
                sessionOrigin.transform.rotation = placementPose.rotation;
                ARReferencePoint commonRefPoint = refPointManager.AddReferencePoint(placementPose);
                commonRefPointObject = GameObject.FindGameObjectsWithTag("ARRefPoint")[0];
                if (commonRefPoint == null)
                {
                    Debug.Log("Failed to add reference point");
                    audioSource.PlayOneShot(failureClip);
                } else
                {
                    Debug.Log("Successfully added reference point");
                    anchorSet = true;
                    placementIndicator.SetActive(false);
                    audioSource.PlayOneShot(successClip);
                    // Instantiate(debugPrefab, placementPose.position, placementPose.rotation);
                }

            }
        
        
        }
        else
        {
            if (UserBeganTouching() && canSet)
            {
                photonDebugText.text = "Called placement function.";
                PhotonView pv = PhotonView.Get(this);
                coordinateDebugText.text = placementPose.position.x.ToString() + ", " + placementPose.position.y.ToString() + ", " + placementPose.position.z.ToString();
                //pv.RPC("PlaceObject", RpcTarget.AllViaServer, DetermineLocalPose(placementPose, commonRefPointObject));
                List<float> toSend = UnpackPose(placementPose);
                //Instantiate(sticker0, placementPose.position, placementPose.rotation);
                pv.RPC("PlaceObject", RpcTarget.AllViaServer, toSend[0], toSend[1], toSend[2]);
            }
        }

        
    }

    private bool UserBeganTouching()
    {
        return Input.GetTouch(0).phase == TouchPhase.Began;
    }
    
    /* Update the anchor target indicator position.
     * If the target is invalid, the indicator will not appear. */
    private void UpdateAnchorTarget()
    {

        bool adjustVisibility = true;

        /* Raycast from the user camera onto the detected plane.
         * If there are no hits, the placement is invalid, and so canSet 
         * will be set accordingly. */
        Vector3 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        canSet = hits.Count > 0;

        /* Determine placement indicator position based on the raycast information */
        if (canSet)
        {
            debugText.text = "Can set!";
            placementPose = hits[0].pose;
            Vector3 cameraForward = Camera.current.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
        else
        {
            debugText.text = "Cannot set!";
        }

        currentCoordinateDebugText.text = placementPose.position.x.ToString() + ", " + placementPose.position.y.ToString() + ", " + placementPose.position.z.ToString();

        /* Do not make visible if we already placed the anchor,
         * but still want to raycast */
        if (adjustVisibility)
        {
            /* Set target indicator visibility based on above code */
            if (canSet)
            {
                placementIndicator.SetActive(true);
                placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
            }
            else
            {
                placementIndicator.SetActive(false);
            }
        }

        
        
    }

    /*
    private void OnGUI()
    {
        GUI.Label(new Rect(60, 20, 1000, 1000), "Set anchor!");
        GUI.skin.label.fontSize = 50;
    }
    */

}

internal class stickerObjects
{
}