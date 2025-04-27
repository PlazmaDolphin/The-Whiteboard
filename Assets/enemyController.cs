using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class enemyController : MonoBehaviour
{
    private List<dummyUnit> allEnemies = new List<dummyUnit>();
    void Start()
    {
        // Find all enemy units in the scene and add them to the list
        dummyUnit[] enemies = FindObjectsByType<dummyUnit>(FindObjectsSortMode.None);
        foreach (dummyUnit enemy in enemies)
        {
            if (enemy.isEnemy)
            {
                allEnemies.Add(enemy);
            }
        }
    }
    public IEnumerator drawAllPaths(){
        // For each enemy, calculate their path, draw it, and wait for it to finish before moving to the next one
        foreach (dummyUnit enemy in allEnemies){
            enemy.addSelfEnemy();
            enemy.genEnemyPenPath();
            //wait until its pen is done drawing
            yield return new WaitUntil(() => enemy.thePen.isAnimating == false);
        }
    }
}
