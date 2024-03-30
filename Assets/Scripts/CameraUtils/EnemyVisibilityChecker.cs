using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibilityChecker : MonoBehaviour
{
    public List<GameObject> visibleEnemies { get; private set; }

    private void Awake()
    {
        visibleEnemies = new List<GameObject>();
    }
    
    public void AddEnemyToVisibleList(GameObject enemyObj)
    {
        visibleEnemies.Add(enemyObj);
    }

    public void RemoveEnemyFromVisibleList(GameObject enemyObj)
    {
        visibleEnemies.Remove(enemyObj);
    }
}
