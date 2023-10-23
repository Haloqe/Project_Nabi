using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    public GameObject leftEnd;
    public GameObject rightEnd;
    //speed at which enemy will move
    [SerializeField] float moveSpeed = 1f;
    //rigid body component of enemy
    private Rigidbody2D enemyRigidBody;
    private Transform currentPoint;
    void Start()
    {
        enemyRigidBody = GetComponent<Rigidbody2D>();
        currentPoint = rightEnd.transform;
    }

    void Update()
    {
        //give the direction that enemy will move towards
        Vector2 point = currentPoint.position - transform.position;
        
        if(currentPoint == rightEnd.transform)
        {
            enemyRigidBody.velocity = new Vector2(moveSpeed, 0f);
        }
        else
        {
            enemyRigidBody.velocity = new Vector2(-moveSpeed, 0f);
        }
        
        //if our enemy has reached the current point and if the current point is right end,
        //we set the current point to leftEnd.
        if(Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == rightEnd.transform)
        {
            FlipEnemyFacing();
            currentPoint = leftEnd.transform;
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f && currentPoint == leftEnd.transform)
        {
            FlipEnemyFacing();
            currentPoint = rightEnd.transform;
        }

    }
    private void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(-(Mathf.Sign(enemyRigidBody.velocity.x)), 1f);
    }

    //just to easily visualise the distance and endpoints of the enemy's movement
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(leftEnd.transform.position, 0.5f);
        Gizmos.DrawWireSphere(rightEnd.transform.position, 0.5f);
        Gizmos.DrawLine(leftEnd.transform.position, rightEnd.transform.position);
    }
}
