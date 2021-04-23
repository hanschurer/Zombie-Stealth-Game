using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerAI : MonoBehaviour
{
    // /The mouse is rotated to give the player a better view of the surroundings to make the most advantageous judgement
    float minMouseRotateX = -45.0f;
    float maxMouseRotateX = 45.0f; 
    float mouseRotateX;

    public static bool isDead = false;
    public static float HP = 2f; //The player will have a very low life value because I don't want to turn this game into a zombie fighting game
    public float Speed = 6f;   //This value set is to help players escape from the chase of zombies but it will not be so easy, because the main method of the game is by dodging and hiding in avoiding zombies

    public static Canvas cav;
    public static Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        //Make the mouse invisible to create a better gaming experience
        Cursor.visible = false;
       
        anim = GetComponentInChildren<Animator>();
        cav = GameObject.FindObjectOfType<Canvas>();
        cav.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Get the input of h v from the keyboard
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Move(h, v);
        //Get the input of routate h and routate v from the mouse
        float rv = Input.GetAxisRaw("Mouse X");
        float rh = Input.GetAxisRaw("Mouse Y");
        Rotate(rh, rv);
       
        Punch();

        Dead();

        
    }

    // player move function
    void Move(float h, float v)
    {
        //Move according to the keyboard arrow keys
        transform.Translate((Vector3.forward * v + Vector3.right * h) * Speed * Time.deltaTime);
        //If the arrow keys are pressed, the movement animation is played and vice versa
        if (h != 0.0f || v != 0.0f)
        {
            if (anim != null)
                anim.SetBool("isMove", true);
        }
        else
        {
            if (anim != null)
                anim.SetBool("isMove", false);
        }
    }


    // mouse rotate
    void Rotate(float rh, float rv)
    {
        transform.Rotate(0, rv * 2f, 0);
        mouseRotateX -= rh * 2f;
        mouseRotateX = Mathf.Clamp(mouseRotateX, minMouseRotateX, maxMouseRotateX);
        Camera.main.transform.localEulerAngles = new Vector3(mouseRotateX, 0.0f, 0.0f); //Let the camera follow the mouse rotation
    }


    // Player 's attack method to kill zombie
    void Punch()
    {

        if (Input.GetMouseButtonDown(0))
        {
            anim.SetBool("punch", true);
            // to control the player animator controller
        }
        else
        {
            anim.SetBool("punch", false);
        }
    }


    //Player dead state
    void Dead()
    {
        if (HP < 0)  
        { 
            anim.SetBool("Dead",true);  //set animation to dead
            cav.enabled = true;  //pop out the canvas
            Cursor.visible = true; //pop out the cursor
            reBorn();
        }
        
    }


    // Player reBorn
    void reBorn()
    {
        anim.SetBool("Dead", false);
        HP = 2f;
        
    }
}