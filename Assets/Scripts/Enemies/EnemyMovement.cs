using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    //적이 패트롤링 (순찰) 하는 포인트의 왼쪽 끝과 오른쪽 끝 선언, 유니티 에디터에서 드래그해서 넣어주세요
    public GameObject LeftEnd;
    public GameObject RightEnd;

    //speed at which an enemy will move
    [SerializeField] float DefaultMoveSpeed = 1f;
    private float _moveSpeed;
    private bool _isRooted = false;

    //rigid body component of enemy
    private Rigidbody2D _enemyRigidBody;
    private Transform _currentPoint;
    void Start()
    {
         _moveSpeed= DefaultMoveSpeed;
        _enemyRigidBody = GetComponent<Rigidbody2D>();
        _currentPoint = RightEnd.transform;
    }

    void Update()
    {

        if (_isRooted) return;

        //give the direction that enemy will move towards
        Vector2 point = _currentPoint.position - transform.position;
        
        if(_currentPoint == RightEnd.transform)
        {
            _enemyRigidBody.velocity = new Vector2(_moveSpeed, 0f);
        }
        else
        {
            _enemyRigidBody.velocity = new Vector2(-_moveSpeed, 0f);
        }
        
        //if our enemy has reached the current point and if the current point is right end,
        //we set the current point to LeftEnd.
        if(Vector2.Distance(transform.position, _currentPoint.position) < 0.5f && _currentPoint == RightEnd.transform)
        {
            FlipEnemyFacing();
            _currentPoint = LeftEnd.transform;
        }

        if (Vector2.Distance(transform.position, _currentPoint.position) < 0.5f && _currentPoint == LeftEnd.transform)
        {
            FlipEnemyFacing();
            _currentPoint = RightEnd.transform;
        }

    }
    private void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(-(Mathf.Sign(_enemyRigidBody.velocity.x)), 1f);
    }

    public void ResetDefaultMoveSpeed()
    {
        _moveSpeed = DefaultMoveSpeed;
    }

    public void EnableDisableMovement(bool shouldEnable)
    {
        _isRooted = !shouldEnable;
        if (!shouldEnable)
        {
            _enemyRigidBody.velocity = Vector2.zero;
        }
    }

    //Visualisation tool for distance and endpoints of the enemy's movement (에너미 패트롤 동선 선으로 나타내 줌,게임 플레이시 사라짐) 
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(LeftEnd.transform.position, 0.5f);
        Gizmos.DrawWireSphere(RightEnd.transform.position, 0.5f);
        Gizmos.DrawLine(LeftEnd.transform.position, RightEnd.transform.position);
    }
}
