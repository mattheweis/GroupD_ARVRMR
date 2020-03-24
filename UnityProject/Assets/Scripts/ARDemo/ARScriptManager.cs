using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;

using Photon.Pun;
using Photon.Realtime;

public class ARScriptManager : MonoBehaviourPunCallbacks
{

    public bool debuggingMode = false;
    
    /* AR variables */
    public ARSessionOrigin sessionOrigin;
    public ARRaycastManager raycastManager;
    private GameObject commonRefPointObject;
    public ARReferencePointManager refPointManager;

    /* Networking */
    public PhotonView photonViewObject;

    /* User stickers */
    public GameObject stickerBall;
    public GameObject sticker0;
    public GameObject sticker1;
    
    /* Detects if the user has placed the anchor yet */
    private bool canSet = false;
    private bool anchorSet = false;

    /* Detects if user placed any items yet */
    private bool placedItemStatus = false;
    private GameObject placedItem;

    /* Position and object for the anchor placement indicator */
    private Pose placementPose;
    public GameObject placementIndicator;

    /* Touch detection */
    private bool startedTouching;

    /* Debugging audio */
    public AudioSource audioSource;
    public AudioClip placementClip;
    public AudioClip successClip;
    public AudioClip failureClip;

    /* Speed Adjustment */
    public float ballSpeed = 4f;

    /* Debugging */
    public GameObject debugPrefab;
    public TMPro.TextMeshProUGUI debugText;
    public TMPro.TextMeshProUGUI photonDebugText;
    public TMPro.TextMeshProUGUI coordinateDebugText;
    public TMPro.TextMeshProUGUI currentCoordinateDebugText;
    public TMPro.TextMeshProUGUI masterClientDebugText;

    /* Testing */
    public string gameMode;
    private Vector3 originPoint;
    private Quaternion originRotation;

    /* Reference points */
    private Vector3 otherRefPoint;
    private Pose commonRefPointPose;

    /* Physics calculations */
    private Rigidbody itemRigid;

    /* Instruction images */
    public Image instructionImage;
    public Sprite s_cannotDetectPlane;
    public Sprite s_emptyImage;
    public Sprite s_tapToAnchor;
    public Sprite s_waitingForPuck;

    /* Start currently for debugging purposes */
    void Start()
    {
        if (debuggingMode)
        {
            debugText.text = "Waiting";
            photonDebugText.text = "Waiting";
        }
        originPoint = transform.position;
        originRotation = transform.rotation;
    }

    [PunRPC]
    void UpdateOtherBiasVector(float x, float y, float z)
    {
        otherRefPoint = new Vector3(x, y, z);
    }
    
    /* Place the GameObject in the common space. Requires X, Y, Z coordinates in world space.
     * The object does not show in debugging as referenced, but it is called by PUN RPC. */
    [PunRPC]
    void PlaceObject(float poseX, float poseY, float poseZ, PhotonMessageInfo info)
    {
        
        if (debuggingMode)
        {
            photonDebugText.text = "PUN RPC was called";
        }

        Vector3 objectVect;

        if (info.Sender.UserId != PhotonNetwork.LocalPlayer.UserId)
        {
            objectVect = DetermineMyVector(new Vector3(poseX, poseY, poseZ), otherRefPoint, commonRefPointPose.position);
        }
        else
        {
            objectVect = new Vector3(poseX, poseY, poseZ);
        }

        if (debuggingMode)
        {
            coordinateDebugText.text = coordinateDebugText.text + "\nto -> " + objectVect.x.ToString() + ", " + objectVect.y.ToString() + ", " + objectVect.z.ToString();
        }
        
        int objectId = 0;
        if (anchorSet)
        {
            if (objectId == 0)
            {
                placedItem = Instantiate(stickerBall, objectVect, Quaternion.Euler(0, 0, 0));
                if (PhotonNetwork.IsMasterClient)
                {
                    itemRigid = placedItem.AddComponent<Rigidbody>();
                    itemRigid.useGravity = false;
                }
                placedItemStatus = true;
            }
            else if (objectId == 1)
            {
                placedItem = Instantiate(sticker0, objectVect, Quaternion.Euler(0, 0, 0));
                placedItemStatus = true;
            }
            audioSource.PlayOneShot(placementClip);
        }
        else
        {
            audioSource.PlayOneShot(failureClip);
        }
    }

