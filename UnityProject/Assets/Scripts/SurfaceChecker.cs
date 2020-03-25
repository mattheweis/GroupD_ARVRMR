using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class SurfaceChecker : MonoBehaviour
{

    public ARRaycastManager RaycastManager;
    public bool canPlace;
    private bool hasPlaced;
    private Pose placementPose;

    public GameObject placementIndicator;
    public GameObject heart;
    public GameObject helloTile;
    public GameObject makeFriends;

    private string objectChecker = "None";


    private GameObject spawnedObject;

    private bool rotateObject = false;

    //private int touchCount;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlacementPose();
        UpdateTargetIndicator();

        //if (Input.touchCount > 0)
        //{
        //    startedTouching = Input.GetTouch(0).phase == TouchPhase.Began;
        //}
        //else
        //{
        //    startedTouching = false;
        //}

    }

    private void UpdateTargetIndicator()
    {
        if (canPlace)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose()
    {
        Vector3 screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        RaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        canPlace = hits.Count > 0;
        if (canPlace)
        {
            placementPose = hits[0].pose;
            Vector3 cameraForward = Camera.current.transform.forward;
            Vector3 cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    public void spawnHeartObject()
    {
        if (hasPlaced)
        {
            Destroy(spawnedObject);
            hasPlaced = false;
        } else
        {
            objectChecker = "heart";
            hasPlaced = true;
            spawnedObject = Instantiate(heart, placementPose.position, placementPose.rotation);
        }

        if (objectChecker != "heart")
        {
            objectChecker = "heart";
            hasPlaced = true;
            spawnedObject = Instantiate(heart, placementPose.position, placementPose.rotation);
        }

        if (rotateObject)
        {
            spawnedObject.transform.Rotate(new Vector3(0, 3, 0), Space.Self);
        }
    }

    public void spawnHelloObject()
    {
        if (hasPlaced)
        {
            Destroy(spawnedObject);
            hasPlaced = false;
        }
        else
        {
            objectChecker = "hello";
            hasPlaced = true;
            spawnedObject = Instantiate(helloTile, placementPose.position, placementPose.rotation);
        }

        if (objectChecker != "hello")
        {
            objectChecker = "hello";
            hasPlaced = true;
            spawnedObject = Instantiate(helloTile, placementPose.position, placementPose.rotation);
        }

        if (rotateObject)
        {
            spawnedObject.transform.Rotate(new Vector3(0, 3, 0), Space.Self);
        }
    }



    public void spawnFriendObject()
    {
        if (hasPlaced)
        {
            Destroy(spawnedObject);
            hasPlaced = false;
        }
        else
        {
            objectChecker = "friend";
            hasPlaced = true;
            spawnedObject = Instantiate(makeFriends, placementPose.position, placementPose.rotation);
        }

        if (objectChecker != "friend")
        {
            objectChecker = "friend";
            hasPlaced = true;
            spawnedObject = Instantiate(makeFriends, placementPose.position, placementPose.rotation);
        }

        if (rotateObject)
        {
            spawnedObject.transform.Rotate(new Vector3(0, 3, 0), Space.Self);
        }
    }

}


//if (!hasPlaced && canPlace && objectChecker == "None" )
//{
//    spawnedObject = Instantiate(heart, placementPose.position, placementPose.rotation);
//   hasPlaced = true;
//   
//}
//if (rotateObject)
//{
//    spawnedObject.transform.Rotate(new Vector3(0, 3, 0), Space.Self);
//}