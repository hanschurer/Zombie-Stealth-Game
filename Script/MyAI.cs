using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyAI : MonoBehaviour
{
    // Zoombie FSM state which contains five states
    public enum ZoombieState
    {
        Patrol,
        Walk,
        Chase,
        Attack,
        Dead,
    }

    public ZoombieState currentState;

    Animator anim;
    private UnityEngine.AI.NavMeshAgent agent;

    public float Speed = 0.9f;             //zombie speed setting This value takes into account the relative values of the zombies and the players very well. The zombies will neither easily catch up with the player nor be too difficult. This gives a sense of tug between the zombies and the player to help the player think about avoidable obstacles in the process of escaping.
    public float HP = 10;                  //The HP value of the zombie is set at 10 because the player does 3 damage per attack.
                                           //This is to ensure that the zombie is not killed too easily and that killing the zombie is not too time consuming.
                                           //So three attacks to kill a zombie would be a good value to improve game play.
    //zombie sensor 
    public float HearingRange = 10f;       //Considering the size of the map and the perception of the zombie, this value is set to 10. 


                                           //This is to allow the zombie to have a wide perception range as well as to take into account the speed of the player to escape from this range
    public float MaxAngle=45f;             //The zombies in the game have a 90 degree angle view. This is to try to mimic the feeling of nature by simulating the human eye. In FPS games, the game character view is usually ninety degrees If the view is too wide, it will looks like moving very fast, too narrow, and this will makes the character moving very slowly. so 90 degrees seems to be the "sweet spot"
    public float MaxRadius=10f;            //This is the length of the zombie ray's distance which is the length of the zombie's line of sight. it will be Reasonable to set as the same as hearing range
    public static bool PlayerInSight = false;

    // use to store player transform
    private Transform nearbyPlayer;
    private Renderer[] rim;
    public static GameObject player;

    //patrol 
    public Transform[] wayPoints;
    public float stopTime = 3f;            //This is the time of the zombie guard stays in for each waypoint it reaches. Three seconds is exactly how long this left-right look animation takes, which will allow the zombie to complete a loop from left to right to look around effectively and avoid causing too much weirdness
    public float stoptimer = 0;            //this is used to calculate how long zombie have been stoped
    private int index = 0;


    void FSMController()
    {
        //Call the corresponding state handling function according to the current state of the zombie
        switch (currentState)
        {
            case ZoombieState.Walk:
                Walkstate();
                break;
            case ZoombieState.Chase:
                Chasestate();
                break;
            case ZoombieState.Attack:
                Attackstate();
                break;
            case ZoombieState.Dead:
                Deadstate();
                break;
            case ZoombieState.Patrol:
                Patrolling();
                break;
        }
    }



    void Start()
    {
        //get all the  component from the Zombie
        anim = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        //put each skin renderer of body into rim and get the number of them
        rim = GetComponentsInChildren<SkinnedMeshRenderer>(); 
        //Have the zombies start patrolling from the first point
        currentState = ZoombieState.Patrol;
        //zombie start patroling from the first waypoints
        agent.destination = wayPoints[0].position;

    }


    void Update()
    {
        // Zombies persistently search for attached units and take damage
        FindPlayer();
        takeDamage();

        //Used to check if the player is in range of the zombie's senses
        Debug.Log(nearbyPlayer);
    }
    private void FixedUpdate()
    {
        FSMController();
    }
    void Walkstate()
    {
        //Set the zombie walking speed to 0.9f. When the speed is <1f the zombie is animated as a walk in the animation controller.
        anim.SetFloat("Speed", Speed);
        //If player transform is in the nearbyplayer, the zombie will chase the player
        if (nearbyPlayer != null)
        {
            currentState = ZoombieState.Chase;
            agent.ResetPath();
            //Set the zombie walking speed to 0.9f. When the speed is <1f the zombie is animated as a walk in the animation controller.
            if (Vector3.Distance(nearbyPlayer.position, transform.position) < 3f)
            {
                currentState = ZoombieState.Attack;
                agent.ResetPath();
                anim.SetBool("isAttack", true);
                return;
            }
            return;
        }

        //The code below for the last area where zombies walk around randomly
        //// choose a random direction to move 
        //Vector3 randomRange = new Vector3((Random.value - 0.5f) * 2 * 15.0f, 0, (Random.value - 0.5f) * 2 * 15.0f);
        //    Vector3 nextDestination = transform.position + randomRange;
        //    agent.destination = nextDestination;
        //    Debug.Log("Zombie now is walking around");
        //Skin rendering of zombies changed to normal
        Normal();

    }

    void Chasestate()
    {
        if (nearbyPlayer != null)
        {
            //In chase situations, the zombies should be facing the player, this is to add a better gameplay experience, after all no zombie will chase with its back to the player
            transform.LookAt(nearbyPlayer);

            //Set the zombie speed to 2f, this value makes the zombies change the animation to chase to give player a Sense of crisis
            anim.SetFloat("Speed", 2f);
            //Zombies chasing in the direction of the player
            agent.SetDestination(nearbyPlayer.position);
            
            //If the player is in range of the attack then enter the attack state
            if (Vector3.Distance(nearbyPlayer.position, transform.position) <= 3f) 
            {
                currentState = ZoombieState.Attack;
                agent.ResetPath();
                return;
            }

        }
        else
        {
            agent.ResetPath();
            currentState = ZoombieState.Patrol;  
            anim.SetBool("isAttack", false);
            return;

        }

        //Debug.Log("Zombie now is chasing around");
       

        Berserk();
    }

    void Attackstate() {

        //If the player vision is lost in the attack state, the zombie will automatically enter patrol mode
        if (nearbyPlayer == null)
        {
            currentState = ZoombieState.Patrol;
            agent.ResetPath();
            anim.SetBool("isAttack", false);
            return;
        }

        if (Vector3.Distance(nearbyPlayer.position, transform.position) > 3f) //If the player is in view but cannot reach the attack distance, the zombie enters chase mode
        {
            currentState = ZoombieState.Chase;
            agent.ResetPath();
            anim.SetBool("isAttack", false);
            return;
        }



        if (nearbyPlayer != null)
        {
            //Calculate the angle between the front of the zombie and the player, only the player in front of the zombie can attack
            Vector3 direction = nearbyPlayer.position - transform.position;
            float degree = Vector3.Angle(direction, transform.forward);

            //If the player is facing a zombie and does not die, the zombie continues to attack the player
            if (degree < 90f / 2 && degree > -90f / 2 && !PlayerAI.isDead)
            {
                anim.SetBool("isAttack", true);
                //The player's blood deduction takes into account the zombie attack time. The attack damage is set to 0.05 enabling the player to take four zombie attack animations. This is deliberately set to minimise the number of attacks and to avoid the frustration of being killed in one hit.
                PlayerAI.HP -= 0.05f;
                
            }
            else
            {
                //If the player is not in front of the zombie, the zombie needs to turn before it can attack
                anim.SetBool("isAttack", false);
                transform.LookAt(nearbyPlayer);
            }
        }

        Berserk();
    }

    void Deadstate()
    {
        //Used to handle zombie death states and destroy game objects after a certain amount of time
        anim.SetBool("isDead", true);
        //The wait time is set to 2f because the zombie death animation is 2f, so that it doesn't end too abruptly.
        Destroy(gameObject, 2f);
        Normal();
    }

    void Berserk()
    {
        //Iterate through each part of the zombie's body to change the shader to red
        foreach (Renderer r in rim)
        {
            r.material.SetFloat("_RimBool", 1.0f);
        }
    }
    public void Normal()
    {
        //Iterate through each part of the zombie's body to change the shader to noraml
        foreach (Renderer r in rim)
        {
            r.material.SetFloat("_RimBool", 0.0f);
        }
    }
    void takeDamage()
    {
        if (player != null)
        {
            if (HP > 0) //Determine if a zombie is dead by HP
            {
                if (Vector3.Distance(player.transform.position, transform.position) < 3f && PlayerAI.anim.GetBool("punch"))   //If the player is close to the zombie && If the player presses attack
                {
                    //Minus HP value
                    HP -= 3;

                    //We can't use nearbyplayer here because the player is not in the visual perception range of the zombie,however we can use the lookat function to make the player re-enter the nearbyplayer object
                    transform.LookAt(player.transform.position);

                    //Zombies enter chase mode, in chase mode will be based on the distance to decide to attack or chase
                    currentState = ZoombieState.Chase;
                }
            }
            else
            {
                currentState = ZoombieState.Dead; //When the life value of the zombie is less than 0, the zombie is judged to be dead
            }
        }




    }

    public void FindPlayer() //Zombie Sense Function
    {
        nearbyPlayer = null;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (player != null)
        {
            //Collider[] surround = new Collider[20];  //collider buffer size
            //int count = Physics.OverlapSphereNonAlloc(transform.position, MaxRadius, surround); //  Returns the amount of colliders stored into the results buffer

            //for (int i = 0; i < count+1; i++)
            //  {
            //if (surround[i] != null) 
            //{

            if (distance < HearingRange)   //if the player is in the range
            {
                
                Vector3 directionBetween = (player.transform.position - transform.position).normalized;
                directionBetween.y *= 0;  //When defining the direction  between the zombie and the player, we only want the values x and z to be involved, so we zero out the value of y

                float angle = Vector3.Angle(transform.forward + Vector3.up * 0.8f, directionBetween);  // defining zombie sight angle

                if (angle <= MaxAngle)  //if player is in the sight angle
                {
                    Ray ray = new Ray(transform.position + Vector3.up * 0.8f, player.transform.position - transform.position); //Here the ray added a vector up 0.8 value is to avoid the zombie's ray from the ground  hit the floor. so it will launch from the zombie's eye area to make it more sensible
                    RaycastHit hit;  

                    if (Physics.Raycast(ray, out hit, MaxRadius))  ////Fire a ray from the zombie's location, get information back from a raycast into hit
                    {
                        if (hit.transform == player.transform)  //If ray does hit the player without hitting any obstcles
                        {
                            PlayerInSight = true;        //zombie will have the player transform
                            nearbyPlayer = player.transform;
                        }
                        else
                        {
                            PlayerInSight = false;
                            nearbyPlayer = null;
                        }
                    }
                }
            }
            // }
            // }
        }

    }

    void Patrolling()
    {
        FindPlayer();

        if (!PlayerInSight) //if the zombie can't see the player then patrolling
        {
            anim.SetFloat("Speed", 1.1f);
            if (agent.remainingDistance < 0.5f)
            {
                anim.SetFloat("Speed", 0.5f);

                stoptimer += Time.deltaTime;   //calculate the stop time

                if (stoptimer > stopTime) //If the stop time is reached
                {
                    index++;
                    Debug.Log(index);
                    index %= 6;   //Get the subscript of the point array using the remainder method cuz we have 6 waypoints so we take the remainder of the six
                    agent.destination = wayPoints[index].position;  //zombie walk to the next waypoint
                    stoptimer = 0;   
                }
            }
        }
        else
        {
            currentState = ZoombieState.Chase;  //if can see the player, chase the player
        }
        Normal();
    }
}
