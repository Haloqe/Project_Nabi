using UnityEngine;

public class EnemyMovement_FollowStraightPath : EnemyMovement
{
    //적이 패트롤링 (순찰) 하는 포인트의 왼쪽 끝과 오른쪽 끝 선언, 유니티 에디터에서 드래그해서 넣어주세요
    public Transform LeftEnd;
    public Transform RightEnd;
    private Transform _TargetEnd;

    protected override void Start()
    {
        base.Start();
        _TargetEnd = RightEnd;
    }

    void Update()
    {
        if (_isRooted) return;

        // give the direction that enemy will move towards
        if(_TargetEnd == RightEnd)
        {
            _rigidBody.velocity = new Vector2(_moveSpeed, 0f);
        }
        else
        {
            _rigidBody.velocity = new Vector2(-_moveSpeed, 0f);
        }
        
        // if our enemy has reached the current point and if the current point is right end,
        // we set the current point to LeftEnd, and vice versa
        if(Vector2.Distance(transform.position, _TargetEnd.position) < 0.5f)
        {
            FlipEnemyFacing();
            _TargetEnd = _TargetEnd == RightEnd ? LeftEnd : RightEnd;
        }

    }
    private void FlipEnemyFacing()
    {
        transform.localScale = new Vector2(
            -(Mathf.Sign(_rigidBody.velocity.x)) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
    }

    //Visualisation tool for distance and endpoints of the enemy's movement (에너미 패트롤 동선 선으로 나타내 줌,게임 플레이시 사라짐) 
    private void OnDrawGizmos()
    {
        if (LeftEnd == null || RightEnd == null) return;
        Gizmos.DrawWireSphere(LeftEnd.position, 0.5f);
        Gizmos.DrawWireSphere(RightEnd.position, 0.5f);
        Gizmos.DrawLine(LeftEnd.position, RightEnd.position);
    }
}
