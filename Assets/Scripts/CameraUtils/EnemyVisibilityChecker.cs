using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibilityChecker : MonoBehaviour
{
    public List<GameObject> VisibleEnemies { get; private set; }

    private void Awake()
    {
        VisibleEnemies = new List<GameObject>();
    }
    
    public void AddEnemyToVisibleList(GameObject enemyObj)
    {
        VisibleEnemies.Add(enemyObj);
    }

    public void RemoveEnemyFromVisibleList(GameObject enemyObj)
    {
        VisibleEnemies.Remove(enemyObj);
    }
}
