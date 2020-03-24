using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class throwchat : MonoBehaviour
{
    // Start is called before the first frame update
    public TMPro.TextMeshProUGUI textToThrow;
    public Animator animator;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator ThrowMessage(string toSend)
    {
        SceneManager.LoadScene("Animation");
        animator.SetBool("throw", true);
        textToThrow.text = toSend;
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("GameRoom");

    }
}
