using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameEnd : MonoBehaviour
{
    public Canvas cav;
    public GameObject player;
    public bool playerwin = false;

    public Text stateText;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        cav = GameObject.FindObjectOfType<Canvas>();
        cav.enabled = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            //if the player trigger the treasture, player win
            playerwin = true;
            stateText= cav.GetComponentInChildren<Text>();
            stateText.text = "YOU WIN";

            //enable the canvas and set the mouse visible
            cav.enabled = true;
            Cursor.visible = true;
            
        }
    }
}
