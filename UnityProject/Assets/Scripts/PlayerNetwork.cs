using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour
{

    public static PlayerNetwork Instance;
    public string userID { get; private set; }

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        userID = "timd";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
