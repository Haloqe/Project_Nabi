using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TestEnemyDropGold : EnemyBase
{
    private Dictionary<int, SEnemyData> _enemies;
    protected override void Initialise()
    {
        _enemies = EnemyManager.Instance.GetEnemies();
        TempDie();
    }

    public void TempDie()
    {
        DropCoin();
        Destroy(gameObject);
        Debug.Log(gameObject.name + " died.");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void DropCoin()
    {
        UnityEngine.Object prefab = Utility.LoadObjectFromPath("Prefabs/Coin/PREF_Coin");
        Debug.Assert(prefab);
        Instantiate(prefab, gameObject.transform.position, gameObject.transform.rotation);

    }

    public int MinCoinRange()
    {
        //TO-DO: 몬스터에 따라서 id 1 이 아니고 특정 id넣어줘야 함.
        return _enemies[1].MinCoin; 
    }
    public int MaxCoinRange()
    { 
        //TO-DO: 몬스터에 따라서 id 1 이 아니고 특정 id넣어줘야 함.
        return _enemies[1].MaxCoin;
    }
}
