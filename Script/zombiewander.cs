using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//This code file is a variant of MyAI, except that the zombie patrol function has been replaced with a zombie random wander function. All comments will be reflected in MyAI.cs file.


public class zombiewander : MonoBehaviour
{
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

    public float Speed = 1.1f;

    public float HP = 10;

    //zombie sensor 
    public float HearingRange = 10f;
    public float SightRange = 10f;
    public float SightAngle = 60;
    public float viewlength = 8f;
    public float MaxAngle = 45f;
    public float MaxRadius = 10f;
    public bool PlayerInSight = false;

    AnimationState info;
    private Transform nearbyPlayer;

    private Renderer[] rim;

    public static GameObject player;



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
        }
    }



    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        rim = GetComponentsInChildren<SkinnedMeshRenderer>(); //put each skin renderer of body into rim and get the number of them

        currentState = ZoombieState.Walk;

    }


    void Update()
    {
        FindPlayer();
        takeDamage();
    }
    private void FixedUpdate()
    {
        FSMController();

    }
    void Walkstate()
    {
        anim.SetFloat("Speed", Speed);
           //choose a random direction to move 
            Vector3 randomRange = new Vector3((Random.value - 0.5f) * 2 * 15.0f, 0, (Random.value - 0.5f) * 2 * 15.0f);
            Vector3 nextDestination = transform.position + randomRange;
            agent.destination = nextDestination;
        Normal();

        if (PlayerInSight)
        {
            currentState = ZoombieState.Chase;
            agent.ResetPath();

            if (Vector3.Distance(player.transform.position, transform.position) < 3f)
            {
                currentState = ZoombieState.Attack;
                agent.ResetPath();
                anim.SetBool("isAttack", true);
                return;

            }
            return;
        }

    }

    void Chasestate()
    {

        FindPlayer();
        if (nearbyPlayer == null)
        {
            currentState = ZoombieState.Walk;
            agent.ResetPath();
            anim.SetBool("isAttack", false);
            return;
        }

        if (Vector3.Distance(nearbyPlayer.position, transform.position) <= 1.5f)
        {
            currentState = ZoombieState.Attack;
            agent.ResetPath();
            return;
        }



        //Debug.Log("Zombie now is chasing around");
        transform.LookAt(nearbyPlayer);
        anim.SetFloat("Speed", 2f);
        agent.SetDestination(nearbyPlayer.position);

        Berserk();
    }

    void Attackstate()
    {


        if (nearbyPlayer == null)
        {
            currentState = ZoombieState.Walk;
            agent.ResetPath();
            anim.SetBool("isAttack", false);
            return;
        }

        if (Vector3.Distance(nearbyPlayer.position, transform.position) > 3f)
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
            if (degree < 90f / 2 && degree > -90f / 2 && !PlayerAI.isDead)
            {
                anim.SetBool("isAttack", true);
                PlayerAI.HP -= 0.05f;
                Debug.Log(PlayerAI.HP);
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
        anim.SetBool("isDead", true);
        Destroy(gameObject, 2f);
        Normal();

    }

    void Berserk()
    {
        foreach (Renderer r in rim)
        {
            r.material.SetFloat("_RimBool", 1.0f);
        }
    }
    public void Normal()
    {

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

    public void FindPlayer()
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
                //Debug.Log("player is in the range");
                Vector3 directionBetween = (player.transform.position - transform.position).normalized;
                directionBetween.y *= 0;  // height is not a factor of the angle

                float angle = Vector3.Angle(transform.forward + Vector3.up * 0.5f, directionBetween);  // find angle between zombie direction line and player line

                if (angle <= MaxAngle)  //if player is in the sight angle
                {
                    Ray ray = new Ray(transform.position + Vector3.up * 0.5f, player.transform.position - transform.position);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, MaxRadius))
                    {
                        if (hit.transform == player.transform)  //If ray does hit the player without hitting any obstcles
                        {
                            PlayerInSight = true;
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

}
