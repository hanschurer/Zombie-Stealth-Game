using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gizimo : MonoBehaviour

{
    // Start is called before the first frame update
    public float maxAngle;
    public float maxRadius;
    public GameObject player;
    public 

    private void OnDrawGizmos()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        //Yellow line to depict the perceived range of the zombie
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxRadius);


        Vector3 fov1 = Quaternion.AngleAxis(maxAngle, transform.up) * transform.forward * maxRadius;
        Vector3 fov2 = Quaternion.AngleAxis(-maxAngle, transform.up) * transform.forward * maxRadius;
        //The two blue lines represent the left and right lines of the field of view respectively
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, fov1);
        Gizmos.DrawRay(transform.position, fov2);




        //If the player is within the perception range and there are no obstacles blocking the line of sight, the line will turn green and vice versa.
        if (!MyAI.PlayerInSight)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;

        Gizmos.DrawRay(transform.position, (player.transform.position - transform.position).normalized * maxRadius);

    }

}
