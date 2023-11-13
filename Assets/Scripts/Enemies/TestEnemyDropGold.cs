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
        UnityEngine.Object prefabObj = null;
        UnityEngine.Object prefab = Utility.LoadObjectFromPath("Prefabs/Coin/PREF_Coin");
        Debug.Assert(prefab);
        Instantiate(prefab, gameObject.transform.position, gameObject.transform.rotation);

    }

    public void CoinRange(string name)
    {
        
    }
}
