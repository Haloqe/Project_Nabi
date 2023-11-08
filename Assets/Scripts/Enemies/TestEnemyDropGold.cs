using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TestEnemyDropGold : EnemyBase
{
    protected override void Initialise()
    {
        TempDie();
    }

    public void TempDie()
    {
        DropGold();
        Destroy(gameObject);
        
        Debug.Log(gameObject.name + " died.");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    protected override void DropGold()
    {
        UnityEngine.Object prefabObj = null;
        UnityEngine.Object prefab = Utility.LoadObjectFromPath("Prefabs/Coin/PREF_Coin");
        Debug.Assert(prefab);
        Instantiate(prefab, gameObject.transform.position, gameObject.transform.rotation);

    }
}