    [PunRPC]
    void BroadcastObjectPush(float x, float y, float z)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (debuggingMode)
            {
                photonDebugText.text = "Received object push broadcast.";
            }
            // Vector3 newVector = DetermineMyVector(new Vector3(x, y, z), otherRefPoint, commonRefPointPose.position);
            itemRigid.velocity = new Vector3(0, 0, 0);
            itemRigid.AddForce(new Vector3(x, y, z), ForceMode.Impulse);
        }

    }

    /* If the object falls through the world, we want to be able to place a new one.
     * This RPC tells all players to destroy the object placed. */
    [PunRPC]
    void DestroyPlacedObject()
    {
        Destroy(placedItem);
        placedItemStatus = false;
    }

    public void IncreaseSpeed()
    {
        ballSpeed += 0.5f;
    }

    public void DecreaseSpeed()
    {
        ballSpeed -= 0.5f;
    }

    /* Generate a Pose object from its individual components */
    Pose PackPose(List<float> fl)
    {
        Pose newPose;
        newPose.position.x = fl[0];
        newPose.position.y = fl[1];
        newPose.position.z = fl[2];
        newPose.rotation = originRotation;
        return newPose;
    }

    /* Deconstruct a Pose object into its individual components.
     * Required for sending information over the network. */
    List<float> UnpackPose(Pose pose)
    {
        return new List<float> { pose.position.x, pose.position.y, pose.position.z };
    }

    /* WIP: Convert the pose with bias into real world space. */
    Pose DetermineMyPose(Pose biasedPose, Vector3 otherReferenceVector, Vector3 myReferenceVector)
    {
        Vector3 correctionVector = myReferenceVector - otherReferenceVector;
        Pose newPose;
        newPose.position = biasedPose.position + correctionVector;
        newPose.rotation = originRotation;
        return newPose;
    }

    Vector3 DetermineMyVector(Vector3 inputVector, Vector3 otherInputVectorBias, Vector3 myInputVectorBias)
    {
        Vector3 correctionVector = myInputVectorBias - otherInputVectorBias;
        correctionVector = inputVector + correctionVector;
        return correctionVector;
    }

    [PunRPC]
    void UpdateObjectPosition(float x, float y, float z)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (debuggingMode)
            {
                photonDebugText.text = "Received update info.";
            }
            placedItem.transform.position = DetermineMyVector(new Vector3(x, y, z), otherRefPoint, commonRefPointPose.position);
        }
    }


    int i = 0;
    void Update()
    {

        if (i % 4 == 0)
        {
            if (placedItemStatus && PhotonNetwork.IsMasterClient)
            {
                if (debuggingMode)
                {
                    photonDebugText.text = "Sent update broadcast.";
                }
                photonViewObject.RPC("UpdateObjectPosition", RpcTarget.AllViaServer, placedItem.transform.position.x, placedItem.transform.position.y, placedItem.transform.position.z);
            }
        }
        i += 1;

        if (PhotonNetwork.IsMasterClient)
        {
            masterClientDebugText.text = "Master Client";
        }
        else
        {
            masterClientDebugText.text = "Not Master Client";
        }

        if (placedItemStatus)
        {
            UpdateAnchorTarget(false);
        }
        else
        {
            UpdateAnchorTarget(true);
        }
        
        
        if (!anchorSet)
        { 
            if (canSet)
            {
                instructionImage.sprite = s_tapToAnchor;
            }
            else
            {
                instructionImage.sprite = s_cannotDetectPlane;
            }
            
            
            if (UserBeganTouching() && canSet && !anchorSet)
            {
                //sessionOrigin.transform.position = placementPose.position;
                //sessionOrigin.transform.rotation = placementPose.rotation;
                ARReferencePoint commonRefPoint = refPointManager.AddReferencePoint(placementPose);
                commonRefPointPose = placementPose;
                
                if (commonRefPoint == null)
                {
                    audioSource.PlayOneShot(failureClip);
                } 
                else
                {
                    photonViewObject.RPC("UpdateOtherBiasVector", RpcTarget.Others, placementPose.position.x, placementPose.position.y, placementPose.position.z);
                    anchorSet = true;
                    placementIndicator.SetActive(false);
                    audioSource.PlayOneShot(successClip);
                    // Instantiate(debugPrefab, placementPose.position, placementPose.rotation);
                }

            }
        }
        else
        {
            
            if (placedItemStatus)
            {
                instructionImage.sprite = s_emptyImage;
            }
            else
            {
                instructionImage.sprite = s_waitingForPuck;
            }
            
            
            if (UserBeganTouching() && canSet)
            {
                if (!placedItemStatus)
                {
                    /* Place the sticker of choice in the AR world. */
                    if (debuggingMode)
                    {
                        photonDebugText.text = "Called placement function.";
                        coordinateDebugText.text = placementPose.position.x.ToString() + ", " + placementPose.position.y.ToString() + ", " + placementPose.position.z.ToString();
                    }
                    List<float> toSend = UnpackPose(placementPose);
                    photonViewObject.RPC("PlaceObject", RpcTarget.AllViaServer, toSend[0], toSend[1], toSend[2]);
                }
                else
                {
                    Vector3 transformDirection;
                    transformDirection = Camera.current.transform.forward;
                    transformDirection.y = 0;
                    transformDirection = transformDirection.normalized * ballSpeed;

                    if (PhotonNetwork.IsMasterClient)
                    {
                        Rigidbody itemRb = placedItem.GetComponent<Rigidbody>();

                        /* If the angle between the camera direction and movement direction is higher than 80',
                         * stop the rigidBody object before applying a force in the opposite direction. This is
                         * intended to allow for the player to ping back an object without having to slow it
                         * down first. */
                       itemRb.velocity = new Vector3(0, 0, 0);

                        /* As it is not intended for the object to move up or down, the Y component of the camera
                         * direction vector is removed. */
                        itemRb.AddForce(transformDirection, ForceMode.Impulse);
                    }
                    else
                    {
                        /* As the master client is the only client with physics simulation, instructions are sent to it instead */
                        photonViewObject.RPC("BroadcastObjectPush", RpcTarget.AllViaServer, transformDirection.x, transformDirection.y, transformDirection.z);
                    }
                }
            }
        }

        if (placedItemStatus)
        {
            if (Vector3.Distance(placedItem.transform.position, Camera.current.transform.position) > 20)
            {
                photonViewObject.RPC("DestroyPlacedObject", RpcTarget.AllViaServer);
                audioSource.PlayOneShot(failureClip);
            }
        }
    }

    /* Detect if user is touching the screen */
    private bool UserBeganTouching()
    {
        return Input.GetTouch(0).phase == TouchPhase.Began;
    }
    
    /* Update the anchor target indicator position.
     * If the target is invalid, the indicator will not appear. */
    private void UpdateAnchorTarget(bool adjustVisibility)
    {

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
            if (debuggingMode)
            {
                debugText.text = "Can set!";
            }
            placementPose = hits[0].pose;
            Vector3 cameraForward = Camera.current.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
        else
        {
            if (debuggingMode)
            {
                debugText.text = "Cannot set!";
            }
        }

        if (debuggingMode)
        {
            currentCoordinateDebugText.text = placementPose.position.x.ToString() + ", " + placementPose.position.y.ToString() + ", " + placementPose.position.z.ToString();
        }
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
        else
        {
            placementIndicator.SetActive(false);
        }
    }

}